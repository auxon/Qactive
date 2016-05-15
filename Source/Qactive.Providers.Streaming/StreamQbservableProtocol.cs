using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.Remoting.Messaging;
using System.Security;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;
using Qactive.Expressions;
using Qactive.Properties;

namespace Qactive
{
  internal sealed class StreamQbservableProtocol : QbservableProtocol<Stream, StreamMessage>, IStreamQbservableProtocol
  {
    private readonly AsyncConsumerQueue sendQ = new AsyncConsumerQueue();
    private readonly AsyncConsumerQueue receiveQ = new AsyncConsumerQueue();

    public StreamQbservableProtocol(Stream stream, IRemotingFormatter formatter, CancellationToken cancel)
    : base(stream, formatter, cancel)
    {
      sendQ.UnhandledExceptions.Subscribe(AddError);
      receiveQ.UnhandledExceptions.Subscribe(AddError);
    }

    public StreamQbservableProtocol(Stream stream, IRemotingFormatter formatter, QbservableServiceOptions serviceOptions, CancellationToken cancel)
    : base(stream, formatter, serviceOptions, cancel)
    {
      sendQ.UnhandledExceptions.Subscribe(AddError);
      receiveQ.UnhandledExceptions.Subscribe(AddError);
    }

    public Task SendAsync(byte[] buffer, int offset, int count)
    {
      return sendQ.EnqueueAsync(async () =>
      {
        try
        {
          await Source.WriteAsync(buffer, offset, count, Cancel).ConfigureAwait(false);
          await Source.FlushAsync(Cancel).ConfigureAwait(false);
        }
        catch (ObjectDisposedException ex)    // Occurred sometimes during testing upon cancellation
        {
          throw new OperationCanceledException(ex.Message, ex);
        }
      });
    }

    public Task ReceiveAsync(byte[] buffer, int offset, int count)
    {
      return receiveQ.EnqueueAsync(async () =>
      {
        try
        {
          int read = await Source.ReadAsync(buffer, offset, count, Cancel).ConfigureAwait(false);

          if (read != count)
          {
            throw new InvalidOperationException("The connection was closed without sending all of the data.");
          }
        }
        catch (ObjectDisposedException ex)    // Occurred sometimes during testing upon cancellation
        {
          throw new OperationCanceledException(ex.Message, ex);
        }
      });
    }

    protected override ClientDuplexQbservableProtocolSink<Stream, StreamMessage> CreateClientDuplexSink()
    {
      return new StreamClientDuplexQbservableProtocolSink(this);
    }

    protected override ServerDuplexQbservableProtocolSink<Stream, StreamMessage> CreateServerDuplexSink()
    {
      return new StreamServerDuplexQbservableProtocolSink(this);
    }

    protected override async Task ClientSendQueryAsync(Expression expression, object argument)
    {
      if (argument != null)
      {
        await SendMessageAsync(QbservableProtocolMessageKind.Argument, argument).ConfigureAwait(false);
      }

      var converter = new SerializableExpressionConverter();

      await SendMessageAsync(QbservableProtocolMessageKind.Subscribe, converter.Convert(expression)).ConfigureAwait(false);
    }

    protected override IObservable<TResult> ClientReceive<TResult>()
    {
      return Observable.Create<TResult>(o =>
      {
        var subscription = Observable.Create<TResult>(
          async (observer, cancel) =>
          {
            do
            {
              var message = await ReceiveMessageAsync().ConfigureAwait(false);

              switch (message.Kind)
              {
                case QbservableProtocolMessageKind.OnNext:
                  observer.OnNext(Deserialize<TResult>(message.Data));
                  break;
                case QbservableProtocolMessageKind.OnCompleted:
                  Deserialize<object>(message.Data);  // just in case data is sent, though it's unexpected.
                  observer.OnCompleted();
                  goto Return;
                case QbservableProtocolMessageKind.OnError:
                  observer.OnError(Deserialize<Exception>(message.Data));
                  goto Return;
                case QbservableProtocolMessageKind.Shutdown:
                  ShutdownWithoutResponse(GetShutdownReason(message, QbservableProtocolShutdownReason.None));
                  goto Return;
                default:
                  if (!message.Handled)
                  {
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Errors.ProtocolUnknownMessageKindFormat, message.Kind));
                  }
                  break;
              }
            }
            while (!cancel.IsCancellationRequested && !Cancel.IsCancellationRequested);

            Return:
            return () => { };
          })
          .Subscribe(o);

        return new CompositeDisposable(
          subscription,
          Disposable.Create(async () =>
          {
            try
            {
              await ShutdownAsync(QbservableProtocolShutdownReason.ClientTerminated).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
              CancelAllCommunication(ExceptionDispatchInfo.Capture(ex));
            }
          }));
      });
    }

    protected override async Task<Tuple<Expression, object>> ServerReceiveQueryAsync()
    {
      do
      {
        var message = await ReceiveMessageAsync().ConfigureAwait(false);

        object argument;

        if (message.Kind == QbservableProtocolMessageKind.Argument)
        {
          argument = Deserialize<object>(message.Data);

          message = await ReceiveMessageAsync().ConfigureAwait(false);
        }
        else
        {
          argument = null;
        }

        if (message.Kind == QbservableProtocolMessageKind.Subscribe)
        {
          var converter = new SerializableExpressionConverter();

          return Tuple.Create(SerializableExpressionConverter.Convert(Deserialize<SerializableExpression>(message.Data)), argument);
        }
        else if (ServerHandleClientShutdown(message))
        {
          throw new OperationCanceledException();
        }
        else if (!message.Handled)
        {
          throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Errors.ProtocolExpectedMessageSubscribeFormat, message.Kind));
        }
      }
      while (true);
    }

    protected override Task ServerSendAsync(NotificationKind kind, object data)
    {
      var messageKind = GetMessageKind(kind);

      if (messageKind == QbservableProtocolMessageKind.OnCompleted)
      {
        return SendMessageAsync(new StreamMessage(messageKind));
      }
      else
      {
        return SendMessageAsync(messageKind, data);
      }
    }

    private static QbservableProtocolShutdownReason GetShutdownReason(StreamMessage message, QbservableProtocolShutdownReason defaultReason)
    {
      if (message.Data.Length > 0)
      {
        return (QbservableProtocolShutdownReason)message.Data[0];
      }
      else
      {
        return defaultReason;
      }
    }

    protected override bool ServerHandleClientShutdown(StreamMessage message)
    {
      if (message.Kind == QbservableProtocolMessageKind.Shutdown)
      {
        var reason = GetShutdownReason(message, QbservableProtocolShutdownReason.ClientTerminated);

        ShutdownWithoutResponse(reason);

        return true;
      }

      return false;
    }

    protected override Task ShutdownCoreAsync()
    {
      return SendMessageAsync(new StreamMessage(QbservableProtocolMessageKind.Shutdown, (byte)ShutdownReason));
    }

    private Task SendMessageAsync(QbservableProtocolMessageKind kind, object data)
    {
      long length;
      return SendMessageAsync(new StreamMessage(kind, Serialize(data, out length), length));
    }

    protected override Task SendMessageCoreAsync(StreamMessage message)
    {
      var lengthBytes = BitConverter.GetBytes(message.Length);

      var buffer = new byte[1L + lengthBytes.Length + message.Length];

      buffer[0] = (byte)message.Kind;

      Array.Copy(lengthBytes, 0, buffer, 1, lengthBytes.Length);

      if (message.Length > 0)
      {
        Array.Copy(message.Data, 0L, buffer, 1L + lengthBytes.Length, message.Length);
      }

      return SendAsync(buffer, 0, buffer.Length);
    }

    protected override async Task<StreamMessage> ReceiveMessageCoreAsync()
    {
      var buffer = new byte[1024];

      await ReceiveAsync(buffer, 0, 9).ConfigureAwait(false);

      var messageKind = (QbservableProtocolMessageKind)buffer[0];
      var length = BitConverter.ToInt64(buffer, 1);

      if (length > 0)
      {
        using (var stream = new MemoryStream((int)length))
        {
          long remainder = length;

          do
          {
            int count = Math.Min(buffer.Length, remainder > int.MaxValue ? int.MaxValue : (int)remainder);

            await ReceiveAsync(buffer, 0, count).ConfigureAwait(false);

            stream.Write(buffer, 0, count);

            remainder -= count;
          }
          while (remainder > 0);

          return new StreamMessage(messageKind, stream.ToArray());
        }
      }

      return new StreamMessage(messageKind, new byte[0]);
    }

    private static QbservableProtocolMessageKind GetMessageKind(NotificationKind kind)
    {
      switch (kind)
      {
        case NotificationKind.OnNext:
          return QbservableProtocolMessageKind.OnNext;
        case NotificationKind.OnCompleted:
          return QbservableProtocolMessageKind.OnCompleted;
        case NotificationKind.OnError:
          return QbservableProtocolMessageKind.OnError;
        default:
          throw new ArgumentOutOfRangeException("kind");
      }
    }

    private void SendDuplexMessage(DuplexStreamMessage message)
    {
      SendMessageAsync(message).Wait(Cancel);
    }

    internal async void SendDuplexMessageAsync(DuplexStreamMessage message)
    {
      try
      {
        await SendMessageAsync(message).ConfigureAwait(false);
      }
      catch (OperationCanceledException)
      {
      }
      catch (Exception ex)
      {
        CancelAllCommunication(ExceptionDispatchInfo.Capture(ex));
      }
    }

    internal object ServerSendDuplexMessage(int clientId, Func<DuplexCallbackId, DuplexStreamMessage> messageFactory)
    {
      return ServerSendDuplexMessage(clientId, messageFactory, sink => sink.RegisterInvokeCallback);
    }

    internal object ServerSendEnumeratorDuplexMessage(int clientId, Func<DuplexCallbackId, DuplexStreamMessage> messageFactory)
    {
      return ServerSendDuplexMessage(clientId, messageFactory, sink => sink.RegisterEnumeratorCallback);
    }

    private object ServerSendDuplexMessage(
      int clientId,
      Func<DuplexCallbackId, DuplexStreamMessage> messageFactory,
      Func<IServerDuplexQbservableProtocolSink, Func<int, Action<object>, Action<ExceptionDispatchInfo>, DuplexCallbackId>> registrationSelector)
    {
      var waitForResponse = new ManualResetEventSlim(false);

      ExceptionDispatchInfo error = null;
      object result = null;

      var duplexSink = FindSink<IServerDuplexQbservableProtocolSink>();

      var id = registrationSelector(duplexSink)(
        clientId,
        value =>
        {
          result = value;
          waitForResponse.Set();
        },
        ex =>
        {
          error = ex;
          waitForResponse.Set();
        });

      var message = messageFactory(id);

      SendDuplexMessage(message);

      waitForResponse.Wait(Cancel);

      if (error != null)
      {
        error.Throw();
      }

      return result;
    }

    internal IDisposable ServerSendSubscribeDuplexMessage(
      int clientId,
      Action<object> onNext,
      Action<ExceptionDispatchInfo> onError,
      Action onCompleted)
    {
      /*
      In testing, the observer permanently blocked incoming data from the client unless concurrency was introduced.
      The order of events were as follows: 

      1. The server received an OnNext notification from an I/O completion port.
      2. The server pushed the value to the observer passed into DuplexCallbackObservable.Subscribe, without introducing concurrency.
      3. The query provider continued executing the serialized query on the current thread.
      4. The query at this point required a synchronous invocation to a client-side member (i.e., duplex enabled).
      5. The server sent the new invocation to the client and then blocked the current thread waiting for an async response.
      
      Since the current thread was an I/O completion port (received for OnNext), it seems that blocking it prevented any 
      further data from being received, even via the Stream.AsyncRead method. Apparently the only solution is to ensure 
      that observable callbacks occur on pooled threads to prevent I/O completion ports from inadvertantly being blocked.
      */
      var scheduler = TaskPoolScheduler.Default;

      var duplexSink = FindSink<IServerDuplexQbservableProtocolSink>();

      var registration = duplexSink.RegisterObservableCallbacks(
        clientId,
        value => scheduler.Schedule(value, (_, v) => { onNext(v); return Disposable.Empty; }),
        ex => scheduler.Schedule(ex, (_, e) => { onError(e); return Disposable.Empty; }),
        () => scheduler.Schedule(onCompleted),
        subscriptionId => SendDuplexMessageAsync(DuplexStreamMessage.CreateDisposeSubscription(subscriptionId, this)));

      var id = registration.Item1;
      var subscription = registration.Item2;

      var message = DuplexStreamMessage.CreateSubscribe(id, this);

      SendDuplexMessage(message);

      return subscription;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#", Justification = "Reviewed")]
    public byte[] Serialize(object data, out long length)
    {
      using (var memory = new MemoryStream())
      {
        if (data == null)
        {
          memory.WriteByte(1);
        }
        else
        {
          memory.WriteByte(0);

          new SecurityPermission(SecurityPermissionFlag.SerializationFormatter).Assert();

          try
          {
            Formatter.Serialize(memory, data);
          }
          finally
          {
            CodeAccessPermission.RevertAssert();
          }
        }

        length = memory.Length;

        return memory.GetBuffer();
      }
    }

    public T Deserialize<T>(byte[] data)
    {
      return Deserialize<T>(data, offset: 0);
    }

    public T Deserialize<T>(byte[] data, int offset)
    {
      if (data == null || data.Length == 0)
      {
        if (offset > 0)
        {
          throw new InvalidOperationException();
        }

        return (T)(object)null;
      }

      using (var memory = new MemoryStream(data))
      {
        memory.Position = offset;

        var isNullDataFlag = memory.ReadByte();

        Contract.Assume(isNullDataFlag == 0 || isNullDataFlag == 1);

        if (isNullDataFlag == 1)
        {
          return (T)(object)null;
        }
        else
        {
          new SecurityPermission(SecurityPermissionFlag.SerializationFormatter).Assert();

          try
          {
            return (T)Formatter.Deserialize(memory);
          }
          finally
          {
            CodeAccessPermission.RevertAssert();
          }
        }
      }
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        sendQ.Dispose();
        receiveQ.Dispose();
      }

      base.Dispose(disposing);
    }
  }
}
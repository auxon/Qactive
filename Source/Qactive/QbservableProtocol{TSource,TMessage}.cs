using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Threading.Tasks;
using Qactive.Expressions;
using Qactive.Properties;

namespace Qactive
{
  public abstract class QbservableProtocol<TSource, TMessage> : QbservableProtocol<TSource>
    where TMessage : IProtocolMessage
  {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    protected IList<QbservableProtocolSink<TSource, TMessage>> Sinks => sinks;

    private readonly List<QbservableProtocolSink<TSource, TMessage>> sinks = new List<QbservableProtocolSink<TSource, TMessage>>();

    protected QbservableProtocol(TSource source, IRemotingFormatter formatter, CancellationToken cancel)
      : base(source, formatter, cancel)
    {
      Contract.Ensures(IsClient);
    }

    protected QbservableProtocol(TSource source, IRemotingFormatter formatter, QbservableServiceOptions serviceOptions, CancellationToken cancel)
      : base(source, formatter, serviceOptions, cancel)
    {
      Contract.Ensures(!IsClient);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    protected virtual IEnumerable<QbservableProtocolSink<TSource, TMessage>> CreateClientSinks()
    {
      // for derived types
      yield break;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    protected virtual IEnumerable<QbservableProtocolSink<TSource, TMessage>> CreateServerSinks()
    {
      // for derived types
      yield break;
    }

    protected abstract ClientDuplexQbservableProtocolSink<TSource, TMessage> CreateClientDuplexSink();

    protected abstract ServerDuplexQbservableProtocolSink<TSource, TMessage> CreateServerDuplexSink();

    internal sealed override IClientDuplexQbservableProtocolSink CreateClientDuplexSinkInternal()
      => CreateClientDuplexSink();

    internal sealed override IServerDuplexQbservableProtocolSink CreateServerDuplexSinkInternal()
      => CreateServerDuplexSink();

    protected override async Task ClientSendQueryAsync(Expression expression, object argument)
    {
      if (argument != null)
      {
        await SendMessageAsync(CreateMessage(QbservableProtocolMessageKind.Argument, argument)).ConfigureAwait(false);
      }

      var converter = new SerializableExpressionConverter();

      await SendMessageAsync(CreateMessage(QbservableProtocolMessageKind.Subscribe, converter.Convert(expression))).ConfigureAwait(false);
    }

    protected override async Task<Tuple<Expression, object>> ServerReceiveQueryAsync()
    {
      do
      {
        var message = await ReceiveMessageAsync().ConfigureAwait(false);

        object argument;

        if (message.Kind == QbservableProtocolMessageKind.Argument)
        {
          argument = Deserialize<object>(message);

          message = await ReceiveMessageAsync().ConfigureAwait(false);
        }
        else
        {
          argument = null;
        }

        if (message.Kind == QbservableProtocolMessageKind.Subscribe)
        {
          var converter = new SerializableExpressionConverter();

          return Tuple.Create(SerializableExpressionConverter.Convert(Deserialize<SerializableExpression>(message)), argument);
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
      var messageKind = kind.AsMessageKind();

      if (messageKind == QbservableProtocolMessageKind.OnCompleted)
      {
        return SendMessageAsync(CreateMessage(messageKind));
      }
      else
      {
        return SendMessageAsync(CreateMessage(messageKind, data));
      }
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
                  observer.OnNext(Deserialize<TResult>(message));
                  break;
                case QbservableProtocolMessageKind.OnCompleted:
                  Deserialize<object>(message);  // just in case data is sent, though it's unexpected.
                  observer.OnCompleted();
                  goto Return;
                case QbservableProtocolMessageKind.OnError:
                  observer.OnError(Deserialize<Exception>(message));
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

    protected virtual bool ServerHandleClientShutdown(TMessage message)
    {
      if (message.Kind == QbservableProtocolMessageKind.Shutdown)
      {
        var reason = GetShutdownReason(message, QbservableProtocolShutdownReason.ClientTerminated);

        ShutdownWithoutResponse(reason);

        return true;
      }

      return false;
    }

    protected abstract QbservableProtocolShutdownReason GetShutdownReason(TMessage message, QbservableProtocolShutdownReason defaultReason);

    protected abstract TMessage CreateMessage(QbservableProtocolMessageKind kind);

    protected abstract TMessage CreateMessage(QbservableProtocolMessageKind kind, object data);

    protected abstract T Deserialize<T>(TMessage message);

    internal sealed override async Task ServerReceiveAsync()
    {
      while (!Cancel.IsCancellationRequested)
      {
        var message = await ReceiveMessageAsync().ConfigureAwait(false);

        if (ServerHandleClientShutdown(message))
        {
          break;
        }
        else if (!message.Handled)
        {
          throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Errors.ProtocolUnknownMessageKindFormat, message));
        }
      }
    }

    public async Task SendMessageAsync(TMessage message)
    {
      message = await ApplySinksForSending(message).ConfigureAwait(false);

      await SendMessageCoreAsync(message).ConfigureAwait(false);
    }

    public async Task SendMessageSafeAsync(TMessage message)
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

    protected async Task<TMessage> ReceiveMessageAsync()
    {
      var message = await ReceiveMessageCoreAsync().ConfigureAwait(false);

      return await ApplySinksForReceiving(message).ConfigureAwait(false);
    }

    protected abstract Task SendMessageCoreAsync(TMessage message);

    protected abstract Task<TMessage> ReceiveMessageCoreAsync();

    public object ServerSendDuplexMessage(int clientId, Func<DuplexCallbackId, TMessage> messageFactory)
      => ServerSendDuplexMessage(clientId, messageFactory, sink => sink.RegisterInvokeCallback);

    public object ServerSendEnumeratorDuplexMessage(int clientId, Func<DuplexCallbackId, TMessage> messageFactory)
      => ServerSendDuplexMessage(clientId, messageFactory, sink => sink.RegisterEnumeratorCallback);

    private object ServerSendDuplexMessage(
      int clientId,
      Func<DuplexCallbackId, TMessage> messageFactory,
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

      SendMessageSafeAsync(message).Wait(Cancel);

      waitForResponse.Wait(Cancel);

      if (error != null)
      {
        error.Throw();
      }

      return result;
    }

    public IDisposable ServerSendSubscribeDuplexMessage(
     int clientId,
     Action<object> onNext,
     Action<ExceptionDispatchInfo> onError,
     Action onCompleted,
     Action<int> dispose)
    {
      var duplexSink = FindSink<IServerDuplexQbservableProtocolSink>();

      var registration = duplexSink.RegisterObservableCallbacks(clientId, onNext, onError, onCompleted, dispose);

      var id = registration.Item1;
      var subscription = registration.Item2;

      SendMessageSafeAsync(CreateSubscribeDuplexMessage(id)).Wait(Cancel);

      return subscription;
    }

    protected abstract TMessage CreateSubscribeDuplexMessage(DuplexCallbackId id);

    internal sealed override async Task InitializeSinksAsync()
    {
      if (IsClient)
      {
        sinks.AddRange(CreateClientSinks());
      }
      else
      {
        sinks.AddRange(CreateServerSinks());

        if (ServiceOptions.EnableDuplex)
        {
          sinks.Add(CreateServerDuplexSink());
        }
      }

      foreach (var sink in sinks)
      {
        await sink.InitializeAsync(this, Cancel).ConfigureAwait(false);
      }
    }

    private async Task<TMessage> ApplySinksForSending(TMessage message)
    {
      foreach (var sink in sinks)
      {
        message = await sink.SendingAsync(message, Cancel).ConfigureAwait(false);
      }

      return message;
    }

    private async Task<TMessage> ApplySinksForReceiving(TMessage message)
    {
      foreach (var sink in sinks)
      {
        message = await sink.ReceivingAsync(message, Cancel).ConfigureAwait(false);
      }

      return message;
    }

    public sealed override TSink FindSink<TSink>()
    {
      return sinks.OfType<TSink>().FirstOrDefault();
    }

    public sealed override TSink GetOrAddSink<TSink>(Func<TSink> createSink)
    {
      var sink = FindSink<TSink>();

      if (sink == null)
      {
        sink = createSink();
        sinks.Add((QbservableProtocolSink<TSource, TMessage>)(object)sink);
      }

      return sink;
    }
  }
}
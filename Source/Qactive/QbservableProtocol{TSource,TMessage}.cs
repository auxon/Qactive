using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Threading.Tasks;
using Qactive.Expressions;
using Qactive.Properties;

namespace Qactive
{
  [ContractClass(typeof(QbservableProtocolContract<,>))]
  public abstract class QbservableProtocol<TSource, TMessage> : QbservableProtocol<TSource>
    where TMessage : IProtocolMessage
  {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    protected IList<QbservableProtocolSink<TSource, TMessage>> Sinks { get; } = new List<QbservableProtocolSink<TSource, TMessage>>();

    protected QbservableProtocol(TSource source, IRemotingFormatter formatter, CancellationToken cancel)
      : base(source, formatter, cancel)
    {
      Contract.Requires(source != null);
      Contract.Requires(formatter != null);
      Contract.Ensures(IsClient);
    }

    protected QbservableProtocol(TSource source, IRemotingFormatter formatter, QbservableServiceOptions serviceOptions, CancellationToken cancel)
      : base(source, formatter, serviceOptions, cancel)
    {
      Contract.Requires(source != null);
      Contract.Requires(formatter != null);
      Contract.Requires(serviceOptions != null);
      Contract.Ensures(!IsClient);
    }

    [ContractInvariantMethod]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
    private void ObjectInvariant()
    {
      Contract.Invariant(Sinks != null);
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

      await SendMessageAsync(CreateMessage(QbservableProtocolMessageKind.Subscribe, converter.TryConvert(expression))).ConfigureAwait(false);
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

          return Tuple.Create(SerializableExpressionConverter.TryConvert(Deserialize<SerializableExpression>(message)), argument);
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
      return Observable.Create<TResult>(
        async (observer, cancel) =>
        {
          var deserializeAnonymousTypeOnNext = TryGetAnonymousTypeDeserializer<TResult>();

          do
          {
            var message = await ReceiveMessageAsync().ConfigureAwait(false);

            switch (message.Kind)
            {
              case QbservableProtocolMessageKind.OnNext:
                observer.OnNext(deserializeAnonymousTypeOnNext != null
                              ? deserializeAnonymousTypeOnNext(message)
                              : Deserialize<TResult>(message));
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
        .Finally(async () =>
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
        });
    }

    private Func<TMessage, TResult> TryGetAnonymousTypeDeserializer<TResult>()
    {
      Func<TMessage, TResult> deserializer = null;
      var resultType = typeof(TResult);
      ConstructorInfo constructor;
      IList<string> propertiesInConstructorOrder;

      if (resultType.IsNotPublic
        && resultType.GetCustomAttributes(typeof(CompilerGenerated), inherit: false) != null
        && (constructor = resultType.GetConstructors().SingleOrDefault()) != null
        && (propertiesInConstructorOrder =
              (from property in resultType.GetProperties()
               join parameter in constructor.GetParameters()
               on property.Name equals parameter.Name
               orderby parameter.Position
               select property.Name)
               .ToList())
           .Count == resultType.GetProperties().Length)
      {
        deserializer = message =>
        {
          var result = Deserialize<CompilerGenerated>(message);

          return (TResult)constructor.Invoke(
            (from property in propertiesInConstructorOrder
             select result.GetProperty<object>(property))
             .ToArray());
        };
      }

      return deserializer;
    }

    protected virtual bool ServerHandleClientShutdown(TMessage message)
    {
      Contract.Requires(message != null);

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
      Contract.Requires(message != null);

      message = await ApplySinksForSending(message).ConfigureAwait(false);

      await SendMessageCoreAsync(message).ConfigureAwait(false);
    }

    public async Task SendMessageSafeAsync(TMessage message)
    {
      Contract.Requires(message != null);

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
    {
      Contract.Requires(messageFactory != null);

      return ServerSendDuplexMessage(clientId, messageFactory, sink => sink.RegisterInvokeCallback);
    }

    public object ServerSendEnumeratorDuplexMessage(int clientId, Func<DuplexCallbackId, TMessage> messageFactory)
    {
      Contract.Requires(messageFactory != null);

      return ServerSendDuplexMessage(clientId, messageFactory, sink => sink.RegisterEnumeratorCallback);
    }

    private object ServerSendDuplexMessage(
      int clientId,
      Func<DuplexCallbackId, TMessage> messageFactory,
      Func<IServerDuplexQbservableProtocolSink, Func<int, Action<object>, Action<ExceptionDispatchInfo>, DuplexCallbackId>> registrationSelector)
    {
      Contract.Requires(messageFactory != null);
      Contract.Requires(registrationSelector != null);

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
      Contract.Requires(onNext != null);
      Contract.Requires(onError != null);
      Contract.Requires(onCompleted != null);
      Contract.Requires(dispose != null);

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
        foreach (var sink in CreateClientSinks())
        {
          Sinks.Add(sink);
        }
      }
      else
      {
        foreach (var sink in CreateServerSinks())
        {
          Sinks.Add(sink);
        }

        if (ServiceOptions.EnableDuplex)
        {
          Sinks.Add(CreateServerDuplexSink());
        }
      }

      foreach (var sink in Sinks)
      {
        await sink.InitializeAsync(this, Cancel).ConfigureAwait(false);
      }
    }

    private async Task<TMessage> ApplySinksForSending(TMessage message)
    {
      Contract.Requires(message != null);

      foreach (var sink in Sinks)
      {
        message = await sink.SendingAsync(message, Cancel).ConfigureAwait(false);
      }

      return message;
    }

    private async Task<TMessage> ApplySinksForReceiving(TMessage message)
    {
      Contract.Requires(message != null);

      foreach (var sink in Sinks)
      {
        message = await sink.ReceivingAsync(message, Cancel).ConfigureAwait(false);
      }

      return message;
    }

    public sealed override TSink FindSink<TSink>()
    {
      return Sinks.OfType<TSink>().FirstOrDefault();
    }

    public sealed override TSink GetOrAddSink<TSink>(Func<TSink> createSink)
    {
      var sink = FindSink<TSink>();

      if (sink == null)
      {
        sink = createSink();
        Sinks.Add((QbservableProtocolSink<TSource, TMessage>)(object)sink);
      }

      return sink;
    }
  }

  [ContractClassFor(typeof(QbservableProtocol<,>))]
  internal abstract class QbservableProtocolContract<TSource, TMessage> : QbservableProtocol<TSource, TMessage>
    where TMessage : IProtocolMessage
  {
    protected QbservableProtocolContract(TSource source, IRemotingFormatter formatter, CancellationToken cancel)
      : base(source, formatter, cancel)
    {
    }

    protected override ClientDuplexQbservableProtocolSink<TSource, TMessage> CreateClientDuplexSink()
    {
      Contract.Ensures(Contract.Result<ClientDuplexQbservableProtocolSink<TSource, TMessage>>() != null);
      return null;
    }

    protected override ServerDuplexQbservableProtocolSink<TSource, TMessage> CreateServerDuplexSink()
    {
      Contract.Ensures(Contract.Result<ServerDuplexQbservableProtocolSink<TSource, TMessage>>() != null);
      return null;
    }

    protected override QbservableProtocolShutdownReason GetShutdownReason(TMessage message, QbservableProtocolShutdownReason defaultReason)
    {
      Contract.Requires(message != null);
      return default(QbservableProtocolShutdownReason);
    }

    protected override TMessage CreateMessage(QbservableProtocolMessageKind kind)
    {
      Contract.Ensures(Contract.Result<TMessage>() != null);
      return default(TMessage);
    }

    protected override TMessage CreateMessage(QbservableProtocolMessageKind kind, object data)
    {
      Contract.Ensures(Contract.Result<TMessage>() != null);
      return default(TMessage);
    }

    protected override T Deserialize<T>(TMessage message)
    {
      Contract.Requires(message != null);
      return default(T);
    }

    protected override Task SendMessageCoreAsync(TMessage message)
    {
      Contract.Requires(message != null);
      return null;
    }

    protected override TMessage CreateSubscribeDuplexMessage(DuplexCallbackId id)
    {
      Contract.Ensures(Contract.Result<TMessage>() != null);
      return default(TMessage);
    }
  }
}
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Reactive.Disposables;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Qactive.Properties;

namespace Qactive
{
  [ContractClass(typeof(ClientDuplexQbservableProtocolSinkContract<,>))]
  public abstract class ClientDuplexQbservableProtocolSink<TSource, TMessage> : QbservableProtocolSink<TSource, TMessage>, IClientDuplexQbservableProtocolSink
    where TMessage : IProtocolMessage
  {
    private readonly ConcurrentDictionary<int, IInvokeDuplexCallback> invokeCallbacks = new ConcurrentDictionary<int, IInvokeDuplexCallback>();
    private readonly ConcurrentDictionary<int, IEnumerableDuplexCallback> enumerableCallbacks = new ConcurrentDictionary<int, IEnumerableDuplexCallback>();
    private readonly ConcurrentDictionary<int, IObservableDuplexCallback> obsevableCallbacks = new ConcurrentDictionary<int, IObservableDuplexCallback>();
    private readonly ConcurrentDictionary<int, NamedEnumerator> enumerators = new ConcurrentDictionary<int, NamedEnumerator>();
    private readonly ConcurrentDictionary<int, NamedDisposable> subscriptions = new ConcurrentDictionary<int, NamedDisposable>();
    private int lastCallbackId;
    private int lastObservableId;
    private int lastSubscriptionId;
    private int lastEnumerableId;
    private int lastEnumeratorId;

    protected abstract QbservableProtocol<TSource, TMessage> Protocol { get; }

    IQbservableProtocol IClientDuplexQbservableProtocolSink.Protocol => Protocol;

    public override Task<TMessage> SendingAsync(TMessage message, CancellationToken cancel)
    {
      return Task.FromResult(message);
    }

    public override Task<TMessage> ReceivingAsync(TMessage message, CancellationToken cancel)
    {
      var duplexMessage = TryParseDuplexMessage(message);

      if (duplexMessage != null && duplexMessage is TMessage)
      {
        message = (TMessage)duplexMessage;

        switch (duplexMessage.Kind)
        {
          case QbservableProtocolMessageKind.DuplexInvoke:
            Invoke(duplexMessage.Id, (object[])duplexMessage.Value);
            break;
          case QbservableProtocolMessageKind.DuplexSubscribe:
            Subscribe(duplexMessage.Id);
            break;
          case QbservableProtocolMessageKind.DuplexDisposeSubscription:
            DisposeSubscription(duplexMessage.Id);
            break;
          case QbservableProtocolMessageKind.DuplexGetEnumerator:
            GetEnumerator(duplexMessage.Id);
            break;
          case QbservableProtocolMessageKind.DuplexMoveNext:
            MoveNext(duplexMessage.Id);
            break;
          case QbservableProtocolMessageKind.DuplexResetEnumerator:
            ResetEnumerator(duplexMessage.Id);
            break;
          case QbservableProtocolMessageKind.DuplexDisposeEnumerator:
            DisposeEnumerator(duplexMessage.Id);
            break;
          default:
            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Errors.ProtocolUnknownMessageKindFormat, duplexMessage.Kind));
        }

        duplexMessage.Handled = true;
      }

      return Task.FromResult(message);
    }

    protected abstract IDuplexProtocolMessage TryParseDuplexMessage(TMessage message);

    public int RegisterInvokeCallback(IInvokeDuplexCallback invoke)
    {
      var id = Interlocked.Increment(ref lastCallbackId);

      if (!invokeCallbacks.TryAdd(id, invoke))
      {
        throw new InvalidOperationException(Errors.ProtocolDuplicateDuplexId);
      }

      return id;
    }

    public IEnumerableDuplexCallback RegisterEnumerableCallback(Func<int, IEnumerableDuplexCallback> callbackFactory)
    {
      var id = Interlocked.Increment(ref lastEnumerableId);
      var enumerable = callbackFactory(id);

      if (!enumerableCallbacks.TryAdd(id, enumerable))
      {
        throw new InvalidOperationException(Errors.ProtocolDuplicateDuplexId);
      }

      return enumerable;
    }

    public IObservableDuplexCallback RegisterObservableCallback(Func<int, IObservableDuplexCallback> callbackFactory)
    {
      var id = Interlocked.Increment(ref lastObservableId);
      var observable = callbackFactory(id);

      if (!obsevableCallbacks.TryAdd(id, observable))
      {
        throw new InvalidOperationException(Errors.ProtocolDuplicateDuplexId);
      }

      return observable;
    }

    private int RegisterSubscription(NamedDisposable subscription)
    {
      Contract.Requires(subscription != null);
      Contract.Ensures(Contract.Result<int>() >= 0);

      var id = Interlocked.Increment(ref lastSubscriptionId);

      if (!subscriptions.TryAdd(id, subscription))
      {
        throw new InvalidOperationException(Errors.ProtocolDuplicateDuplexId);
      }

      return id;
    }

    private int RegisterEnumerator(NamedEnumerator enumerator)
    {
      Contract.Requires(enumerator != null);
      Contract.Ensures(Contract.Result<int>() >= 0);

      var id = Interlocked.Increment(ref lastEnumeratorId);

      if (!enumerators.TryAdd(id, enumerator))
      {
        throw new InvalidOperationException(Errors.ProtocolDuplicateDuplexId);
      }

      return id;
    }

    private IEnumerableDuplexCallback GetEnumerable(int clientId)
    {
      Contract.Ensures(Contract.Result<IEnumerableDuplexCallback>() != null);

      IEnumerableDuplexCallback enumerable;

      if (!enumerableCallbacks.TryGetValue(clientId, out enumerable))
      {
        throw new InvalidOperationException(Errors.ProtocolInvalidDuplexId);
      }

      return enumerable;
    }

    private NamedEnumerator GetEnumerator(int enumeratorId)
    {
      Contract.Ensures(Contract.Result<IEnumerator>() != null);

      NamedEnumerator enumerator;

      if (enumerators.TryGetValue(enumeratorId, out enumerator))
      {
        return enumerator;
      }

      // The enumerator may be missing if Disposed has been called or if MoveNext is called again after it has already returned false.
      return null;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "There is no meaningful way to handle exceptions here other than passing them to a handler, and we cannot let them leave this context because they will be missed.")]
    protected void Invoke(DuplexCallbackId id, object[] arguments)
    {
      Contract.Requires(arguments != null);

      IInvokeDuplexCallback callback;

      if (!invokeCallbacks.TryGetValue(id.ClientId, out callback))
      {
        throw new InvalidOperationException(Errors.ProtocolInvalidDuplexId);
      }

#if TRACE
      var sourceId = GetSourceId(id);

      Log.Invoking(callback.Name, arguments, false, sourceId, LogMessages.Received);
#endif

      object result;
      try
      {
        result = callback.Invoke(arguments);
      }
      catch (Exception ex)
      {
        SendError(id, ExceptionDispatchInfo.Capture(ex));
        return;
      }

      var duplex = result as DuplexCallback;

      if (duplex != null)
      {
        duplex.SetClientProtocol(Protocol);
      }

#if TRACE
      Log.Invoked(callback.Name, arguments, result, false, sourceId, LogMessages.Sending);
#endif

      SendResponse(id, result);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "There is no meaningful way to handle exceptions here other than passing them to a handler, and we cannot let them leave this context because they will be missed.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The subscription local is registered with this sink object.")]
    protected void Subscribe(DuplexCallbackId id)
    {
      IObservableDuplexCallback observable;

      if (!obsevableCallbacks.TryGetValue(id.ClientId, out observable))
      {
        throw new InvalidOperationException(Errors.ProtocolInvalidDuplexId);
      }

#if TRACE
      var sourceId = GetSourceId(id);

      Log.Subscribing(observable.Name, false, true, sourceId, LogMessages.Received);
#endif

      var subscription = new SingleAssignmentDisposable();

      var subscriptionId = RegisterSubscription(new NamedDisposable(observable.Name, subscription));

      try
      {
        subscription.Disposable = observable.Subscribe(
          value => SendOnNext(observable.Name, id, value),
          ex => SendOnError(observable.Name, id, ExceptionDispatchInfo.Capture(ex)),
          () => SendOnCompleted(observable.Name, id));
      }
      catch (Exception ex)
      {
        SendOnError(observable.Name, id, ExceptionDispatchInfo.Capture(ex));
        return;
      }

#if TRACE
      Log.Subscribed(observable.Name, false, true, sourceId, LogMessages.Sending);
#endif

      SendSubscribeResponse(id, subscriptionId);
    }

    protected void DisposeSubscription(DuplexCallbackId id)
    {
      NamedDisposable subscription;

      if (!subscriptions.TryGetValue(id.ClientId, out subscription))
      {
        throw new InvalidOperationException(Errors.ProtocolInvalidDuplexId);
      }

#if TRACE
      var sourceId = GetSourceId(id);

      Log.Unsubscribing(subscription.Name, false, true, sourceId, LogMessages.Received);
#endif

      subscription.Dispose();

#if TRACE
      Log.Unsubscribed(subscription.Name, false, false, sourceId, LogMessages.Sending);
#endif
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "There is no meaningful way to handle exceptions here other than passing them to a handler, and we cannot let them leave this context because they will be missed.")]
    protected void GetEnumerator(DuplexCallbackId id)
    {
      var enumerable = GetEnumerable(id.ClientId);

#if TRACE
      var sourceId = GetSourceId(id);

      Log.Enumerating(enumerable.Name, false, sourceId, LogMessages.Received);
#endif

      IEnumerator enumerator;
      try
      {
        enumerator = enumerable.GetEnumerator();
      }
      catch (Exception ex)
      {
        SendGetEnumeratorError(id, ExceptionDispatchInfo.Capture(ex));
        return;
      }

      var enumeratorId = RegisterEnumerator(new NamedEnumerator(enumerable.Name, enumerator));

      SendGetEnumeratorResponse(id, enumeratorId);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "There is no meaningful way to handle exceptions here other than passing them to a handler, and we cannot let them leave this context because they will be missed.")]
    protected void MoveNext(DuplexCallbackId id)
    {
      var enumerator = GetEnumerator(id.ClientId);

#if TRACE
      var sourceId = GetSourceId(id);

      if (enumerator != null)
      {
        Log.MoveNext(enumerator.Name, false, sourceId, LogMessages.Received);
      }
#endif

      object current;
      bool result;
      try
      {
        if (enumerator != null && enumerator.MoveNext())
        {
          result = true;
          current = enumerator.Current;
        }
        else
        {
          result = false;
          current = null;
        }
      }
      catch (Exception ex)
      {
        SendEnumeratorError(id, ExceptionDispatchInfo.Capture(ex));
        return;
      }

#if TRACE
      if (result)
      {
        Log.Current(enumerator.Name, current, false, sourceId, LogMessages.Sending);
      }
#endif

      SendEnumeratorResponse(id, result, current);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "There is no meaningful way to handle exceptions here other than passing them to a handler, and we cannot let them leave this context because they will be missed.")]
    protected void ResetEnumerator(DuplexCallbackId id)
    {
      var enumerator = GetEnumerator(id.ClientId);

      try
      {
        enumerator.Reset();
      }
      catch (Exception ex)
      {
        SendEnumeratorError(id, ExceptionDispatchInfo.Capture(ex));
      }
    }

    protected void DisposeEnumerator(DuplexCallbackId id)
    {
      var enumerator = GetEnumerator(id.ClientId);

#if TRACE
      var sourceId = GetSourceId(id);

      Log.Enumerated(enumerator.Name, false, sourceId, LogMessages.Received);
#endif

      var disposable = enumerator.Decorated as IDisposable;

      if (disposable != null)
      {
        disposable.Dispose();
      }
    }

    public virtual async void SendOnNext(string name, DuplexCallbackId id, object value)
    {
#if TRACE
      var sourceId = GetSourceId(id);

      Log.OnNext(name, value, false, false, sourceId, LogMessages.Sending);
#endif

      await Protocol.SendMessageSafeAsync(CreateOnNext(id, value)).ConfigureAwait(false);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Error", Justification = "Standard naming in Rx.")]
    public virtual async void SendOnError(string name, DuplexCallbackId id, ExceptionDispatchInfo error)
    {
#if TRACE
      var sourceId = GetSourceId(id);

      Log.OnError(name, error.SourceException, false, false, sourceId, LogMessages.Sending);
#endif

      await Protocol.SendMessageSafeAsync(CreateOnError(id, error)).ConfigureAwait(false);
    }

    public virtual async void SendOnCompleted(string name, DuplexCallbackId id)
    {
#if TRACE
      var sourceId = GetSourceId(id);

      Log.OnCompleted(name, false, false, sourceId, LogMessages.Sending);
#endif

      await Protocol.SendMessageSafeAsync(CreateOnCompleted(id)).ConfigureAwait(false);
    }

    protected virtual async void SendResponse(DuplexCallbackId id, object result)
      => await Protocol.SendMessageSafeAsync(CreateResponse(id, result)).ConfigureAwait(false);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Error", Justification = "Standard naming in Rx.")]
    protected virtual async void SendError(DuplexCallbackId id, ExceptionDispatchInfo error)
      => await Protocol.SendMessageSafeAsync(CreateErrorResponse(id, error)).ConfigureAwait(false);

    protected virtual async void SendSubscribeResponse(DuplexCallbackId id, int clientSubscriptionId)
      => await Protocol.SendMessageSafeAsync(CreateSubscribeResponse(id, clientSubscriptionId)).ConfigureAwait(false);

    protected virtual async void SendGetEnumeratorResponse(DuplexCallbackId id, int clientEnumeratorId)
      => await Protocol.SendMessageSafeAsync(CreateGetEnumeratorResponse(id, clientEnumeratorId)).ConfigureAwait(false);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Error", Justification = "Standard naming in Rx.")]
    protected virtual async void SendGetEnumeratorError(DuplexCallbackId id, ExceptionDispatchInfo error)
    {
      Contract.Requires(error != null);

      await Protocol.SendMessageSafeAsync(CreateGetEnumeratorError(id, error)).ConfigureAwait(false);
    }

    protected virtual async void SendEnumeratorResponse(DuplexCallbackId id, bool result, object current)
      => await Protocol.SendMessageSafeAsync(CreateEnumeratorResponse(id, result, current)).ConfigureAwait(false);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Error", Justification = "Standard naming in Rx.")]
    protected virtual async void SendEnumeratorError(DuplexCallbackId id, ExceptionDispatchInfo error)
    {
      Contract.Requires(error != null);

      await Protocol.SendMessageSafeAsync(CreateEnumeratorError(id, error)).ConfigureAwait(false);
    }

    protected abstract TMessage CreateOnNext(DuplexCallbackId id, object value);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Error", Justification = "Standard naming in Rx.")]
    protected abstract TMessage CreateOnError(DuplexCallbackId id, ExceptionDispatchInfo error);

    protected abstract TMessage CreateOnCompleted(DuplexCallbackId id);

    protected abstract TMessage CreateResponse(DuplexCallbackId id, object result);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Error", Justification = "Standard naming in Rx.")]
    protected abstract TMessage CreateErrorResponse(DuplexCallbackId id, ExceptionDispatchInfo error);

    protected abstract TMessage CreateSubscribeResponse(DuplexCallbackId id, int clientSubscriptionId);

    protected abstract TMessage CreateGetEnumeratorResponse(DuplexCallbackId id, int clientEnumeratorId);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Error", Justification = "Standard naming in Rx.")]
    protected abstract TMessage CreateGetEnumeratorError(DuplexCallbackId id, ExceptionDispatchInfo error);

    protected abstract TMessage CreateEnumeratorResponse(DuplexCallbackId id, bool result, object current);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Error", Justification = "Standard naming in Rx.")]
    protected abstract TMessage CreateEnumeratorError(DuplexCallbackId id, ExceptionDispatchInfo error);

    private string GetSourceId(DuplexCallbackId id)
      => Protocol.ClientId + " " + id;
  }

  [ContractClassFor(typeof(ClientDuplexQbservableProtocolSink<,>))]
  internal abstract class ClientDuplexQbservableProtocolSinkContract<TSource, TMessage> : ClientDuplexQbservableProtocolSink<TSource, TMessage>
     where TMessage : IProtocolMessage
  {
    protected override QbservableProtocol<TSource, TMessage> Protocol
    {
      get
      {
        Contract.Ensures(Contract.Result<QbservableProtocol<TSource, TMessage>>() != null);
        return null;
      }
    }

    protected override IDuplexProtocolMessage TryParseDuplexMessage(TMessage message)
    {
      Contract.Requires(message != null);
      return null;
    }

    protected override TMessage CreateOnNext(DuplexCallbackId id, object value)
    {
      Contract.Ensures(Contract.Result<TMessage>() != null);
      return default(TMessage);
    }

    protected override TMessage CreateOnError(DuplexCallbackId id, ExceptionDispatchInfo error)
    {
      Contract.Requires(error != null);
      Contract.Ensures(Contract.Result<TMessage>() != null);
      return default(TMessage);
    }

    protected override TMessage CreateOnCompleted(DuplexCallbackId id)
    {
      Contract.Ensures(Contract.Result<TMessage>() != null);
      return default(TMessage);
    }

    protected override TMessage CreateResponse(DuplexCallbackId id, object result)
    {
      Contract.Ensures(Contract.Result<TMessage>() != null);
      return default(TMessage);
    }

    protected override TMessage CreateErrorResponse(DuplexCallbackId id, ExceptionDispatchInfo error)
    {
      Contract.Requires(error != null);
      Contract.Ensures(Contract.Result<TMessage>() != null);
      return default(TMessage);
    }

    protected override TMessage CreateSubscribeResponse(DuplexCallbackId id, int clientSubscriptionId)
    {
      Contract.Ensures(Contract.Result<TMessage>() != null);
      return default(TMessage);
    }

    protected override TMessage CreateGetEnumeratorResponse(DuplexCallbackId id, int clientEnumeratorId)
    {
      Contract.Ensures(Contract.Result<TMessage>() != null);
      return default(TMessage);
    }

    protected override TMessage CreateGetEnumeratorError(DuplexCallbackId id, ExceptionDispatchInfo error)
    {
      Contract.Requires(error != null);
      Contract.Ensures(Contract.Result<TMessage>() != null);
      return default(TMessage);
    }

    protected override TMessage CreateEnumeratorResponse(DuplexCallbackId id, bool result, object current)
    {
      Contract.Ensures(Contract.Result<TMessage>() != null);
      return default(TMessage);
    }

    protected override TMessage CreateEnumeratorError(DuplexCallbackId id, ExceptionDispatchInfo error)
    {
      Contract.Requires(error != null);
      Contract.Ensures(Contract.Result<TMessage>() != null);
      return default(TMessage);
    }
  }
}
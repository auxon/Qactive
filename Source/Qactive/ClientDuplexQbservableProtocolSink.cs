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
    private readonly ConcurrentDictionary<int, Func<object[], object>> invokeCallbacks = new ConcurrentDictionary<int, Func<object[], object>>();
    private readonly ConcurrentDictionary<int, Func<IEnumerator>> enumerableCallbacks = new ConcurrentDictionary<int, Func<IEnumerator>>();
    private readonly ConcurrentDictionary<int, Func<int, IDisposable>> obsevableCallbacks = new ConcurrentDictionary<int, Func<int, IDisposable>>();
    private readonly ConcurrentDictionary<int, IEnumerator> enumerators = new ConcurrentDictionary<int, IEnumerator>();
    private readonly ConcurrentDictionary<int, IDisposable> subscriptions = new ConcurrentDictionary<int, IDisposable>();
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
            DisposeSubscription(duplexMessage.Id.ClientId);
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
            DisposeEnumerator(duplexMessage.Id.ClientId);
            break;
          default:
            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Errors.ProtocolUnknownMessageKindFormat, duplexMessage.Kind));
        }

        duplexMessage.Handled = true;
      }

      return Task.FromResult(message);
    }

    protected abstract IDuplexProtocolMessage TryParseDuplexMessage(TMessage message);

    public int RegisterInvokeCallback(Func<object[], object> callback)
    {
      var id = Interlocked.Increment(ref lastCallbackId);

      if (!invokeCallbacks.TryAdd(id, callback))
      {
        throw new InvalidOperationException(Errors.ProtocolDuplicateDuplexId);
      }

      return id;
    }

    public int RegisterEnumerableCallback(Func<IEnumerator> getEnumerator)
    {
      var id = Interlocked.Increment(ref lastEnumerableId);

      if (!enumerableCallbacks.TryAdd(id, getEnumerator))
      {
        throw new InvalidOperationException(Errors.ProtocolDuplicateDuplexId);
      }

      return id;
    }

    public int RegisterObservableCallback(Func<int, IDisposable> subscribe)
    {
      var id = Interlocked.Increment(ref lastObservableId);

      if (!obsevableCallbacks.TryAdd(id, subscribe))
      {
        throw new InvalidOperationException(Errors.ProtocolDuplicateDuplexId);
      }

      return id;
    }

    private int RegisterSubscription(IDisposable subscription)
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

    private int RegisterEnumerator(IEnumerator enumerator)
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

    private Func<IEnumerator> GetEnumerable(int clientId)
    {
      Contract.Ensures(Contract.Result<Func<IEnumerator>>() != null);

      Func<IEnumerator> enumerable;

      if (!enumerableCallbacks.TryGetValue(clientId, out enumerable))
      {
        throw new InvalidOperationException(Errors.ProtocolInvalidDuplexId);
      }

      return enumerable;
    }

    private IEnumerator GetEnumerator(int enumeratorId)
    {
      Contract.Ensures(Contract.Result<IEnumerator>() != null);

      IEnumerator enumerator;

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

      Func<object[], object> callback;

      if (!invokeCallbacks.TryGetValue(id.ClientId, out callback))
      {
        throw new InvalidOperationException(Errors.ProtocolInvalidDuplexId);
      }

      object result;
      try
      {
        result = callback(arguments);
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

      SendResponse(id, result);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "There is no meaningful way to handle exceptions here other than passing them to a handler, and we cannot let them leave this context because they will be missed.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The subscription local is registered with this sink object.")]
    protected void Subscribe(DuplexCallbackId id)
    {
      Func<int, IDisposable> subscribe;

      if (!obsevableCallbacks.TryGetValue(id.ClientId, out subscribe))
      {
        throw new InvalidOperationException(Errors.ProtocolInvalidDuplexId);
      }

      var subscription = new SingleAssignmentDisposable();

      var subscriptionId = RegisterSubscription(subscription);

      try
      {
        subscription.Disposable = subscribe(id.ServerId);
      }
      catch (Exception ex)
      {
        SendOnError(id, ExceptionDispatchInfo.Capture(ex));
        return;
      }

      SendSubscribeResponse(id, subscriptionId);
    }

    protected void DisposeSubscription(int subscriptionId)
    {
      IDisposable subscription;

      if (!subscriptions.TryGetValue(subscriptionId, out subscription))
      {
        throw new InvalidOperationException(Errors.ProtocolInvalidDuplexId);
      }

      subscription.Dispose();
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "There is no meaningful way to handle exceptions here other than passing them to a handler, and we cannot let them leave this context because they will be missed.")]
    protected void GetEnumerator(DuplexCallbackId id)
    {
      var enumerable = GetEnumerable(id.ClientId);

      IEnumerator enumerator;
      try
      {
        enumerator = enumerable();
      }
      catch (Exception ex)
      {
        SendGetEnumeratorError(id, ExceptionDispatchInfo.Capture(ex));
        return;
      }

      var enumeratorId = RegisterEnumerator(enumerator);

      SendGetEnumeratorResponse(id, enumeratorId);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "There is no meaningful way to handle exceptions here other than passing them to a handler, and we cannot let them leave this context because they will be missed.")]
    protected void MoveNext(DuplexCallbackId id)
    {
      var enumerator = GetEnumerator(id.ClientId);

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

    protected void DisposeEnumerator(int enumeratorId)
    {
      var disposable = GetEnumerator(enumeratorId) as IDisposable;

      if (disposable != null)
      {
        disposable.Dispose();
      }
    }

    public virtual async void SendOnNext(DuplexCallbackId id, object value) => await Protocol.SendMessageSafeAsync(CreateOnNext(id, value)).ConfigureAwait(false);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Error", Justification = "Standard naming in Rx.")]
    public virtual async void SendOnError(DuplexCallbackId id, ExceptionDispatchInfo error)
      => await Protocol.SendMessageSafeAsync(CreateOnError(id, error)).ConfigureAwait(false);

    public virtual async void SendOnCompleted(DuplexCallbackId id)
      => await Protocol.SendMessageSafeAsync(CreateOnCompleted(id)).ConfigureAwait(false);

    protected virtual async void SendResponse(DuplexCallbackId id, object result)
      => await Protocol.SendMessageSafeAsync(CreateResponse(id, result)).ConfigureAwait(false);

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

    protected abstract TMessage CreateOnError(DuplexCallbackId id, ExceptionDispatchInfo error);

    protected abstract TMessage CreateOnCompleted(DuplexCallbackId id);

    protected abstract TMessage CreateResponse(DuplexCallbackId id, object result);

    protected abstract TMessage CreateErrorResponse(DuplexCallbackId id, ExceptionDispatchInfo error);

    protected abstract TMessage CreateSubscribeResponse(DuplexCallbackId id, int clientSubscriptionId);

    protected abstract TMessage CreateGetEnumeratorResponse(DuplexCallbackId id, int clientEnumeratorId);

    protected abstract TMessage CreateGetEnumeratorError(DuplexCallbackId id, ExceptionDispatchInfo error);

    protected abstract TMessage CreateEnumeratorResponse(DuplexCallbackId id, bool result, object current);

    protected abstract TMessage CreateEnumeratorError(DuplexCallbackId id, ExceptionDispatchInfo error);
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
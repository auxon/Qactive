using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Reactive.Disposables;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Qactive.Properties;

namespace Qactive
{
  [ContractClass(typeof(ServerDuplexQbservableProtocolSinkContract<,>))]
  public abstract class ServerDuplexQbservableProtocolSink<TSource, TMessage> : QbservableProtocolSink<TSource, TMessage>, IServerDuplexQbservableProtocolSink
    where TMessage : IProtocolMessage
  {
    private readonly ConcurrentDictionary<DuplexCallbackId, Tuple<Action<object>, Action<ExceptionDispatchInfo>>> invokeCallbacks = new ConcurrentDictionary<DuplexCallbackId, Tuple<Action<object>, Action<ExceptionDispatchInfo>>>();
    private readonly ConcurrentDictionary<DuplexCallbackId, Tuple<Action<object>, Action<ExceptionDispatchInfo>>> enumeratorCallbacks = new ConcurrentDictionary<DuplexCallbackId, Tuple<Action<object>, Action<ExceptionDispatchInfo>>>();
    private readonly ConcurrentDictionary<DuplexCallbackId, Tuple<Action<object>, Action<ExceptionDispatchInfo>, Action, Action<int>>> observableCallbacks = new ConcurrentDictionary<DuplexCallbackId, Tuple<Action<object>, Action<ExceptionDispatchInfo>, Action, Action<int>>>();
    private readonly Dictionary<DuplexCallbackId, int?> subscriptions = new Dictionary<DuplexCallbackId, int?>();
    private int lastCallbackId;
    private int lastEnumeratorId;
    private int lastObservableId;

    protected abstract QbservableProtocol<TSource, TMessage> Protocol { get; }

    IQbservableProtocol IServerDuplexQbservableProtocolSink.Protocol => Protocol;

    [ContractInvariantMethod]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
    private void ObjectInvariant()
    {
      Contract.Invariant(invokeCallbacks != null);
      Contract.Invariant(enumeratorCallbacks != null);
      Contract.Invariant(observableCallbacks != null);
      Contract.Invariant(subscriptions != null);
    }

    public override Task<TMessage> SendingAsync(TMessage message, CancellationToken cancel)
      => Task.FromResult(message);

    public override Task<TMessage> ReceivingAsync(TMessage message, CancellationToken cancel)
    {
      var duplexMessage = TryParseDuplexMessage(message);

      if (duplexMessage != null && duplexMessage is TMessage)
      {
        message = (TMessage)duplexMessage;

        switch (duplexMessage.Kind)
        {
          case QbservableProtocolMessageKind.DuplexResponse:
            HandleResponse(duplexMessage.Id, duplexMessage.Value);
            break;
          case QbservableProtocolMessageKind.DuplexErrorResponse:
            HandleErrorResponse(duplexMessage.Id, duplexMessage.Error);
            break;
          case QbservableProtocolMessageKind.DuplexSubscribeResponse:
            HandleSubscribeResponse(duplexMessage.Id, (int)duplexMessage.Value);
            break;
          case QbservableProtocolMessageKind.DuplexGetEnumeratorResponse:
            HandleGetEnumeratorResponse(duplexMessage.Id, (int)duplexMessage.Value);
            break;
          case QbservableProtocolMessageKind.DuplexGetEnumeratorErrorResponse:
            HandleGetEnumeratorErrorResponse(duplexMessage.Id, duplexMessage.Error);
            break;
          case QbservableProtocolMessageKind.DuplexEnumeratorResponse:
            HandleEnumeratorResponse(duplexMessage.Id, (Tuple<bool, object>)duplexMessage.Value);
            break;
          case QbservableProtocolMessageKind.DuplexEnumeratorErrorResponse:
            HandleEnumeratorErrorResponse(duplexMessage.Id, duplexMessage.Error);
            break;
          case QbservableProtocolMessageKind.DuplexOnNext:
            HandleOnNext(duplexMessage.Id, duplexMessage.Value);
            break;
          case QbservableProtocolMessageKind.DuplexOnCompleted:
            HandleOnCompleted(duplexMessage.Id);
            break;
          case QbservableProtocolMessageKind.DuplexOnError:
            HandleOnError(duplexMessage.Id, duplexMessage.Error);
            break;
          default:
            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Errors.ProtocolUnknownMessageKindFormat, duplexMessage.Kind));
        }

        duplexMessage.Handled = true;
      }

      return Task.FromResult(message);
    }

    protected abstract IDuplexProtocolMessage TryParseDuplexMessage(TMessage message);

    public DuplexCallbackId RegisterInvokeCallback(int clientId, Action<object> callback, Action<ExceptionDispatchInfo> onError)
    {
      var serverId = Interlocked.Increment(ref lastCallbackId);

      var id = new DuplexCallbackId(clientId, serverId);

      if (!invokeCallbacks.TryAdd(id, Tuple.Create(callback, onError)))
      {
        throw new InvalidOperationException(Errors.ProtocolDuplicateDuplexIdForInvoke);
      }

      return id;
    }

    public DuplexCallbackId RegisterEnumeratorCallback(int clientId, Action<object> callback, Action<ExceptionDispatchInfo> onError)
    {
      var serverId = Interlocked.Increment(ref lastEnumeratorId);

      var id = new DuplexCallbackId(clientId, serverId);

      if (!enumeratorCallbacks.TryAdd(id, Tuple.Create(callback, onError)))
      {
        throw new InvalidOperationException(Errors.ProtocolDuplicateDuplexIdForEnumerator);
      }

      return id;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Returned to caller.")]
    public Tuple<DuplexCallbackId, IDisposable> RegisterObservableCallbacks(int clientId, Action<object> onNext, Action<ExceptionDispatchInfo> onError, Action onCompleted, Action<int> dispose)
    {
      var serverId = Interlocked.Increment(ref lastObservableId);

      var id = new DuplexCallbackId(clientId, serverId);

      var actions = Tuple.Create(onNext, onError, onCompleted, dispose);

      if (!observableCallbacks.TryAdd(id, actions))
      {
        throw new InvalidOperationException(Errors.ProtocolDuplicateDuplexIdForObservable);
      }

      return Tuple.Create(
        id,
        Disposable.Create(() =>
        {
          lock (actions)
          {
            int? clientSubscriptionId = null;

            if (TryGetOrAddSubscriptionOneTime(id, ref clientSubscriptionId))
            {
              Contract.Assume(clientSubscriptionId.HasValue);   // Disposable.Create ensures that this code only runs once

              actions.Item4(clientSubscriptionId.Value);
            }
          }
        }));
    }

    private Tuple<Action<object>, Action<ExceptionDispatchInfo>> GetInvokeCallbacks(DuplexCallbackId id)
    {
      Contract.Ensures(Contract.Result<Tuple<Action<object>, Action<ExceptionDispatchInfo>>>() != null);

      Tuple<Action<object>, Action<ExceptionDispatchInfo>> actions;

      if (!invokeCallbacks.TryGetValue(id, out actions))
      {
        throw new InvalidOperationException(Errors.ProtocolInvalidDuplexIdForInvoke);
      }

      return actions;
    }

    private Tuple<Action<object>, Action<ExceptionDispatchInfo>> GetEnumeratorCallbacks(DuplexCallbackId id)
    {
      Contract.Ensures(Contract.Result<Tuple<Action<object>, Action<ExceptionDispatchInfo>>>() != null);

      Tuple<Action<object>, Action<ExceptionDispatchInfo>> actions;

      if (!enumeratorCallbacks.TryGetValue(id, out actions))
      {
        throw new InvalidOperationException(Errors.ProtocolInvalidDuplexIdForEnumerator);
      }

      return actions;
    }

    private bool TryGetOrAddSubscriptionOneTime(DuplexCallbackId id, ref int? clientSubscriptionId)
    {
      int? s;
      if (subscriptions.TryGetValue(id, out s))
      {
        subscriptions.Remove(id);

        clientSubscriptionId = s;
        return true;
      }
      else
      {
        subscriptions.Add(id, clientSubscriptionId);
      }

      return false;
    }

    private void TryInvokeObservableCallback(
      DuplexCallbackId id,
      Action<Tuple<Action<object>, Action<ExceptionDispatchInfo>, Action, Action<int>>> action)
    {
      Contract.Requires(action != null);

      Tuple<Action<object>, Action<ExceptionDispatchInfo>, Action, Action<int>> callbacks;

      if (observableCallbacks.TryGetValue(id, out callbacks))
      {
        action(callbacks);
      }

      /* It's acceptable for the callbacks to be missing due to a race condition between the
       * client sending notifications and the server disposing of the subscription, which causes
       * the callbacks to be removed.
       */
    }

    public virtual IDisposable Subscribe(int clientId, Action<object> onNext, Action<ExceptionDispatchInfo> onError, Action onCompleted)
    {
      return Protocol.ServerSendSubscribeDuplexMessage(
        clientId,
        onNext,
        onError,
        onCompleted,
        async subscriptionId => await Protocol.SendMessageSafeAsync(CreateDisposeSubscription(subscriptionId)).ConfigureAwait(false));
    }

    public virtual object Invoke(int clientId, object[] arguments)
      => Protocol.ServerSendDuplexMessage(clientId, id => CreateInvoke(id, arguments));

    public virtual int GetEnumerator(int clientId)
      => (int)Protocol.ServerSendDuplexMessage(clientId, CreateGetEnumerator);

    public virtual Tuple<bool, object> MoveNext(int enumeratorId)
      => (Tuple<bool, object>)Protocol.ServerSendEnumeratorDuplexMessage(enumeratorId, CreateMoveNext);

    public virtual void ResetEnumerator(int enumeratorId)
      => Protocol.ServerSendEnumeratorDuplexMessage(enumeratorId, CreateResetEnumerator);

    public virtual async void DisposeEnumerator(int enumeratorId)
      => await Protocol.SendMessageSafeAsync(CreateDisposeEnumerator(enumeratorId)).ConfigureAwait(false);

    protected abstract TMessage CreateDisposeSubscription(int subscriptionId);

    protected abstract TMessage CreateInvoke(DuplexCallbackId clientId, object[] arguments);

    protected abstract TMessage CreateGetEnumerator(DuplexCallbackId enumeratorId);

    protected abstract TMessage CreateMoveNext(DuplexCallbackId enumeratorId);

    protected abstract TMessage CreateResetEnumerator(DuplexCallbackId enumeratorId);

    protected abstract TMessage CreateDisposeEnumerator(int enumeratorId);

    protected void HandleResponse(DuplexCallbackId id, object value)
      => GetInvokeCallbacks(id).Item1(value);

    protected void HandleErrorResponse(DuplexCallbackId id, ExceptionDispatchInfo error)
    {
      Contract.Requires(error != null);

      GetInvokeCallbacks(id).Item2(error);
    }

    protected void HandleGetEnumeratorResponse(DuplexCallbackId id, int clientEnumeratorId)
      => HandleResponse(id, clientEnumeratorId);

    protected void HandleGetEnumeratorErrorResponse(DuplexCallbackId id, ExceptionDispatchInfo error)
    {
      Contract.Requires(error != null);

      HandleErrorResponse(id, error);
    }

    protected void HandleEnumeratorResponse(DuplexCallbackId id, Tuple<bool, object> result)
    {
      Contract.Requires(result != null);

      GetEnumeratorCallbacks(id).Item1(result);
    }

    protected void HandleEnumeratorErrorResponse(DuplexCallbackId id, ExceptionDispatchInfo error)
    {
      Contract.Requires(error != null);

      GetEnumeratorCallbacks(id).Item2(error);
    }

    protected void HandleSubscribeResponse(DuplexCallbackId id, int clientSubscriptionId)
    {
      Tuple<Action<object>, Action<ExceptionDispatchInfo>, Action, Action<int>> actions;

      if (!observableCallbacks.TryGetValue(id, out actions))
      {
        throw new InvalidOperationException(Errors.ProtocolInvalidDuplexIdForObservable);
      }

      lock (actions)
      {
        int? inputOnly = clientSubscriptionId;
        if (TryGetOrAddSubscriptionOneTime(id, ref inputOnly))
        {
          subscriptions.Remove(id);

          Tuple<Action<object>, Action<ExceptionDispatchInfo>, Action, Action<int>> ignored;
          observableCallbacks.TryRemove(id, out ignored);

          actions.Item4(clientSubscriptionId);
        }
      }
    }

    protected void HandleOnNext(DuplexCallbackId id, object result)
      => TryInvokeObservableCallback(id, actions => actions.Item1(result));

    protected void HandleOnCompleted(DuplexCallbackId id)
      => TryInvokeObservableCallback(id, actions => actions.Item3());

    protected void HandleOnError(DuplexCallbackId id, ExceptionDispatchInfo error)
    {
      Contract.Requires(error != null);

      TryInvokeObservableCallback(id, actions => actions.Item2(error));
    }
  }

  [ContractClassFor(typeof(ServerDuplexQbservableProtocolSink<,>))]
  internal abstract class ServerDuplexQbservableProtocolSinkContract<TSource, TMessage> : ServerDuplexQbservableProtocolSink<TSource, TMessage>
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

    protected override TMessage CreateDisposeSubscription(int subscriptionId)
    {
      Contract.Ensures(Contract.Result<TMessage>() != null);
      return default(TMessage);
    }

    protected override TMessage CreateInvoke(DuplexCallbackId clientId, object[] arguments)
    {
      Contract.Ensures(Contract.Result<TMessage>() != null);
      return default(TMessage);
    }

    protected override TMessage CreateGetEnumerator(DuplexCallbackId enumeratorId)
    {
      Contract.Ensures(Contract.Result<TMessage>() != null);
      return default(TMessage);
    }

    protected override TMessage CreateMoveNext(DuplexCallbackId enumeratorId)
    {
      Contract.Ensures(Contract.Result<TMessage>() != null);
      return default(TMessage);
    }

    protected override TMessage CreateResetEnumerator(DuplexCallbackId enumeratorId)
    {
      Contract.Ensures(Contract.Result<TMessage>() != null);
      return default(TMessage);
    }

    protected override TMessage CreateDisposeEnumerator(int enumeratorId)
    {
      Contract.Ensures(Contract.Result<TMessage>() != null);
      return default(TMessage);
    }
  }
}
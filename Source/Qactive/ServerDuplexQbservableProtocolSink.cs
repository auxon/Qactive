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
    private readonly ConcurrentDictionary<DuplexCallbackId, Tuple<Action<DuplexCallbackId, object>, Action<DuplexCallbackId, ExceptionDispatchInfo>, Action<DuplexCallbackId>, Action<DuplexCallbackId>, Action<DuplexCallbackId>>> observableCallbacks = new ConcurrentDictionary<DuplexCallbackId, Tuple<Action<DuplexCallbackId, object>, Action<DuplexCallbackId, ExceptionDispatchInfo>, Action<DuplexCallbackId>, Action<DuplexCallbackId>, Action<DuplexCallbackId>>>();
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
    public Tuple<DuplexCallbackId, IDisposable> RegisterObservableCallbacks(int clientId, Action<DuplexCallbackId, object> onNext, Action<DuplexCallbackId, ExceptionDispatchInfo> onError, Action<DuplexCallbackId> onCompleted, Action<DuplexCallbackId> dispose, Action<DuplexCallbackId> subscribed)
    {
      var serverId = Interlocked.Increment(ref lastObservableId);

      var id = new DuplexCallbackId(clientId, serverId);

      var actions = Tuple.Create(onNext, onError, onCompleted, dispose, subscribed);

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

              actions.Item4(new DuplexCallbackId(clientSubscriptionId.Value, id.ServerId));
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
      Action<Tuple<Action<DuplexCallbackId, object>, Action<DuplexCallbackId, ExceptionDispatchInfo>, Action<DuplexCallbackId>, Action<DuplexCallbackId>, Action<DuplexCallbackId>>> action)
    {
      Contract.Requires(action != null);

      Tuple<Action<DuplexCallbackId, object>, Action<DuplexCallbackId, ExceptionDispatchInfo>, Action<DuplexCallbackId>, Action<DuplexCallbackId>, Action<DuplexCallbackId>> callbacks;

      if (observableCallbacks.TryGetValue(id, out callbacks))
      {
        action(callbacks);
      }

      /* It's acceptable for the callbacks to be missing due to a race condition between the
       * client sending notifications and the server disposing of the subscription, which causes
       * the callbacks to be removed.
       */
    }

    public virtual IDisposable Subscribe(string name, int clientId, Action<object> onNext, Action<ExceptionDispatchInfo> onError, Action onCompleted)
    {
      return Protocol.ServerSendSubscribeDuplexMessage(
        clientId,
        id =>
        {
#if TRACE
          var sourceId = GetSourceId(id);

          Log.Subscribing(name, true, false, sourceId, LogMessages.Sending);
#endif

          return CreateSubscribe(id);
        },
        id =>
        {
#if TRACE
          var sourceId = GetSourceId(id);

          Log.Subscribed(name, true, true, sourceId, LogMessages.Received);
#endif
        },
        (id, value) =>
        {
#if TRACE
          var sourceId = GetSourceId(id);

          Log.OnNext(name, value, true, true, sourceId, LogMessages.Received);
#endif

          onNext(value);
        },
        (id, ex) =>
        {
#if TRACE
          var sourceId = GetSourceId(id);

          Log.OnError(name, ex.SourceException, true, true, sourceId, LogMessages.Received);
#endif

          onError(ex);
        },
        id =>
        {
#if TRACE
          var sourceId = GetSourceId(id);

          Log.OnCompleted(name, true, true, sourceId, LogMessages.Received);
#endif

          onCompleted();
        },
        async id =>
        {
#if TRACE
          var sourceId = GetSourceId(id);

          Log.Unsubscribing(name, true, false, sourceId, LogMessages.Sending);
#endif

          await Protocol.SendMessageSafeAsync(CreateDisposeSubscription(id.ClientId)).ConfigureAwait(false);

#if TRACE
          Log.Unsubscribed(name, true, true, sourceId, LogMessages.Received);
#endif
        });
    }

    public virtual object Invoke(string name, int clientId, object[] arguments)
    {
      object sourceId = null;

      var result = Protocol.ServerSendDuplexMessage(
        clientId,
        id =>
        {
#if TRACE
          sourceId = GetSourceId(id);

          Log.Invoking(name, arguments, true, sourceId, LogMessages.Sending);
#endif

          return CreateInvoke(id, arguments);
        });

      Log.Invoked(name, arguments, result, true, sourceId, LogMessages.Received);

      return result;
    }

    public virtual int GetEnumerator(string name, int clientId)
    {
      return (int)Protocol.ServerSendDuplexMessage(
        clientId,
        id =>
        {
#if TRACE
          var sourceId = GetSourceId(id);

          Log.Enumerating(name, true, sourceId, LogMessages.Sending);
#endif

          return CreateGetEnumerator(id);
        });
    }

    public virtual Tuple<bool, object> MoveNext(string name, int enumeratorId)
    {
      object sourceId = null;

      var result = (Tuple<bool, object>)Protocol.ServerSendEnumeratorDuplexMessage(enumeratorId,
        id =>
        {
#if TRACE
          sourceId = GetSourceId(id);

          Log.MoveNext(name, true, sourceId, LogMessages.Sending);
#endif

          return CreateMoveNext(id);
        });

#if TRACE
      if (result.Item1)
      {
        Log.Current(name, result.Item2, true, sourceId, LogMessages.Received);
      }
#endif

      return result;
    }

    public virtual void ResetEnumerator(string name, int enumeratorId)
      => Protocol.ServerSendEnumeratorDuplexMessage(enumeratorId, CreateResetEnumerator);

    public virtual async void DisposeEnumerator(string name, int enumeratorId)
    {
      var message = CreateDisposeEnumerator(enumeratorId);

#if TRACE
      var duplex = message as IDuplexProtocolMessage;
      var sourceId = GetSourceId(duplex?.Id ?? enumeratorId);

      Log.Enumerated(name, true, sourceId, LogMessages.Sending);
#endif

      try
      {
        await Protocol.SendMessageSafeAsync(message).ConfigureAwait(false);
      }
      catch (Exception ex)
      {
        Fail(ExceptionDispatchInfo.Capture(ex));
      }
    }

    protected abstract TMessage CreateSubscribe(DuplexCallbackId clientId);

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
      Tuple<Action<DuplexCallbackId, object>, Action<DuplexCallbackId, ExceptionDispatchInfo>, Action<DuplexCallbackId>, Action<DuplexCallbackId>, Action<DuplexCallbackId>> actions;

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

          Tuple<Action<DuplexCallbackId, object>, Action<DuplexCallbackId, ExceptionDispatchInfo>, Action<DuplexCallbackId>, Action<DuplexCallbackId>, Action<DuplexCallbackId>> ignored;
          observableCallbacks.TryRemove(id, out ignored);

          actions.Item4(new DuplexCallbackId(clientSubscriptionId, id.ServerId));
        }
        else
        {
          actions.Item5(new DuplexCallbackId(clientSubscriptionId, id.ServerId));
        }
      }
    }

    protected void HandleOnNext(DuplexCallbackId id, object result)
      => TryInvokeObservableCallback(id, actions => actions.Item1(id, result));

    protected void HandleOnCompleted(DuplexCallbackId id)
      => TryInvokeObservableCallback(id, actions => actions.Item3(id));

    protected void HandleOnError(DuplexCallbackId id, ExceptionDispatchInfo error)
    {
      Contract.Requires(error != null);

      TryInvokeObservableCallback(id, actions => actions.Item2(id, error));
    }

    private string GetSourceId(DuplexCallbackId id)
      => Protocol.ClientId + " " + id;
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

    protected override TMessage CreateSubscribe(DuplexCallbackId clientId)
    {
      Contract.Ensures(Contract.Result<TMessage>() != null);
      return default(TMessage);
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
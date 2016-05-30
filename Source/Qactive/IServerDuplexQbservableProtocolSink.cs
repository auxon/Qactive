using System;
using System.Diagnostics.Contracts;
using System.Runtime.ExceptionServices;

namespace Qactive
{
  [ContractClass(typeof(IServerDuplexQbservableProtocolSinkContract))]
  public interface IServerDuplexQbservableProtocolSink
  {
    IQbservableProtocol Protocol { get; }

    DuplexCallbackId RegisterInvokeCallback(int clientId, Action<object> callback, Action<ExceptionDispatchInfo> onError);

    DuplexCallbackId RegisterEnumeratorCallback(int clientId, Action<object> callback, Action<ExceptionDispatchInfo> onError);

    Tuple<DuplexCallbackId, IDisposable> RegisterObservableCallbacks(int clientId, Action<DuplexCallbackId, object> onNext, Action<DuplexCallbackId, ExceptionDispatchInfo> onError, Action<DuplexCallbackId> onCompleted, Action<DuplexCallbackId> dispose, Action<DuplexCallbackId> subscribed);

    object Invoke(string name, int clientId, object[] arguments);

    IDisposable Subscribe(string name, int clientId, Action<object> onNext, Action<ExceptionDispatchInfo> onError, Action onCompleted);

    int GetEnumerator(string name, int clientId);

    Tuple<bool, object> MoveNext(string name, int enumeratorId);

    void ResetEnumerator(string name, int enumeratorId);

    void DisposeEnumerator(string name, int enumeratorId);
  }

  [ContractClassFor(typeof(IServerDuplexQbservableProtocolSink))]
  internal abstract class IServerDuplexQbservableProtocolSinkContract : IServerDuplexQbservableProtocolSink
  {
    public IQbservableProtocol Protocol
    {
      get
      {
        Contract.Ensures(Contract.Result<IQbservableProtocol>() != null);
        return null;
      }
    }

    public DuplexCallbackId RegisterInvokeCallback(int clientId, Action<object> callback, Action<ExceptionDispatchInfo> onError)
    {
      Contract.Requires(callback != null);
      Contract.Requires(onError != null);
      return default(DuplexCallbackId);
    }

    public DuplexCallbackId RegisterEnumeratorCallback(int clientId, Action<object> callback, Action<ExceptionDispatchInfo> onError)
    {
      Contract.Requires(callback != null);
      Contract.Requires(onError != null);
      return default(DuplexCallbackId);
    }

    public Tuple<DuplexCallbackId, IDisposable> RegisterObservableCallbacks(int clientId, Action<DuplexCallbackId, object> onNext, Action<DuplexCallbackId, ExceptionDispatchInfo> onError, Action<DuplexCallbackId> onCompleted, Action<DuplexCallbackId> dispose, Action<DuplexCallbackId> subscribed)
    {
      Contract.Requires(onNext != null);
      Contract.Requires(onError != null);
      Contract.Requires(onCompleted != null);
      Contract.Requires(dispose != null);
      Contract.Requires(subscribed != null);
      Contract.Ensures(Contract.Result<Tuple<DuplexCallbackId, IDisposable>>() != null);
      return null;
    }

    public object Invoke(string name, int clientId, object[] arguments)
    {
      Contract.Requires(!string.IsNullOrEmpty(name));
      return null;
    }

    public IDisposable Subscribe(string name, int clientId, Action<object> onNext, Action<ExceptionDispatchInfo> onError, Action onCompleted)
    {
      Contract.Requires(!string.IsNullOrEmpty(name));
      Contract.Requires(onNext != null);
      Contract.Requires(onError != null);
      Contract.Requires(onCompleted != null);
      Contract.Ensures(Contract.Result<IDisposable>() != null);
      return null;
    }

    public int GetEnumerator(string name, int clientId)
    {
      Contract.Requires(!string.IsNullOrEmpty(name));
      return 0;
    }

    public Tuple<bool, object> MoveNext(string name, int enumeratorId)
    {
      Contract.Requires(!string.IsNullOrEmpty(name));
      Contract.Ensures(Contract.Result<Tuple<bool, object>>() != null);
      return null;
    }

    public void ResetEnumerator(string name, int enumeratorId)
    {
      Contract.Requires(!string.IsNullOrEmpty(name));
    }

    public void DisposeEnumerator(string name, int enumeratorId)
    {
      Contract.Requires(!string.IsNullOrEmpty(name));
    }
  }
}
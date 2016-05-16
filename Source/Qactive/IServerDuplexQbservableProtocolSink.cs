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

    Tuple<DuplexCallbackId, IDisposable> RegisterObservableCallbacks(int clientId, Action<object> onNext, Action<ExceptionDispatchInfo> onError, Action onCompleted, Action<int> dispose);

    object Invoke(int clientId, object[] arguments);

    IDisposable Subscribe(int clientId, Action<object> onNext, Action<ExceptionDispatchInfo> onError, Action onCompleted);

    int GetEnumerator(int clientId);

    Tuple<bool, object> MoveNext(int enumeratorId);

    void ResetEnumerator(int enumeratorId);

    void DisposeEnumerator(int enumeratorId);
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

    public Tuple<DuplexCallbackId, IDisposable> RegisterObservableCallbacks(int clientId, Action<object> onNext, Action<ExceptionDispatchInfo> onError, Action onCompleted, Action<int> dispose)
    {
      Contract.Requires(onNext != null);
      Contract.Requires(onError != null);
      Contract.Requires(onCompleted != null);
      Contract.Requires(dispose != null);
      Contract.Ensures(Contract.Result<Tuple<DuplexCallbackId, IDisposable>>() != null);
      return null;
    }

    public object Invoke(int clientId, object[] arguments)
    {
      return null;
    }

    public IDisposable Subscribe(int clientId, Action<object> onNext, Action<ExceptionDispatchInfo> onError, Action onCompleted)
    {
      Contract.Requires(onNext != null);
      Contract.Requires(onError != null);
      Contract.Requires(onCompleted != null);
      Contract.Ensures(Contract.Result<IDisposable>() != null);
      return null;
    }

    public int GetEnumerator(int clientId)
    {
      return 0;
    }

    public Tuple<bool, object> MoveNext(int enumeratorId)
    {
      Contract.Ensures(Contract.Result<Tuple<bool, object>>() != null);
      return null;
    }

    public void ResetEnumerator(int enumeratorId)
    {
    }

    public void DisposeEnumerator(int enumeratorId)
    {
    }
  }
}
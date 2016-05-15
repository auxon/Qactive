using System;
using System.Runtime.ExceptionServices;

namespace Qactive
{
  public interface IServerDuplexQbservableProtocolSink
  {
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
}
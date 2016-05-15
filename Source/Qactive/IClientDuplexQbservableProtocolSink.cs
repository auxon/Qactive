using System;
using System.Collections;
using System.Runtime.ExceptionServices;

namespace Qactive
{
  public interface IClientDuplexQbservableProtocolSink
  {
    int RegisterInvokeCallback(Func<object[], object> callback);

    int RegisterEnumerableCallback(Func<IEnumerator> getEnumerator);

    int RegisterObservableCallback(Func<int, IDisposable> subscribe);

    void SendOnNext(DuplexCallbackId id, object value);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Error", Justification = "Standard naming in Rx.")]
    void SendOnError(DuplexCallbackId id, ExceptionDispatchInfo error);

    void SendOnCompleted(DuplexCallbackId id);
  }
}
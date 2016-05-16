using System;
using System.Collections;
using System.Diagnostics.Contracts;
using System.Runtime.ExceptionServices;

namespace Qactive
{
  [ContractClass(typeof(IClientDuplexQbservableProtocolSinkContract))]
  public interface IClientDuplexQbservableProtocolSink
  {
    IQbservableProtocol Protocol { get; }

    int RegisterInvokeCallback(Func<object[], object> callback);

    int RegisterEnumerableCallback(Func<IEnumerator> getEnumerator);

    int RegisterObservableCallback(Func<int, IDisposable> subscribe);

    void SendOnNext(DuplexCallbackId id, object value);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Error", Justification = "Standard naming in Rx.")]
    void SendOnError(DuplexCallbackId id, ExceptionDispatchInfo error);

    void SendOnCompleted(DuplexCallbackId id);
  }

  [ContractClassFor(typeof(IClientDuplexQbservableProtocolSink))]
  internal abstract class IClientDuplexQbservableProtocolSinkContract : IClientDuplexQbservableProtocolSink
  {
    public IQbservableProtocol Protocol
    {
      get
      {
        Contract.Ensures(Contract.Result<IQbservableProtocol>() != null);
        return null;
      }
    }

    public int RegisterEnumerableCallback(Func<IEnumerator> getEnumerator)
    {
      Contract.Requires(getEnumerator != null);
      return 0;
    }

    public int RegisterInvokeCallback(Func<object[], object> callback)
    {
      Contract.Requires(callback != null);
      return 0;
    }

    public int RegisterObservableCallback(Func<int, IDisposable> subscribe)
    {
      Contract.Requires(subscribe != null);
      return 0;
    }

    public void SendOnCompleted(DuplexCallbackId id)
    {
    }

    public void SendOnError(DuplexCallbackId id, ExceptionDispatchInfo error)
    {
      Contract.Requires(error != null);
    }

    public void SendOnNext(DuplexCallbackId id, object value)
    {
    }
  }
}
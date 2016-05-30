using System;
using System.Diagnostics.Contracts;
using System.Runtime.ExceptionServices;

namespace Qactive
{
  [ContractClass(typeof(IClientDuplexQbservableProtocolSinkContract))]
  public interface IClientDuplexQbservableProtocolSink
  {
    IQbservableProtocol Protocol { get; }

    int RegisterInvokeCallback(IInvokeDuplexCallback invoke);

    IEnumerableDuplexCallback RegisterEnumerableCallback(Func<int, IEnumerableDuplexCallback> callbackFactory);

    IObservableDuplexCallback RegisterObservableCallback(Func<int, IObservableDuplexCallback> callbackFactory);

    void SendOnNext(string name, DuplexCallbackId id, object value);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Error", Justification = "Standard naming in Rx.")]
    void SendOnError(string name, DuplexCallbackId id, ExceptionDispatchInfo error);

    void SendOnCompleted(string name, DuplexCallbackId id);
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

    public int RegisterInvokeCallback(IInvokeDuplexCallback invoke)
    {
      Contract.Requires(invoke != null);
      return 0;
    }

    public IEnumerableDuplexCallback RegisterEnumerableCallback(Func<int, IEnumerableDuplexCallback> callbackFactory)
    {
      Contract.Requires(callbackFactory != null);
      Contract.Ensures(Contract.Result<IEnumerableDuplexCallback>() != null);
      return null;
    }

    public IObservableDuplexCallback RegisterObservableCallback(Func<int, IObservableDuplexCallback> callbackFactory)
    {
      Contract.Requires(callbackFactory != null);
      Contract.Ensures(Contract.Result<IObservableDuplexCallback>() != null);
      return null;
    }

    public void SendOnCompleted(string name, DuplexCallbackId id)
    {
#if TRACE
      Contract.Requires(!string.IsNullOrEmpty(name));
#endif
    }

    public void SendOnError(string name, DuplexCallbackId id, ExceptionDispatchInfo error)
    {
#if TRACE
      Contract.Requires(!string.IsNullOrEmpty(name));
#endif
      Contract.Requires(error != null);
    }

    public void SendOnNext(string name, DuplexCallbackId id, object value)
    {
#if TRACE
      Contract.Requires(!string.IsNullOrEmpty(name));
#endif
    }
  }
}
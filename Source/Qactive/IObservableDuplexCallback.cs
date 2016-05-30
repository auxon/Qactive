using System;
using System.Diagnostics.Contracts;

namespace Qactive
{
  [ContractClass(typeof(IObservableDuplexCallbackContract))]
  public interface IObservableDuplexCallback
  {
    string Name { get; }

    IDisposable Subscribe(Action<object> onNext, Action<Exception> onError, Action onCompleted);
  }

  [ContractClassFor(typeof(IObservableDuplexCallback))]
  internal abstract class IObservableDuplexCallbackContract : IObservableDuplexCallback
  {
    public string Name
    {
      get
      {
        Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));
        return null;
      }
    }

    public IDisposable Subscribe(Action<object> onNext, Action<Exception> onError, Action onCompleted)
    {
      Contract.Requires(onNext != null);
      Contract.Requires(onError != null);
      Contract.Requires(onCompleted != null);
      Contract.Ensures(Contract.Result<IDisposable>() != null);
      return null;
    }
  }
}

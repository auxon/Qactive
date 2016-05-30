using System.Diagnostics.Contracts;

namespace Qactive
{
  [ContractClass(typeof(IInvokeDuplexCallbackContract))]
  public interface IInvokeDuplexCallback
  {
    string Name { get; }

    object Invoke(object[] arguments);
  }

  [ContractClassFor(typeof(IInvokeDuplexCallback))]
  internal abstract class IInvokeDuplexCallbackContract : IInvokeDuplexCallback
  {
    public string Name
    {
      get
      {
        Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));
        return null;
      }
    }

    public object Invoke(object[] arguments)
    {
      return null;
    }
  }
}

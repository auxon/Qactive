using System.Collections;
using System.Diagnostics.Contracts;

namespace Qactive
{
  [ContractClass(typeof(IEnumerableDuplexCallbackContract))]
  public interface IEnumerableDuplexCallback
  {
    string Name { get; }

    IEnumerator GetEnumerator();
  }

  [ContractClassFor(typeof(IEnumerableDuplexCallback))]
  internal abstract class IEnumerableDuplexCallbackContract : IEnumerableDuplexCallback
  {
    public string Name
    {
      get
      {
        Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));
        return null;
      }
    }

    public IEnumerator GetEnumerator()
    {
      Contract.Ensures(Contract.Result<IEnumerator>() != null);
      return null;
    }
  }
}

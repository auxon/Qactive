using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Security;
using System.Security.Policy;

namespace Qactive
{
  [ContractClass(typeof(ISecureQbservableProviderFactoryContract))]
  public interface ISecureQbservableProviderFactory
  {
    IEnumerable<StrongName> FullTrustAssemblies { get; }

    IEnumerable<IPermission> MinimumServerPermissions { get; }
  }

  [ContractClassFor(typeof(ISecureQbservableProviderFactory))]
  internal abstract class ISecureQbservableProviderFactoryContract : ISecureQbservableProviderFactory
  {
    public IEnumerable<StrongName> FullTrustAssemblies
    {
      get
      {
        Contract.Ensures(Contract.Result<IEnumerable<StrongName>>() != null);
        return null;
      }
    }

    public IEnumerable<IPermission> MinimumServerPermissions
    {
      get
      {
        Contract.Ensures(Contract.Result<IEnumerable<IPermission>>() != null);
        return null;
      }
    }
  }
}

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Security;
using System.Security.Policy;

namespace Qactive
{
  [Serializable]
  [ContractClass(typeof(QactiveProviderFactoryContract))]
  public abstract class QactiveProviderFactory
  {
    public virtual IEnumerable<StrongName> FullTrustAssemblies
    {
      get
      {
        Contract.Ensures(Contract.Result<IEnumerable<StrongName>>() != null);
        return Enumerable.Empty<StrongName>();
      }
    }

    public virtual IEnumerable<IPermission> MinimumServerPermissions
    {
      get
      {
        Contract.Ensures(Contract.Result<IEnumerable<IPermission>>() != null);
        return Enumerable.Empty<IPermission>();
      }
    }

    public abstract QactiveProvider Create();
  }

  [ContractClassFor(typeof(QactiveProviderFactory))]
  internal abstract class QactiveProviderFactoryContract : QactiveProviderFactory
  {
    public override QactiveProvider Create()
    {
      Contract.Ensures(Contract.Result<QactiveProvider>() != null);
      return null;
    }
  }
}

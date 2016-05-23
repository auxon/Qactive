using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Security;
#if CAS
using System.Security.Policy;
#endif

namespace Qactive
{
#if SERIALIZATION
  [Serializable]
#endif
  [ContractClass(typeof(QactiveProviderFactoryContract))]
  public abstract class QactiveProviderFactory
  {
#if CAS
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
#endif

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

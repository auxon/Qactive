using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Policy;

namespace Qactive
{
  [Serializable]
  public abstract class QactiveProviderFactory
  {
    public virtual IEnumerable<StrongName> FullTrustAssemblies => Enumerable.Empty<StrongName>();

    public virtual IEnumerable<IPermission> MinimumServerPermissions => Enumerable.Empty<IPermission>();

    public abstract QactiveProvider Create();
  }
}

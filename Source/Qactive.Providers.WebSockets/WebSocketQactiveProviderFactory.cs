using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net;
using System.Security;
using System.Security.Policy;

namespace Qactive
{
  [Serializable]
  internal class WebSocketQactiveProviderFactory : QactiveProviderFactory
#if CAS_IN_STREAMING
    , ISecureQbservableProviderFactory
#endif
  {
    public Uri Uri { get; }

#if CAS_REF
    public override IEnumerable<StrongName> FullTrustAssemblies
      => new[]
      {
        typeof(WebSocketQactiveProvider).Assembly.Evidence.GetHostEvidence<StrongName>(),
        typeof(StreamQbservableProtocolFactory).Assembly.Evidence.GetHostEvidence<StrongName>()
      };
#else
    public IEnumerable<StrongName> FullTrustAssemblies
      => new[]
      {
        typeof(WebSocketQactiveProvider).Assembly.Evidence.GetHostEvidence<StrongName>(),
        typeof(StreamQbservableProtocolFactory).Assembly.Evidence.GetHostEvidence<StrongName>()
      };
#endif

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2103:ReviewImperativeSecurity", Justification = "Reviewed.")]
#if CAS_REF
    public override IEnumerable<IPermission> MinimumServerPermissions
      => new IPermission[]
      {
        new WebPermission(NetworkAccess.Accept, Uri.ToString()),
        new SocketPermission(NetworkAccess.Accept, TransportType.Tcp, Uri.Host, Uri.Port)
      };
#else
    public IEnumerable<IPermission> MinimumServerPermissions
      => new IPermission[]
      {
        new WebPermission(NetworkAccess.Accept, Uri.ToString()),
        new SocketPermission(NetworkAccess.Accept, TransportType.Tcp, Uri.Host, Uri.Port)
      };
#endif

    public WebSocketQactiveProviderFactory(Uri uri)
    {
      Contract.Requires(uri != null);

      Uri = uri;
    }

    [ContractInvariantMethod]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
    private void ObjectInvariant()
    {
      Contract.Invariant(Uri != null);
    }

    public override QactiveProvider Create()
      => WebSocketQactiveProvider.Server(Uri);
  }
}

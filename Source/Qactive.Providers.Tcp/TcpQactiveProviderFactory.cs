using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net;
using System.Security;
using System.Security.Policy;

namespace Qactive
{
  [Serializable]
  internal class TcpQactiveProviderFactory : QactiveProviderFactory
#if CAS_IN_STREAMING
    , ISecureQbservableProviderFactory
#endif
  {
    public IPEndPoint EndPoint { get; }

#if CAS_REF
    public override IEnumerable<StrongName> FullTrustAssemblies
      => new[]
      {
        typeof(TcpQactiveProvider).Assembly.Evidence.GetHostEvidence<StrongName>(),
        typeof(StreamQbservableProtocolFactory).Assembly.Evidence.GetHostEvidence<StrongName>()
      };
#else
    public IEnumerable<StrongName> FullTrustAssemblies
      => new[]
      {
        typeof(TcpQactiveProvider).Assembly.Evidence.GetHostEvidence<StrongName>(),
        typeof(StreamQbservableProtocolFactory).Assembly.Evidence.GetHostEvidence<StrongName>()
      };
#endif

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2103:ReviewImperativeSecurity", Justification = "Reviewed.")]
#if CAS_REF
    public override IEnumerable<IPermission> MinimumServerPermissions
      => new[] { new SocketPermission(NetworkAccess.Accept, TransportType.Tcp, EndPoint.Address.ToString(), EndPoint.Port) };
#else
    public IEnumerable<IPermission> MinimumServerPermissions
      => new[] { new SocketPermission(NetworkAccess.Accept, TransportType.Tcp, EndPoint.Address.ToString(), EndPoint.Port) };
#endif

    public TcpQactiveProviderFactory(IPEndPoint endPoint)
    {
      Contract.Requires(endPoint != null);

      EndPoint = endPoint;
    }

    [ContractInvariantMethod]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
    private void ObjectInvariant()
    {
      Contract.Invariant(EndPoint != null);
    }

    public override QactiveProvider Create()
      => TcpQactiveProvider.Server(EndPoint);
  }
}

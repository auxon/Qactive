using System;
using System.Collections.Generic;
using System.Net;
using System.Security;
using System.Security.Policy;

namespace Qactive
{
  [Serializable]
  internal class TcpQactiveProviderFactory : QactiveProviderFactory
  {
    public IPEndPoint EndPoint { get; }

    public override IEnumerable<StrongName> FullTrustAssemblies => new[] { typeof(TcpQactiveProvider).Assembly.Evidence.GetHostEvidence<StrongName>() };

    public override IEnumerable<IPermission> MinimumServerPermissions => new[] { new SocketPermission(NetworkAccess.Accept, TransportType.Tcp, EndPoint.Address.ToString(), EndPoint.Port) };

    public TcpQactiveProviderFactory(IPEndPoint endPoint)
    {
      EndPoint = endPoint;
    }

    public override QactiveProvider Create() => TcpQactiveProvider.Server(EndPoint);
  }
}

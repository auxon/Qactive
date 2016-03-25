using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Security;
using System.Security.Policy;

namespace Qactive
{
  [Serializable]
  internal sealed class TcpQactiveProviderFactory : QactiveProviderFactory
  {
    public IPEndPoint EndPoint { get; }

    public Func<IRemotingFormatter> FormatterFactory { get; }

    public override IEnumerable<StrongName> FullTrustAssemblies => new[] { typeof(TcpQactiveProvider).Assembly.Evidence.GetHostEvidence<StrongName>() };

    public override IEnumerable<IPermission> MinimumServerPermissions => new[] { new SocketPermission(NetworkAccess.Accept, TransportType.Tcp, EndPoint.Address.ToString(), EndPoint.Port) };

    public TcpQactiveProviderFactory(IPEndPoint endPoint, Func<IRemotingFormatter> formatterFactory = null)
    {
      EndPoint = endPoint;
      FormatterFactory = formatterFactory;
    }

    public override QactiveProvider Create() => FormatterFactory == null ? TcpQactiveProvider.Server(EndPoint) : TcpQactiveProvider.Server(EndPoint, FormatterFactory);
  }
}

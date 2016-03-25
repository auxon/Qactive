using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using System.Security;
using System.Security.Policy;

namespace Qactive
{
  [Serializable]
  internal sealed class TcpQactiveProviderFactory : QactiveProviderFactory
  {
    public IPEndPoint EndPoint { get; }

    private readonly Action<Socket> prepareSocket;
    private readonly Func<IRemotingFormatter> formatterFactory;

    public override IEnumerable<StrongName> FullTrustAssemblies => new[] { typeof(TcpQactiveProvider).Assembly.Evidence.GetHostEvidence<StrongName>() };

    public override IEnumerable<IPermission> MinimumServerPermissions => new[] { new SocketPermission(NetworkAccess.Accept, TransportType.Tcp, EndPoint.Address.ToString(), EndPoint.Port) };

    public TcpQactiveProviderFactory(IPEndPoint endPoint, Action<Socket> prepareSocket, Func<IRemotingFormatter> formatterFactory = null)
    {
      EndPoint = endPoint;
      this.prepareSocket = prepareSocket;
      this.formatterFactory = formatterFactory;
    }

    public override QactiveProvider Create() => formatterFactory == null ? TcpQactiveProvider.Server(EndPoint, prepareSocket) : TcpQactiveProvider.Server(EndPoint, prepareSocket, formatterFactory);
  }
}

using System;
using System.Net;

namespace Qactive
{
  [Serializable]
  internal sealed class TcpQactiveProviderFactory<TTransportInitializer> : TcpQactiveProviderFactory
    where TTransportInitializer : ITcpQactiveProviderTransportInitializer, new()
  {
    public TcpQactiveProviderFactory(IPEndPoint endPoint)
      : base(endPoint)
    {
    }

    public override QactiveProvider Create() => TcpQactiveProvider.Server(EndPoint, Activator.CreateInstance<TTransportInitializer>());
  }
}

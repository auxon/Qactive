using System;
using System.Diagnostics.Contracts;
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
      Contract.Requires(endPoint != null);
    }

    public override QactiveProvider Create()
      => TcpQactiveProvider.Server(EndPoint, Activator.CreateInstance<TTransportInitializer>());
  }
}

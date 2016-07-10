using System;
using System.Diagnostics.Contracts;

namespace Qactive
{
  [Serializable]
  internal sealed class WebSocketQactiveProviderFactory<TTransportInitializer> : WebSocketQactiveProviderFactory
    where TTransportInitializer : IWebSocketQactiveProviderTransportInitializer, new()
  {
    public WebSocketQactiveProviderFactory(Uri uri)
      : base(uri)
    {
      Contract.Requires(uri != null);
    }

    public override QactiveProvider Create()
      => WebSocketQactiveProvider.Server(Uri, Activator.CreateInstance<TTransportInitializer>());
  }
}

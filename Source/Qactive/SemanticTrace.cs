namespace Qactive
{
  public enum SemanticTrace
  {
    None,

    // Server-side tracing
    ServerStarting,
    ServerStopped,
    ServerClientSubscribing,
    ServerReceivingExpression,
    ServerRewrittenExpression,
    ServerClientSubscribed,
    ServerClientDisconnecting,
    ServerClientDisconnected,
    ServerDisconnectingClient,
    ServerDisconnectedClient,

    // Full-duplex (server-side)
    ServerSubscribing,
    ServerSubscribed,
    ServerDisconnecting,
    ServerDisconnected,

    // Client-side tracing
    ClientSubscribing,
    ClientSendingExpression,
    ClientSubscribed,
    ClientDisconnecting,
    ClientDisconnected,

    // Full-duplex (client-side)
    ClientServerSubscribing,
    ClientRewrittenExpression,
    ClientServerSubscribed,
    ClientServerDisconnecting,
    ClientServerDisconnected,
    ClientDisconnectingServer,
    ClientDisconnectedServer
  }
}

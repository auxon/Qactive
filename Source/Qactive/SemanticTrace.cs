namespace Qactive
{
  public enum SemanticTrace
  {
    None,

    // Server-side tracing
    ServerStarting,
    ServerStarted,
    ServerStopping,
    ServerStopped,
    ServerClientConnecting,
    ServerClientConnected,
    ServerClientSubscribing,
    ServerReceivingExpression,
    ServerRewrittenExpression,
    ServerClientSubscribed,
    ServerOnNext,
    ServerOnError,
    ServerOnCompleted,
    ServerClientUnsubscribing,
    ServerClientUnsubscribed,
    ServerClientDisconnecting,
    ServerClientDisconnected,
    ServerDisconnectingClient,
    ServerDisconnectedClient,

    // Full-duplex (server-side)
    ServerConnecting, // TODO: log this
    ServerConnected, // TODO: log this
    ServerSubscribing,
    ServerSubscribed,
    ServerClientOnNext,
    ServerClientOnError,
    ServerClientOnCompleted,
    ServerUnsubscribing,
    ServerUnsubscribed,
    ServerEnumerating,
    ServerMoveNext,
    ServerCurrent,
    ServerEnumerated,
    ServerInvoking,
    ServerInvoked,
    ServerDisconnecting, // TODO: log this
    ServerDisconnected, // TODO: log this

    // Client-side tracing
    ClientConnecting,
    ClientConnected,
    ClientSubscribing,
    ClientSendingExpression,
    ClientSubscribed,
    ClientServerOnNext,
    ClientServerOnError,
    ClientServerOnCompleted,
    ClientUnsubscribing,
    ClientUnsubscribed,
    ClientDisconnecting,
    ClientDisconnected,

    // Full-duplex (client-side)
    ClientServerConnecting, // TODO: log this
    ClientServerConnected, // TODO: log this
    ClientServerSubscribing,
    ClientRewrittenExpression,
    ClientServerSubscribed,
    ClientOnNext,
    ClientOnError,
    ClientOnCompleted,
    ClientServerUnsubscribing,
    ClientServerUnsubscribed,
    ClientServerEnumerating,
    ClientServerMoveNext,
    ClientServerCurrent,
    ClientServerEnumerated,
    ClientServerInvoking,
    ClientServerInvoked,
    ClientServerDisconnecting, // TODO: log this
    ClientServerDisconnected, // TODO: log this
    ClientDisconnectingServer, // TODO: log this
    ClientDisconnectedServer // TODO: log this
  }
}

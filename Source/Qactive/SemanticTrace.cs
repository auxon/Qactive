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
    ServerDisconnecting,
    ServerDisconnected,

    // Client-side tracing
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
    ClientServerDisconnecting,
    ClientServerDisconnected,
    ClientDisconnectingServer,
    ClientDisconnectedServer
  }
}

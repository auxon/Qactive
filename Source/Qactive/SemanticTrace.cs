namespace Qactive
{
  public enum SemanticTrace
  {
    None,

    // Server-side tracing
    ServerStarting, // TODO: log this
    ServerStopped, // TODO: log this
    ServerClientSubscribing,
    ServerReceivingExpression,
    ServerRewrittenExpression,
    ServerClientSubscribed,
    ServerOnNext,
    ServerOnError,
    ServerOnCompleted,
    ServerClientUnsubscribing,
    ServerClientUnsubscribed,
    ServerClientDisconnecting, // TODO: log this
    ServerClientDisconnected, // TODO: log this
    ServerDisconnectingClient, // TODO: log this
    ServerDisconnectedClient, // TODO: log this

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
    ServerDisconnecting, // TODO: log this
    ServerDisconnected, // TODO: log this

    // Client-side tracing
    ClientSubscribing,
    ClientSendingExpression,
    ClientSubscribed,
    ClientServerOnNext,
    ClientServerOnError,
    ClientServerOnCompleted,
    ClientUnsubscribing,
    ClientUnsubscribed,
    ClientDisconnecting, // TODO: log this
    ClientDisconnected, // TODO: log this

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
    ClientServerDisconnecting, // TODO: log this
    ClientServerDisconnected, // TODO: log this
    ClientDisconnectingServer, // TODO: log this
    ClientDisconnectedServer // TODO: log this
  }
}

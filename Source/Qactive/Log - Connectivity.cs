using System.Diagnostics;
using System.Runtime.CompilerServices;
using Qactive.Properties;

namespace Qactive
{
  static partial class Log
  {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Qactive.Log.SemanticObjectUnsafe(System.Diagnostics.TraceSource,Qactive.SemanticTrace,System.Diagnostics.TraceEventType,System.Object,System.String,System.Object)", Justification = "It's just a separator.")]
    [Conditional("TRACE")]
    public static void Starting(bool isServer, bool isServerReceiving, object sourceId, [CallerMemberName]string label = null, object data = null)
#if TRACING
      => QactiveTraceSources.Qactive.SemanticObjectUnsafe(
          isServer
        ? isServerReceiving
          ? SemanticTrace.ServerClientConnecting
          : SemanticTrace.ServerStarting
        : SemanticTrace.ClientConnecting,
        TraceEventType.Information,
        sourceId,
        label + ": " + (isServer && !isServerReceiving ? LogMessages.Starting : LogMessages.Connecting),
        data);
#else
    {
    }
#endif

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Qactive.Log.SemanticObjectUnsafe(System.Diagnostics.TraceSource,Qactive.SemanticTrace,System.Diagnostics.TraceEventType,System.Object,System.String,System.Object)", Justification = "It's just a separator.")]
    [Conditional("TRACE")]
    public static void Started(bool isServer, bool isServerReceiving, object sourceId, [CallerMemberName]string label = null, object data = null)
#if TRACING
      => QactiveTraceSources.Qactive.SemanticObjectUnsafe(
          isServer
        ? isServerReceiving
          ? SemanticTrace.ServerClientConnected
          : SemanticTrace.ServerStarted
        : SemanticTrace.ClientConnected,
        TraceEventType.Information,
        sourceId,
        label + ": " + (isServer && !isServerReceiving ? LogMessages.Started : LogMessages.Connected),
        data);
#else
    {
    }
#endif

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Qactive.Log.SemanticObjectUnsafe(System.Diagnostics.TraceSource,Qactive.SemanticTrace,System.Diagnostics.TraceEventType,System.Object,System.String,System.Object)", Justification = "It's just a separator.")]
    [Conditional("TRACE")]
    public static void Stopping(bool isServer, bool isServerReceiving, object sourceId, [CallerMemberName]string label = null, object data = null)
#if TRACING
      => QactiveTraceSources.Qactive.SemanticObjectUnsafe(
          isServer
        ? isServerReceiving
          ? SemanticTrace.ServerClientDisconnecting
          : SemanticTrace.ServerStopping
        : SemanticTrace.ClientDisconnecting,
        TraceEventType.Information,
        sourceId,
        label + ": " + (isServer && !isServerReceiving ? LogMessages.Stopping : LogMessages.Disconnecting),
        data);
#else
    {
    }
#endif

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Qactive.Log.SemanticObjectUnsafe(System.Diagnostics.TraceSource,Qactive.SemanticTrace,System.Diagnostics.TraceEventType,System.Object,System.String,System.Object)", Justification = "It's just a separator.")]
    [Conditional("TRACE")]
    public static void Stopped(bool isServer, bool isServerReceiving, object sourceId, [CallerMemberName]string label = null, object data = null)
#if TRACING
      => QactiveTraceSources.Qactive.SemanticObjectUnsafe(
          isServer
        ? isServerReceiving
          ? SemanticTrace.ServerClientDisconnected
          : SemanticTrace.ServerStopped
        : SemanticTrace.ClientDisconnected,
        TraceEventType.Information,
        sourceId,
        label + ": " + (isServer && !isServerReceiving ? LogMessages.Stopped : LogMessages.Disconnected),
        data);
#else
    {
    }
#endif

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Qactive.Log.SemanticObjectUnsafe(System.Diagnostics.TraceSource,Qactive.SemanticTrace,System.Diagnostics.TraceEventType,System.Object,System.String,System.Object)", Justification = "It's just a separator.")]
    [Conditional("TRACE")]
    public static void DuplexConnecting(bool isServer, object sourceId, [CallerMemberName]string label = null, object data = null)
#if TRACING
      => QactiveTraceSources.Qactive.SemanticObjectUnsafe(
          isServer
        ? SemanticTrace.ServerConnecting
        : SemanticTrace.ClientServerConnecting,
        TraceEventType.Information,
        sourceId,
        label + ": " + LogMessages.Connecting,
        data);
#else
    {
    }
#endif

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Qactive.Log.SemanticObjectUnsafe(System.Diagnostics.TraceSource,Qactive.SemanticTrace,System.Diagnostics.TraceEventType,System.Object,System.String,System.Object)", Justification = "It's just a separator.")]
    [Conditional("TRACE")]
    public static void DuplexConnected(bool isServer, object sourceId, [CallerMemberName]string label = null, object data = null)
#if TRACING
      => QactiveTraceSources.Qactive.SemanticObjectUnsafe(
          isServer
        ? SemanticTrace.ServerConnected
        : SemanticTrace.ClientServerConnected,
        TraceEventType.Information,
        sourceId,
        label + ": " + LogMessages.Connected,
        data);
#else
    {
    }
#endif

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Qactive.Log.SemanticObjectUnsafe(System.Diagnostics.TraceSource,Qactive.SemanticTrace,System.Diagnostics.TraceEventType,System.Object,System.String,System.Object)", Justification = "It's just a separator.")]
    [Conditional("TRACE")]
    public static void DuplexDisconnecting(bool isServer, bool isClientReceiving, object sourceId, [CallerMemberName]string label = null, object data = null)
#if TRACING
      => QactiveTraceSources.Qactive.SemanticObjectUnsafe(
          isServer
        ? SemanticTrace.ServerDisconnectingClient
        : isClientReceiving
          ? SemanticTrace.ClientServerDisconnecting
          : SemanticTrace.ClientDisconnectingServer,
        TraceEventType.Information,
        sourceId,
        label + ": " + LogMessages.Disconnecting,
        data);
#else
    {
    }
#endif

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Qactive.Log.SemanticObjectUnsafe(System.Diagnostics.TraceSource,Qactive.SemanticTrace,System.Diagnostics.TraceEventType,System.Object,System.String,System.Object)", Justification = "It's just a separator.")]
    [Conditional("TRACE")]
    public static void DuplexDisconnected(bool isServer, bool isClientReceiving, object sourceId, [CallerMemberName]string label = null, object data = null)
#if TRACING
      => QactiveTraceSources.Qactive.SemanticObjectUnsafe(
          isServer
        ? SemanticTrace.ServerDisconnectedClient
        : isClientReceiving
          ? SemanticTrace.ClientServerDisconnected
          : SemanticTrace.ClientDisconnectedServer,
        TraceEventType.Information,
        sourceId,
        label + ": " + LogMessages.Disconnected,
        data);
#else
    {
    }
#endif
  }
}

using System.Diagnostics;
using System.Runtime.CompilerServices;
using Qactive.Properties;

namespace Qactive
{
  static partial class Log
  {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Qactive.Log.SemanticObject(System.Diagnostics.TraceSource,Qactive.SemanticTrace,System.Diagnostics.TraceEventType,System.Object,System.String,System.Object)", Justification = "It's just a separator.")]
    [Conditional("TRACE")]
    public static void Invoking(string name, object[] arguments, bool isServer, object sourceId, [CallerMemberName]string label = null)
#if TRACING
      => QactiveTraceSources.Qactive.SemanticObject(
          isServer
        ? SemanticTrace.ServerInvoking
        : SemanticTrace.ClientServerInvoking,
        TraceEventType.Information,
        sourceId,
        label + ": " + LogMessages.Invoking,
        name + (arguments == null ? null : "(" + string.Join(", ", arguments) + ")"));
#else
    {
    }
#endif

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Qactive.Log.SemanticObject(System.Diagnostics.TraceSource,Qactive.SemanticTrace,System.Diagnostics.TraceEventType,System.Object,System.String,System.Object)", Justification = "It's just a separator.")]
    [Conditional("TRACE")]
    public static void Invoked(string name, object[] arguments, object returnValue, bool isServer, object sourceId, [CallerMemberName]string label = null)
#if TRACING
      => QactiveTraceSources.Qactive.SemanticObject(
          isServer
        ? SemanticTrace.ServerInvoked
        : SemanticTrace.ClientServerInvoked,
        TraceEventType.Information,
        sourceId,
        label + ": " + LogMessages.Invoked,
        name + (arguments == null ? null : "(" + string.Join(", ", arguments) + ")") + " -> " + returnValue);
#else
    {
    }
#endif
  }
}

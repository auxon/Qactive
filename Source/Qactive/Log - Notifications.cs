using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using Qactive.Properties;

namespace Qactive
{
  static partial class Log
  {
    public static IObservable<TSource> TraceNotifications<TSource>(this IObservable<TSource> source, string name, bool isServer, bool isReceiving, object sourceId, [CallerMemberName]string label = null)
    {
      Contract.Requires(source != null);
      Contract.Ensures(Contract.Result<IObservable<TSource>>() != null);

#if TRACE
      return source.Do(
        value => OnNext(name, value, isServer, isReceiving, sourceId, label),
        error => OnError(name, error, isServer, isReceiving, sourceId, label),
        () => OnCompleted(name, isServer, isReceiving, sourceId, label));
#else
      return source;
#endif
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Qactive.Log.SemanticObject(System.Diagnostics.TraceSource,Qactive.SemanticTrace,System.Diagnostics.TraceEventType,System.Object,System.String,System.Object)", Justification = "It's just a separator.")]
    [Conditional("TRACE")]
    public static void OnNext<T>(string observable, T value, bool isServer, bool isReceiving, object sourceId, [CallerMemberName]string label = null)
#if TRACING
      => QactiveTraceSources.Qactive.SemanticObject(
          isServer
        ? isReceiving
          ? SemanticTrace.ServerClientOnNext
          : SemanticTrace.ServerOnNext
        : isReceiving
          ? SemanticTrace.ClientServerOnNext
          : SemanticTrace.ClientOnNext,
        TraceEventType.Verbose,
        sourceId,
        label + ": " + LogMessages.OnNext,
        observable + " -> " + value);
#else
    {
    }
#endif

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Qactive.Log.SemanticObject(System.Diagnostics.TraceSource,Qactive.SemanticTrace,System.Diagnostics.TraceEventType,System.Object,System.String,System.Object)", Justification = "It's just a separator.")]
    [Conditional("TRACE")]
    public static void OnError(string observable, Exception error, bool isServer, bool isReceiving, object sourceId, [CallerMemberName]string label = null)
#if TRACING
      => QactiveTraceSources.Qactive.SemanticObject(
          isServer
        ? isReceiving
          ? SemanticTrace.ServerClientOnError
          : SemanticTrace.ServerOnError
        : isReceiving
          ? SemanticTrace.ClientServerOnError
          : SemanticTrace.ClientOnError,
        TraceEventType.Verbose,
        sourceId,
        label + ": " + LogMessages.OnError,
        observable + " -> " + error);
#else
    {
    }
#endif

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Qactive.Log.SemanticObject(System.Diagnostics.TraceSource,Qactive.SemanticTrace,System.Diagnostics.TraceEventType,System.Object,System.String,System.Object)", Justification = "It's just a separator.")]
    [Conditional("TRACE")]
    public static void OnCompleted(string observable, bool isServer, bool isReceiving, object sourceId, [CallerMemberName]string label = null)
#if TRACING
      => QactiveTraceSources.Qactive.SemanticObject(
          isServer
        ? isReceiving
          ? SemanticTrace.ServerClientOnCompleted
          : SemanticTrace.ServerOnCompleted
        : isReceiving
          ? SemanticTrace.ClientServerOnCompleted
          : SemanticTrace.ClientOnCompleted,
        TraceEventType.Verbose,
        sourceId,
        label + ": " + LogMessages.OnCompleted,
        observable);
#else
    {
    }
#endif

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Qactive.Log.SemanticObject(System.Diagnostics.TraceSource,Qactive.SemanticTrace,System.Diagnostics.TraceEventType,System.Object,System.String,System.Object)", Justification = "It's just a separator.")]
    [Conditional("TRACE")]
    public static void MoveNext(string enumerable, bool isServer, object sourceId, [CallerMemberName]string label = null)
#if TRACING
      => QactiveTraceSources.Qactive.SemanticObject(
          isServer
        ? SemanticTrace.ServerMoveNext
        : SemanticTrace.ClientServerMoveNext,
        TraceEventType.Verbose,
        sourceId,
        label + ": " + LogMessages.MoveNext,
        enumerable);
#else
    {
    }
#endif

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Qactive.Log.SemanticObject(System.Diagnostics.TraceSource,Qactive.SemanticTrace,System.Diagnostics.TraceEventType,System.Object,System.String,System.Object)", Justification = "It's just a separator.")]
    [Conditional("TRACE")]
    public static void Current(string enumerable, object value, bool isServer, object sourceId, [CallerMemberName]string label = null)
#if TRACING
      => QactiveTraceSources.Qactive.SemanticObject(
          isServer
        ? SemanticTrace.ServerCurrent
        : SemanticTrace.ClientServerCurrent,
        TraceEventType.Verbose,
        sourceId,
        label + ": " + LogMessages.Current,
        enumerable + " -> " + value);
#else
    {
    }
#endif
  }
}

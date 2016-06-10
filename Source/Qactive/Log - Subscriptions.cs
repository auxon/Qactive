using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using Qactive.Properties;

namespace Qactive
{
  static partial class Log
  {
    public static IObservable<TSource> TraceSubscriptions<TSource>(this IObservable<TSource> source, string name, bool isServer, bool isReceiving, object sourceId, [CallerMemberName]string label = null)
    {
      Contract.Requires(source != null);
      Contract.Ensures(Contract.Result<IObservable<TSource>>() != null);

#if TRACE
      return Observable.Create<TSource>(observer =>
      {
        Subscribing(name, isServer, isReceiving, sourceId, label);

        var subscription = source.Subscribe(observer);

        Subscribed(name, isServer, isReceiving, sourceId, label);

        return Disposable.Create(() =>
        {
          Unsubscribing(name, isServer, isReceiving, sourceId, label);

          subscription.Dispose();

          Unsubscribed(name, isServer, isReceiving, sourceId, label);
        });
      });
#else
      return source;
#endif
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Qactive.Log.SemanticObjectUnsafe(System.Diagnostics.TraceSource,Qactive.SemanticTrace,System.Diagnostics.TraceEventType,System.Object,System.String,System.Object)", Justification = "It's just a separator.")]
    [Conditional("TRACE")]
    public static void Subscribing(string observable, bool isServer, bool isReceiving, object sourceId, [CallerMemberName]string label = null)
#if TRACING
      => QactiveTraceSources.Qactive.SemanticObjectUnsafe(
          isServer
        ? isReceiving
          ? SemanticTrace.ServerClientSubscribing
          : SemanticTrace.ServerSubscribing
        : isReceiving
          ? SemanticTrace.ClientServerSubscribing
          : SemanticTrace.ClientSubscribing,
        TraceEventType.Information,
        sourceId,
        label + ": " + LogMessages.Subscribing,
        observable);
#else
    {
    }
#endif

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Qactive.Log.SemanticObjectUnsafe(System.Diagnostics.TraceSource,Qactive.SemanticTrace,System.Diagnostics.TraceEventType,System.Object,System.String,System.Object)", Justification = "It's just a separator.")]
    [Conditional("TRACE")]
    public static void Subscribed(string observable, bool isServer, bool isReceiving, object sourceId, [CallerMemberName]string label = null)
#if TRACING
      => QactiveTraceSources.Qactive.SemanticObjectUnsafe(
          isServer
        ? isReceiving
          ? SemanticTrace.ServerClientSubscribed
          : SemanticTrace.ServerSubscribed
        : isReceiving
          ? SemanticTrace.ClientServerSubscribed
          : SemanticTrace.ClientSubscribed,
        TraceEventType.Information,
        sourceId,
        label + ": " + LogMessages.Subscribed,
        observable);
#else
    {
    }
#endif

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Qactive.Log.SemanticObjectUnsafe(System.Diagnostics.TraceSource,Qactive.SemanticTrace,System.Diagnostics.TraceEventType,System.Object,System.String,System.Object)", Justification = "It's just a separator.")]
    [Conditional("TRACE")]
    public static void Unsubscribing(string observable, bool isServer, bool isReceiving, object sourceId, [CallerMemberName]string label = null)
#if TRACING
      => QactiveTraceSources.Qactive.SemanticObjectUnsafe(
          isServer
        ? isReceiving
          ? SemanticTrace.ServerClientUnsubscribing
          : SemanticTrace.ServerUnsubscribing
        : isReceiving
          ? SemanticTrace.ClientServerUnsubscribing
          : SemanticTrace.ClientUnsubscribing,
        TraceEventType.Information,
        sourceId,
        label + ": " + LogMessages.Unsubscribing,
        observable);
#else
    {
    }
#endif

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Qactive.Log.SemanticObjectUnsafe(System.Diagnostics.TraceSource,Qactive.SemanticTrace,System.Diagnostics.TraceEventType,System.Object,System.String,System.Object)", Justification = "It's just a separator.")]
    [Conditional("TRACE")]
    public static void Unsubscribed(string observable, bool isServer, bool isReceiving, object sourceId, [CallerMemberName]string label = null)
#if TRACING
      => QactiveTraceSources.Qactive.SemanticObjectUnsafe(
          isServer
        ? isReceiving
          ? SemanticTrace.ServerClientUnsubscribed
          : SemanticTrace.ServerUnsubscribed
        : isReceiving
          ? SemanticTrace.ClientServerUnsubscribed
          : SemanticTrace.ClientUnsubscribed,
        TraceEventType.Information,
        sourceId,
        label + ": " + LogMessages.Unsubscribed,
        observable);
#else
    {
    }
#endif

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Qactive.Log.SemanticObjectUnsafe(System.Diagnostics.TraceSource,Qactive.SemanticTrace,System.Diagnostics.TraceEventType,System.Object,System.String,System.Object)", Justification = "It's just a separator.")]
    [Conditional("TRACE")]
    public static void Enumerating(string enumerable, bool isServer, object sourceId, [CallerMemberName]string label = null)
#if TRACING
      => QactiveTraceSources.Qactive.SemanticObjectUnsafe(
          isServer
        ? SemanticTrace.ServerEnumerating
        : SemanticTrace.ClientServerEnumerating,
        TraceEventType.Information,
        sourceId,
        label + ": " + LogMessages.Enumerating,
        enumerable);
#else
    {
    }
#endif

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Qactive.Log.SemanticObjectUnsafe(System.Diagnostics.TraceSource,Qactive.SemanticTrace,System.Diagnostics.TraceEventType,System.Object,System.String,System.Object)", Justification = "It's just a separator.")]
    [Conditional("TRACE")]
    public static void Enumerated(string enumerable, bool isServer, object sourceId, [CallerMemberName]string label = null)
#if TRACING
      => QactiveTraceSources.Qactive.SemanticObjectUnsafe(
          isServer
        ? SemanticTrace.ServerEnumerated
        : SemanticTrace.ClientServerEnumerated,
        TraceEventType.Information,
        sourceId,
        label + ": " + LogMessages.Enumerated,
        enumerable);
#else
    {
    }
#endif
  }
}

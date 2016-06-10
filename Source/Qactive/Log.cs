using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Security;
#if CAS
using System.Security.Permissions;
#endif

namespace Qactive
{
  // TODO: Expose log events in the portable library (and possibly the full library as well) as an IObservable<T> where T is some kind of custom, lightweight trace event object.
  internal static partial class Log
  {
#if CAS
    private static PropertyInfo debugView;

    [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
    private static object GetDebugView(Expression expression)
    {
      if (debugView == null)
      {
        debugView = typeof(Expression).GetProperty("DebugView", BindingFlags.NonPublic | BindingFlags.Instance);
      }

      return debugView.GetValue(expression);
    }
#endif

    [Conditional("DEBUG")]
    private static void DebugPrint(object expressionDebugView, string category)
      => Debug.WriteLine(Environment.NewLine + expressionDebugView, category ?? string.Empty);

#if TRACING
    public static void Unsafe(Exception exception)
    {
      new PermissionSet(PermissionState.Unrestricted).Assert();

      try
      {
        QactiveTraceSources.Qactive.TraceEvent(TraceEventType.Error, 0, exception?.ToString());
      }
      catch (Exception ex)
      {
        WriteLine(QactiveTraceSources.Qactive, $"Failed to log full exception: {exception?.Message ?? "{null}"}\r\n{ex}");
        throw;
      }
      finally
      {
        PermissionSet.RevertAssert();
      }
    }

    private static void WriteLine(this TraceSource trace, FormattableString message)
    {
      Contract.Requires(trace != null);

      trace.TraceEvent(TraceEventType.Error, 0, message?.ToString(CultureInfo.InvariantCulture));
    }

    /*
    private static void Semantic(this TraceSource trace, SemanticTrace id, TraceEventType type, string message, object data)
    {
      Contract.Requires(trace != null);

      trace.TraceEvent(type, (int)id, message + " = " + data);
    }

    private static void Semantic(this TraceSource trace, SemanticTrace id, TraceEventType type, object data)
    {
      Contract.Requires(trace != null);

      trace.TraceEvent(type, (int)id, data?.ToString());
    }

    private static void Semantic(this TraceSource trace, SemanticTrace id, TraceEventType type, string message)
    {
      Contract.Requires(trace != null);

      trace.TraceEvent(type, (int)id, message);
    }

    private static void Semantic(this TraceSource trace, SemanticTrace id, TraceEventType type, string format, params object[] args)
    {
      Contract.Requires(trace != null);

      if (format == null || args == null)
      {
        trace.Semantic(id, type, format);
      }
      else
      {
        trace.TraceEvent(type, (int)id, format, args);
      }
    }

    private static void Semantic(this TraceSource trace, SemanticTrace id, TraceEventType type, FormattableString message)
    {
      Contract.Requires(trace != null);

      trace.TraceEvent(type, (int)id, message?.ToString(CultureInfo.InvariantCulture));
    }
    */

    private static void SemanticObject(this TraceSource trace, SemanticTrace id, TraceEventType type, object objectId, string message, object data)
    {
      Contract.Requires(trace != null);

      trace.TraceEvent(type, (int)id, FormatObjectId(objectId) + " " + message + " = " + data);
    }

    private static void SemanticObjectUnsafe(this TraceSource trace, SemanticTrace id, TraceEventType type, object objectId, string message, object data)
    {
      Contract.Requires(trace != null);

#if TRACING
      new PermissionSet(PermissionState.Unrestricted).Assert();

      try
      {
#endif
        SemanticObject(trace, id, type, objectId, message, data);
#if TRACING
      }
      finally
      {
        PermissionSet.RevertAssert();
      }
#endif
    }

    /*
    private static void SemanticObject(this TraceSource trace, SemanticTrace id, TraceEventType type, object objectId, string message)
    {
      Contract.Requires(trace != null);

      trace.TraceEvent(type, (int)id, FormatObjectId(objectId) + " " + message);
    }

    private static void SemanticObject(this TraceSource trace, SemanticTrace id, TraceEventType type, object objectId, object data)
    {
      Contract.Requires(trace != null);

      trace.TraceEvent(type, (int)id, FormatObjectId(objectId) + " " + data);
    }

    private static void SemanticObject(this TraceSource trace, SemanticTrace id, TraceEventType type, object objectId, string format, params object[] args)
    {
      Contract.Requires(trace != null);

      if (format == null || args == null)
      {
        trace.SemanticObject(id, type, objectId, format);
      }
      else
      {
        trace.TraceEvent(type, (int)id, FormatObjectId(objectId) + " " + string.Format(CultureInfo.InvariantCulture, format, args));
      }
    }

    private static void SemanticObject(this TraceSource trace, SemanticTrace id, TraceEventType type, object objectId, FormattableString message)
    {
      Contract.Requires(trace != null);

      trace.TraceEvent(type, (int)id, FormatObjectId(objectId) + " " + message?.ToString(CultureInfo.InvariantCulture));
    }
    */

    private static string FormatObjectId(object value)
      => "[" + (value?.ToString() ?? "?") + "]";
#endif
  }
}

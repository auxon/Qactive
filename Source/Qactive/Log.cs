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
  internal static partial class Log
  {
    private static PropertyInfo debugView;

#if CAS
    [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
#endif
    private static object GetDebugView(Expression expression)
    {
      if (debugView == null)
      {
        debugView = typeof(Expression).GetProperty("DebugView", BindingFlags.NonPublic | BindingFlags.Instance);
      }

      return debugView.GetValue(expression);
    }

    [Conditional("DEBUG")]
    private static void DebugPrint(object expressionDebugView, string category)
      => Debug.WriteLine(Environment.NewLine + expressionDebugView, category ?? string.Empty);

#if TRACING
    public static void Unsafe(Exception exception)
    {
      new PermissionSet(PermissionState.Unrestricted).Assert();

      try
      {
        QactiveTraceSources.Qactive.TraceData(TraceEventType.Error, 0, exception);
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

      trace.TraceData(type, (int)id, message, data);
    }

    private static void Semantic(this TraceSource trace, SemanticTrace id, TraceEventType type, object data)
    {
      Contract.Requires(trace != null);

      trace.TraceData(type, (int)id, data);
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

      trace.TraceData(type, (int)id, FormatObjectId(objectId), message, data);
    }
    /*
    private static void SemanticObject(this TraceSource trace, SemanticTrace id, TraceEventType type, object objectId, object data)
    {
      Contract.Requires(trace != null);

      trace.TraceData(type, (int)id, FormatObjectId(objectId), data);
    }

    private static void SemanticObject(this TraceSource trace, SemanticTrace id, TraceEventType type, object objectId, string message)
    {
      Contract.Requires(trace != null);

      trace.TraceEvent(type, (int)id, FormatObjectId(objectId) + message);
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
        trace.TraceEvent(type, (int)id, FormatObjectId(objectId) + string.Format(CultureInfo.InvariantCulture, format, args));
      }
    }

    private static void SemanticObject(this TraceSource trace, SemanticTrace id, TraceEventType type, object objectId, FormattableString message)
    {
      Contract.Requires(trace != null);

      trace.TraceEvent(type, (int)id, FormatObjectId(objectId) + message?.ToString(CultureInfo.InvariantCulture));
    }
    */
    private static string FormatObjectId(object value)
      => "[" + (value?.ToString() ?? "?") + "] ";
#endif
  }
}

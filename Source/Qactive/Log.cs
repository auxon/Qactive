using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Security;
using System.Security.Permissions;

namespace Qactive
{
  // TODO: Use a TraceSource
  internal static class Log
  {
    private static PropertyInfo debugView;

    [Conditional("DEBUG")]
    [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
    public static void DebugPrint(Expression expression, string category)
    {
      if (debugView == null)
      {
        debugView = typeof(Expression).GetProperty("DebugView", BindingFlags.NonPublic | BindingFlags.Instance);
      }

      var value = debugView.GetValue(expression);

      Debug.WriteLine(Environment.NewLine + value, category);
    }

    public static void Unsafe(Exception exception)
    {
      new PermissionSet(PermissionState.Unrestricted).Assert();

      try
      {
        Trace.WriteLine(exception);
      }
      catch (Exception ex)
      {
        WriteLine($"Failed to log full exception: {exception.Message}\r\n{ex}");
        throw;
      }
      finally
      {
        PermissionSet.RevertAssert();
      }
    }

    public static void WriteLine(FormattableString message)
    {
      Trace.WriteLine(message.ToString(CultureInfo.InvariantCulture));
    }
  }
}

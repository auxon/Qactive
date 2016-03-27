using System;
using System.Diagnostics;
using System.Security;
using System.Security.Permissions;

namespace Qactive
{
  internal static class Log
  {
    public static void Unsafe(Exception exception)
    {
      new PermissionSet(PermissionState.Unrestricted).Assert();

      try
      {
        Debug.WriteLine(exception);
      }
      catch (Exception ex)
      {
        Debug.WriteLine($"Failed to log full exception: {exception.Message}\r\n{ex}");
        throw;
      }
      finally
      {
        PermissionSet.RevertAssert();
      }
    }
  }
}

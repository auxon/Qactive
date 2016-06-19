using System.Diagnostics.Contracts;

namespace System.Reflection
{
  internal static class PropertyInfoExtensions
  {
    public static object GetValue(this PropertyInfo info, object obj)
    {
      Contract.Requires(info != null);

      return info.GetValue(obj, null);
    }
  }
}

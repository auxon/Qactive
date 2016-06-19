using System.Diagnostics.Contracts;
using System.Linq;

namespace System.Reflection
{
  internal static class TypeExtensions
  {
    public static TAttribute GetCustomAttribute<TAttribute>(this Type type, bool inherit)
      where TAttribute : Attribute
    {
      Contract.Requires(type != null);

      return type.GetCustomAttributes(typeof(TAttribute), inherit).OfType<TAttribute>().FirstOrDefault();
    }
  }
}

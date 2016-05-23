using System.Diagnostics.Contracts;

namespace System.Reflection
{
  internal static class ReflectionExtensions
  {
    public static bool GetIsGenericType(this Type type)
    {
      Contract.Requires(type != null);

      return type.IsGenericType;
    }

    public static bool GetIsGenericTypeDefinition(this Type type)
    {
      Contract.Requires(type != null);

      return type.IsGenericTypeDefinition;
    }
  }
}

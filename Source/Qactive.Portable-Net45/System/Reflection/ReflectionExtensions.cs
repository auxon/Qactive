using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace System.Reflection
{
  internal static class ReflectionExtensions
  {
    public static MethodInfo GetMethod(this Type type, string name, BindingFlags flags)
    {
      Contract.Requires(type != null);
      Contract.Ensures(Contract.Result<MethodInfo>() != null);
    }

    public static bool GetIsGenericType(this Type type)
    {
      Contract.Requires(type != null);
    }

    public static bool GetIsGenericTypeDefinition(this Type type)
    {
      Contract.Requires(type != null);

    }

    public static Type GetGenericTypeDefinition(this Type type)
    {
      Contract.Requires(type != null);

    }

    public static IList<Type> GetGenericArguments(this Type type)
    {
      Contract.Requires(type != null);

    }

    public static IEnumerable<Type> GetInterfaces(this Type type)
    {
      Contract.Requires(type != null);

    }
  }
}

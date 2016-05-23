using System.Diagnostics.Contracts;

namespace System.Reflection
{
  internal static partial class PortableReflectionExtensions
  {
    public static Assembly GetAssembly(this Type type)
    {
      Contract.Requires(type != null);
      Contract.Ensures(Contract.Result<Assembly>() != null);

#if REFLECTION
      return type.Assembly;
#else

#endif
    }

    public static bool GetIsPrimitive(this Type type)
    {
      Contract.Requires(type != null);

#if REFLECTION
      return type.IsPrimitive;
#else

#endif
    }

    public static bool GetIsNotPublic(this Type type)
    {
      Contract.Requires(type != null);

#if REFLECTION
      return type.IsNotPublic;
#else

#endif
    }

    public static bool GetIsValueType(this Type type)
    {
      Contract.Requires(type != null);

#if REFLECTION
      return type.IsValueType;
#else

#endif
    }

    public static bool GetIsEnum(this Type type)
    {
      Contract.Requires(type != null);

#if REFLECTION
      return type.IsEnum;
#else

#endif
    }

    public static bool GetIsGenericType(this Type type)
    {
      Contract.Requires(type != null);

#if REFLECTION
      return type.IsGenericType;
#else

#endif
    }

    public static bool GetIsGenericTypeDefinition(this Type type)
    {
      Contract.Requires(type != null);

#if REFLECTION
      return type.IsGenericTypeDefinition;
#else

#endif
    }
  }
}

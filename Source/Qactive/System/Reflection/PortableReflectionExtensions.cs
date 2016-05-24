using System.Diagnostics.Contracts;

namespace System.Reflection
{
  public static partial class PortableReflectionExtensions
  {
    public static Assembly GetAssembly(this Type type)
    {
      Contract.Requires(type != null);
      Contract.Ensures(Contract.Result<Assembly>() != null);

#if REFLECTION
      return type.Assembly;
#else
      return type.GetTypeInfo().Assembly;
#endif
    }

    [Pure]
    public static bool GetIsPrimitive(this Type type)
    {
      Contract.Requires(type != null);

#if REFLECTION
      Contract.Ensures(Contract.Result<bool>() == type.IsPrimitive);

      return type.IsPrimitive;
#else
      return type.GetTypeInfo().IsPrimitive;
#endif
    }

    [Pure]
    public static bool GetIsNotPublic(this Type type)
    {
      Contract.Requires(type != null);

#if REFLECTION
      Contract.Ensures(Contract.Result<bool>() == type.IsNotPublic);

      return type.IsNotPublic;
#else
      return type.GetTypeInfo().IsNotPublic;
#endif
    }

    [Pure]
    public static bool GetIsValueType(this Type type)
    {
      Contract.Requires(type != null);

#if REFLECTION
      Contract.Ensures(Contract.Result<bool>() == type.IsValueType);

      return type.IsValueType;
#else
      return type.GetTypeInfo().IsValueType;
#endif
    }

    [Pure]
    public static bool GetIsEnum(this Type type)
    {
      Contract.Requires(type != null);

#if REFLECTION
      Contract.Ensures(Contract.Result<bool>() == type.IsEnum);

      return type.IsEnum;
#else
      return type.GetTypeInfo().IsEnum;
#endif
    }

    [Pure]
    public static bool GetIsGenericType(this Type type)
    {
      Contract.Requires(type != null);

#if REFLECTION
      Contract.Ensures(Contract.Result<bool>() == type.IsGenericType);

      return type.IsGenericType;
#else
      return type.GetTypeInfo().IsGenericType;
#endif
    }

    [Pure]
    public static bool GetIsGenericTypeDefinition(this Type type)
    {
      Contract.Requires(type != null);

#if REFLECTION
      Contract.Ensures(Contract.Result<bool>() == type.IsGenericTypeDefinition);

      return type.IsGenericTypeDefinition;
#else
      return type.GetTypeInfo().IsGenericTypeDefinition;
#endif
    }
  }
}

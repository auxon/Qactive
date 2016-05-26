using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace System.Reflection
{
  partial class PortableReflectionExtensions
  {
    internal static TAttribute GetCustomAttribute<TAttribute>(this Type type, bool inherit)
      where TAttribute : Attribute
    {
      Contract.Requires(type != null);

      return type.GetTypeInfo().GetCustomAttribute<TAttribute>(inherit);
    }

    internal static IEnumerable<Attribute> GetCustomAttributes(this Type type, Type attributeType, bool inherit)
    {
      Contract.Requires(type != null);
      Contract.Requires(attributeType != null);
      Contract.Ensures(Contract.Result<IEnumerable<Attribute>>() != null);

      return type.GetTypeInfo().GetCustomAttributes(attributeType, inherit);
    }

    internal static InterfaceMapping GetInterfaceMap(this Type type, Type interfaceType)
    {
      Contract.Requires(type != null);
      Contract.Requires(interfaceType != null);

      return type.GetTypeInfo().GetRuntimeInterfaceMap(interfaceType);
    }

    internal static PropertyInfo[] GetProperties(this Type type)
    {
      Contract.Requires(type != null);
      Contract.Ensures(Contract.Result<PropertyInfo[]>() != null);

      return type.GetTypeInfo().DeclaredProperties.ToArray();
    }

    internal static IEnumerable<MethodInfo> GetMethods(this Type type)
    {
      Contract.Requires(type != null);
      Contract.Ensures(Contract.Result<IEnumerable<MethodInfo>>() != null);

      return GetMethods(type, BindingFlags.Default);
    }

    [Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "_", Justification = "Required to match the signature of the same method in the FCL.")]
    internal static IEnumerable<MethodInfo> GetMethods(this Type type, BindingFlags flags)
    {
      Contract.Requires(type != null);
      Contract.Ensures(Contract.Result<IEnumerable<MethodInfo>>() != null);

      return type.GetTypeInfo().DeclaredMethods.Where(method => ShouldBind(flags, method.IsPublic, method.IsStatic));
    }

    internal static MethodInfo GetMethod(this Type type, string name)
    {
      Contract.Requires(type != null);
      Contract.Requires(name != null);

      return GetMethod(type, name, BindingFlags.Default);
    }

    [Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "_", Justification = "Required to match the signature of the same method in the FCL.")]
    internal static MethodInfo GetMethod(this Type type, string name, BindingFlags flags)
    {
      Contract.Requires(type != null);
      Contract.Requires(name != null);

      return type.GetTypeInfo().DeclaredMethods.FirstOrDefault(method => method.Name == name && ShouldBind(flags, method.IsPublic, method.IsStatic));
    }

    internal static ConstructorInfo[] GetConstructors(this Type type)
    {
      Contract.Requires(type != null);
      Contract.Ensures(Contract.Result<ConstructorInfo[]>() != null);

      return GetConstructors(type, BindingFlags.Default);
    }

    [Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "_", Justification = "Required to match the signature of the same method in the FCL.")]
    internal static ConstructorInfo[] GetConstructors(this Type type, BindingFlags flags)
    {
      Contract.Requires(type != null);
      Contract.Ensures(Contract.Result<ConstructorInfo[]>() != null);

      return type.GetTypeInfo().DeclaredConstructors.Where(ctor => ShouldBind(flags, ctor.IsPublic, ctor.IsStatic)).ToArray();
    }

    internal static ConstructorInfo GetConstructor(this Type type, params Type[] parameters)
    {
      Contract.Requires(type != null);

      return GetConstructor(type, BindingFlags.Default, null, parameters, null);
    }

    [Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "_", Justification = "Required to match the signature of the same method in the FCL.")]
    [Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "__", Justification = "Required to match the signature of the same method in the FCL.")]
    [Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "___", Justification = "Required to match the signature of the same method in the FCL.")]
    internal static ConstructorInfo GetConstructor(this Type type, BindingFlags flags, object __, Type[] parameters, object ___)
    {
      Contract.Requires(type != null);

      return type.GetTypeInfo().DeclaredConstructors.FirstOrDefault(ctor =>
           ShouldBind(flags, ctor.IsPublic, ctor.IsStatic)
        && (parameters?.SequenceEqual(from parameter in ctor.GetParameters()
                                      select parameter.ParameterType)
                     ?? ctor.GetParameters().Length == 0));
    }

    [Pure]
    public static bool IsAssignableFrom(this Type type, Type other)
    {
      Contract.Requires(type != null);
      Contract.Requires(other != null);

      return type.GetTypeInfo().IsAssignableFrom(other.GetTypeInfo());
    }

    internal static Type[] GetGenericArguments(this Type type)
    {
      Contract.Requires(type != null);
      Contract.Ensures(Contract.Result<Type[]>() != null);

      return type.GetTypeInfo().GenericTypeArguments;
    }

    internal static IEnumerable<Type> GetInterfaces(this Type type)
    {
      Contract.Requires(type != null);
      Contract.Ensures(Contract.Result<IEnumerable<Type>>() != null);

      return type.GetTypeInfo().ImplementedInterfaces;
    }

    private static bool ShouldBind(BindingFlags flags, bool isPublic, bool isStatic)
      => (!flags.HasFlag(BindingFlags.Static) || isStatic)
      && (!flags.HasFlag(BindingFlags.Instance) || !isStatic)
      && (!flags.HasFlag(BindingFlags.Public) || isPublic)
      && (!flags.HasFlag(BindingFlags.NonPublic) || !isPublic);
  }
}

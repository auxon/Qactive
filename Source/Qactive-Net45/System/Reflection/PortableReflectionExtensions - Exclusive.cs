using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;

namespace System.Reflection
{
  partial class PortableReflectionExtensions
  {
    [Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Bug in Code Analysis probably due to use of the when keyword. The Exception type is not being caught here.")]
    internal static Type GetType(this Assembly assembly, string name, bool throwOnError)
    {
      Contract.Requires(assembly != null);
      Contract.Requires(name != null);

      try
      {
        return assembly.GetType(name);
      }
      catch (BadImageFormatException) when (!throwOnError)
      {
        return null;
      }
      catch (IOException) when (!throwOnError)
      {
        return null;
      }
      catch (ArgumentException) when (!throwOnError)
      {
        return null;
      }
    }

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

    [Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "_", Justification = "Required to match the signature of the same method in the FCL.")]
    internal static FieldInfo GetField(this Type type, string name, BindingFlags _)
    {
      Contract.Requires(type != null);
      Contract.Requires(name != null);

      return type.GetTypeInfo().GetDeclaredField(name);
    }

    internal static IEnumerable<MethodInfo> GetMethods(this Type type)
    {
      Contract.Requires(type != null);
      Contract.Ensures(Contract.Result<IEnumerable<MethodInfo>>() != null);

      return GetMethods(type, BindingFlags.Default);
    }

    [Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "_", Justification = "Required to match the signature of the same method in the FCL.")]
    internal static IEnumerable<MethodInfo> GetMethods(this Type type, BindingFlags _)
    {
      Contract.Requires(type != null);
      Contract.Ensures(Contract.Result<IEnumerable<MethodInfo>>() != null);

      return type.GetTypeInfo().DeclaredMethods;
    }

    internal static MethodInfo GetMethod(this Type type, string name)
    {
      Contract.Requires(type != null);
      Contract.Requires(name != null);

      return GetMethod(type, name, BindingFlags.Default);
    }

    [Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "_", Justification = "Required to match the signature of the same method in the FCL.")]
    internal static MethodInfo GetMethod(this Type type, string name, BindingFlags _)
    {
      Contract.Requires(type != null);
      Contract.Requires(name != null);

      return type.GetTypeInfo().GetDeclaredMethod(name);
    }

    internal static MethodInfo GetMethod(this Type type, string name, params Type[] parameters)
    {
      Contract.Requires(type != null);
      Contract.Requires(name != null);

      return type.GetTypeInfo().GetDeclaredMethods(name).FirstOrDefault(method =>
        parameters?.SequenceEqual(from parameter in method.GetParameters()
                                  select parameter.ParameterType)
                 ?? method.GetParameters().Length == 0);
    }

    internal static ConstructorInfo[] GetConstructors(this Type type)
    {
      Contract.Requires(type != null);
      Contract.Ensures(Contract.Result<ConstructorInfo[]>() != null);

      return GetConstructors(type, BindingFlags.Default);
    }

    [Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "_", Justification = "Required to match the signature of the same method in the FCL.")]
    internal static ConstructorInfo[] GetConstructors(this Type type, BindingFlags _)
    {
      Contract.Requires(type != null);
      Contract.Ensures(Contract.Result<ConstructorInfo[]>() != null);

      return type.GetTypeInfo().DeclaredConstructors.ToArray();
    }

    internal static ConstructorInfo GetConstructor(this Type type, params Type[] parameters)
    {
      Contract.Requires(type != null);

      return GetConstructor(type, BindingFlags.Default, null, parameters, null);
    }

    [Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "_", Justification = "Required to match the signature of the same method in the FCL.")]
    [Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "__", Justification = "Required to match the signature of the same method in the FCL.")]
    [Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "___", Justification = "Required to match the signature of the same method in the FCL.")]
    internal static ConstructorInfo GetConstructor(this Type type, BindingFlags _, object __, Type[] parameters, object ___)
    {
      Contract.Requires(type != null);

      return type.GetTypeInfo().DeclaredConstructors.FirstOrDefault(ctor =>
        parameters?.SequenceEqual(from parameter in ctor.GetParameters()
                                  select parameter.ParameterType)
                 ?? ctor.GetParameters().Length == 0);
    }

    [Pure]
    public static bool IsAssignableFrom(this Type type, Type other)
    {
      Contract.Requires(type != null);
      Contract.Requires(other != null);

      return type.GetTypeInfo().IsAssignableFrom(other.GetTypeInfo());
    }

    [Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "_", Justification = "Required to match the signature of the same method in the FCL.")]
    internal static Type GetNestedType(this Type type, string name, BindingFlags _)
    {
      Contract.Requires(type != null);
      Contract.Requires(name != null);

      return type.GetTypeInfo().GetDeclaredNestedType(name)?.AsType();
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
  }
}

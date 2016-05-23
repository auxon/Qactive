using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;

namespace System.Reflection
{
  partial class PortableReflectionExtensions
  {
    public static Type GetType(this Assembly assembly, string name, bool throwOnError)
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

    public static TAttribute GetCustomAttribute<TAttribute>(this Type type, bool inherit)
      where TAttribute : Attribute
    {
      Contract.Requires(type != null);

      return type.GetTypeInfo().GetCustomAttribute<TAttribute>(inherit);
    }

    public static IEnumerable<Attribute> GetCustomAttributes(this Type type, Type attributeType, bool inherit)
    {
      Contract.Requires(type != null);
      Contract.Requires(attributeType != null);
      Contract.Ensures(Contract.Result<IEnumerable<Attribute>>() != null);

      return type.GetTypeInfo().GetCustomAttributes(attributeType, inherit);
    }

    public static InterfaceMapping GetInterfaceMap(this Type type, Type interfaceType)
    {
      Contract.Requires(type != null);
      Contract.Requires(interfaceType != null);

      return type.GetTypeInfo().GetRuntimeInterfaceMap(interfaceType);
    }

    public static PropertyInfo[] GetProperties(this Type type)
    {
      Contract.Requires(type != null);
      Contract.Ensures(Contract.Result<PropertyInfo[]>() != null);

      return type.GetTypeInfo().DeclaredProperties.ToArray();
    }

    public static PropertyInfo GetProperty(this Type type, string name, BindingFlags _)
    {
      Contract.Requires(type != null);
      Contract.Requires(name != null);

      return type.GetTypeInfo().GetDeclaredProperty(name);
    }

    public static FieldInfo GetField(this Type type, string name, BindingFlags _)
    {
      Contract.Requires(type != null);
      Contract.Requires(name != null);

      return type.GetTypeInfo().GetDeclaredField(name);
    }

    public static IEnumerable<MethodInfo> GetMethods(this Type type)
    {
      Contract.Requires(type != null);
      Contract.Ensures(Contract.Result<IEnumerable<MethodInfo>>() != null);

      return GetMethods(type, BindingFlags.Default);
    }

    public static IEnumerable<MethodInfo> GetMethods(this Type type, BindingFlags _)
    {
      Contract.Requires(type != null);
      Contract.Ensures(Contract.Result<IEnumerable<MethodInfo>>() != null);

      return type.GetTypeInfo().DeclaredMethods;
    }

    public static MethodInfo GetMethod(this Type type, string name)
    {
      Contract.Requires(type != null);
      Contract.Requires(name != null);

      return GetMethod(type, name, BindingFlags.Default);
    }

    public static MethodInfo GetMethod(this Type type, string name, BindingFlags _)
    {
      Contract.Requires(type != null);
      Contract.Requires(name != null);

      return type.GetTypeInfo().GetDeclaredMethod(name);
    }

    public static MethodInfo GetMethod(this Type type, string name, params Type[] parameters)
    {
      Contract.Requires(type != null);
      Contract.Requires(name != null);

      return type.GetTypeInfo().GetDeclaredMethods(name).FirstOrDefault(method =>
        parameters?.SequenceEqual(from parameter in method.GetParameters()
                                  select parameter.ParameterType)
                 ?? method.GetParameters().Length == 0);
    }

    public static ConstructorInfo[] GetConstructors(this Type type)
    {
      Contract.Requires(type != null);
      Contract.Ensures(Contract.Result<ConstructorInfo[]>() != null);

      return GetConstructors(type, BindingFlags.Default);
    }

    public static ConstructorInfo[] GetConstructors(this Type type, BindingFlags _)
    {
      Contract.Requires(type != null);
      Contract.Ensures(Contract.Result<ConstructorInfo[]>() != null);

      return type.GetTypeInfo().DeclaredConstructors.ToArray();
    }

    public static ConstructorInfo GetConstructor(this Type type, params Type[] parameters)
    {
      Contract.Requires(type != null);

      return GetConstructor(type, BindingFlags.Default, null, parameters, null);
    }

    public static ConstructorInfo GetConstructor(this Type type, BindingFlags flags, object _, Type[] parameters, object __)
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

    public static Type GetNestedType(this Type type, string name, BindingFlags _)
    {
      Contract.Requires(type != null);
      Contract.Requires(name != null);

      return type.GetTypeInfo().GetDeclaredNestedType(name)?.AsType();
    }

    public static Type GetGenericTypeDefinition(this Type type)
    {
      Contract.Requires(type != null);

      return type.GetTypeInfo().GetGenericTypeDefinition();
    }

    public static Type[] GetGenericArguments(this Type type)
    {
      Contract.Requires(type != null);
      Contract.Ensures(Contract.Result<Type[]>() != null);

      return type.GetTypeInfo().GenericTypeArguments;
    }

    public static IEnumerable<Type> GetInterfaces(this Type type)
    {
      Contract.Requires(type != null);
      Contract.Ensures(Contract.Result<IEnumerable<Type>>() != null);

      return type.GetTypeInfo().ImplementedInterfaces;
    }
  }
}

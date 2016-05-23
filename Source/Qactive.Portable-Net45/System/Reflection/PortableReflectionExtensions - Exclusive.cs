using System.Diagnostics.Contracts;

namespace System.Reflection
{
  partial class PortableReflectionExtensions
  {
    public static Type GetType(this Assembly assembly, string typeName, bool throwOnError, bool ignoreCase)
    {
      Contract.Requires(assembly != null);
      Contract.Requires(typeName != null);
      Contract.Ensures(Contract.Result<Type>() != null);


    }

    public static TAttribute GetCustomAttribute<TAttribute>(this Type type, bool inherit)
      where TAttribute : Attribute
    {
      Contract.Requires(type != null);
    }

    public static Attribute[] GetCustomAttributes(this Type type, Type attributeType, bool inherit)
    {
      Contract.Requires(type != null);
      Contract.Requires(attributeType != null);

    }

    public static InterfaceMapping GetInterfaceMap(this Type type, Type interfaceType)
    {
      Contract.Requires(type != null);
      Contract.Requires(interfaceType != null);
    }

    public static PropertyInfo[] GetProperties(this Type type)
    {
      Contract.Requires(type != null);
      Contract.Ensures(Contract.Result<PropertyInfo[]>() != null);

    }

    public static PropertyInfo GetProperty(this Type type, string name, BindingFlags flags)
    {
      Contract.Requires(type != null);
      Contract.Requires(name != null);

    }

    public static FieldInfo GetField(this Type type, string name, BindingFlags flags)
    {
      Contract.Requires(type != null);
      Contract.Requires(name != null);

    }

    public static MethodInfo[] GetMethods(this Type type)
    {
      Contract.Requires(type != null);

      return GetMethods(type, BindingFlags.Public | BindingFlags.Instance);
    }

    public static MethodInfo[] GetMethods(this Type type, BindingFlags flags)
    {
      Contract.Requires(type != null);

    }

    public static MethodInfo GetMethod(this Type type, string name)
    {
      Contract.Requires(type != null);
      Contract.Ensures(Contract.Result<MethodInfo>() != null);

      return GetMethod(type, name, BindingFlags.Public | BindingFlags.Instance);
    }

    public static MethodInfo GetMethod(this Type type, string name, BindingFlags flags)
    {
      Contract.Requires(type != null);
      Contract.Ensures(Contract.Result<MethodInfo>() != null);

    }

    public static MethodInfo GetMethod(this Type type, string name, params Type[] parameters)
    {
      Contract.Requires(type != null);
      Contract.Requires(name != null);

    }

    public static ConstructorInfo[] GetConstructors(this Type type)
    {
      Contract.Requires(type != null);
      Contract.Ensures(Contract.Result<ConstructorInfo[]>() != null);

      return GetConstructors(type, BindingFlags.Public | BindingFlags.Instance);
    }

    public static ConstructorInfo[] GetConstructors(this Type type, BindingFlags flags)
    {
      Contract.Requires(type != null);

    }

    public static ConstructorInfo GetConstructor(this Type type, params Type[] parameters)
    {
      Contract.Requires(type != null);

      return GetConstructor(type, BindingFlags.Public | BindingFlags.Instance, null, parameters, null);
    }

    public static ConstructorInfo GetConstructor(this Type type, BindingFlags flags, object _, Type[] parameters, object __)
    {
      Contract.Requires(type != null);

    }

    public static bool IsAssignableFrom(this Type type, Type other)
    {
      Contract.Requires(type != null);
      Contract.Requires(other != null);

    }

    public static Type GetNestedType(this Type type, string name, BindingFlags flags)
    {
      Contract.Requires(type != null);
      Contract.Requires(name != null);

    }

    public static Type GetGenericTypeDefinition(this Type type)
    {
      Contract.Requires(type != null);

    }

    public static Type[] GetGenericArguments(this Type type)
    {
      Contract.Requires(type != null);

    }

    public static Type[] GetInterfaces(this Type type)
    {
      Contract.Requires(type != null);

    }
  }
}

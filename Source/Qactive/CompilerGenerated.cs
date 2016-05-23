using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Qactive
{
#if SERIALIZATION
  [Serializable]
#endif
  internal sealed class CompilerGenerated
  {
    private static readonly ConstructorInfo constructor = typeof(CompilerGenerated).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(KeyValuePair<string, object>[]) }, null);
    private static readonly ConstructorInfo propertyConstructor = typeof(KeyValuePair<string, object>).GetConstructor(new[] { typeof(string), typeof(object) });
    private static readonly MethodInfo getPropertyMethod = typeof(CompilerGenerated).GetMethods().Where(m => m.Name == "GetProperty").First();
    private static readonly MethodInfo setPropertyMethod = typeof(CompilerGenerated).GetMethod("SetProperty");

    private readonly Dictionary<string, object> properties = new Dictionary<string, object>();

    public CompilerGenerated()
    {
    }

    private CompilerGenerated(KeyValuePair<string, object>[] properties)
    {
      Contract.Requires(properties != null);

      foreach (var property in properties)
      {
        this.properties.Add(property.Key, property.Value);
      }
    }

    [ContractInvariantMethod]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
    private void ObjectInvariant()
    {
      Contract.Invariant(properties != null);
    }

    public static NewExpression New(IEnumerable<MemberInfo> members, IEnumerable<Expression> arguments)
    {
      Contract.Requires(members != null);
      Contract.Requires(arguments != null);
      Contract.Ensures(Contract.Result<NewExpression>() != null);

      return Expression.New(
        constructor,
        Expression.NewArrayInit(
          typeof(KeyValuePair<string, object>),
          members.Zip(arguments, (property, argument) =>
            Expression.New(propertyConstructor,
              Expression.Constant(property.Name),
              Expression.Convert(argument, typeof(object))))));
    }

    public static MethodCallExpression Get(Expression instance, MemberInfo member, Func<Type, Type> updateGenericTypeArguments)
    {
      Contract.Requires(instance != null);
      Contract.Requires(member != null);
      Contract.Requires(updateGenericTypeArguments != null);
      Contract.Ensures(Contract.Result<MethodCallExpression>() != null);

      string name;
      Type type;

      var property = member as PropertyInfo;

      if (property != null)
      {
        name = property.Name;
        type = updateGenericTypeArguments(property.PropertyType);
      }
      else
      {
        var field = (FieldInfo)member;

        name = field.Name;
        type = updateGenericTypeArguments(field.FieldType);
      }

      return Expression.Call(instance, getPropertyMethod.MakeGenericMethod(type), Expression.Constant(name));
    }

    public static MethodCallExpression Set(Expression left, Expression right)
    {
      Contract.Requires(left != null);
      Contract.Requires(right != null);
      Contract.Ensures(Contract.Result<MethodCallExpression>() != null);

      var member = (MemberExpression)left;
      var property = (PropertyInfo)member.Member;

      return Expression.Call(member, setPropertyMethod, Expression.Constant(property.Name), right);
    }

    public T GetProperty<T>(string name)
    {
      Contract.Requires(!string.IsNullOrEmpty(name));

      return (T)properties[name];
    }

    public void SetProperty(string name, object value)
    {
      Contract.Requires(!string.IsNullOrEmpty(name));

      properties[name] = value;
    }
  }
}
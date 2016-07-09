using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;

namespace Qactive.Expressions
{
  public sealed class ExpressionEqualityComparer : IEqualityComparer<Expression>
  {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "It's immutable.")]
    public static readonly ExpressionEqualityComparer Exact = new ExpressionEqualityComparer(reflectionNamesOnly: false);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "It's immutable.")]
    public static readonly ExpressionEqualityComparer ReflectionNamesOnly = new ExpressionEqualityComparer(reflectionNamesOnly: true);

    private readonly bool reflectionNamesOnly;

    private ExpressionEqualityComparer(bool reflectionNamesOnly)
    {
      this.reflectionNamesOnly = reflectionNamesOnly;
    }

    public bool Equals(Expression x, Expression y)
      => Equals(x, y, new EqualityExpressionVisitor(ShallowEquals, TypeEquals, MemberEquals));

    public static bool Equals(Expression first, Expression second, EqualityExpressionVisitor visitor)
    {
      Contract.Requires(visitor != null);

      return visitor.ShallowEquals(first, second) && DeepEquals(first, second, visitor);
    }

    public bool ShallowEquals(Expression first, Expression second)
      => NullsOrEquals(first, second, (f, s) => f.NodeType == s.NodeType && TypeEquals(GetRepresentativeType(f), GetRepresentativeType(s)));

    public static Type GetRepresentativeType(Expression expression)
    {
      Contract.Requires(expression != null);
      Contract.Ensures(Contract.Result<Type>() != null);

      var type = expression.GetType();

      if (type != typeof(Expression))
      {
        while (type.BaseType != typeof(Expression))
        {
          type = type.BaseType;
        }
      }

      return type;
    }

    private static bool DeepEquals(Expression first, Expression second, EqualityExpressionVisitor visitor)
    {
      Contract.Requires((first == null && second == null) || (first != null && second != null));
      Contract.Requires(visitor != null);

      if (first == null)
      {
        return true;
      }

      visitor.SetOtherRoot(second);
      visitor.Visit(first);

      Contract.Assume(visitor.StackCount == 1);

      return visitor.AreEqual;
    }

    public int GetHashCode(Expression obj)
      => obj == null ? 0 : obj.GetType().AssemblyQualifiedName.GetHashCode();

    internal static bool NullsOrEquals<T>(T first, T second, Func<T, T, bool> comparer)
      => (first == null && second == null)
      || (first != null && second != null && comparer(first, second));

    internal bool TypeEquals(Type first, Type second)
      => reflectionNamesOnly ? first.AssemblyQualifiedName == second.AssemblyQualifiedName : first == second;

    internal bool MemberEquals(MemberInfo first, MemberInfo second)
      => reflectionNamesOnly ? first.DeclaringType == second.DeclaringType && first.Name == second.Name : first == second;
  }
}


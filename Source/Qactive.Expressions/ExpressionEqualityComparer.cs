using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;

namespace Qactive.Expressions
{
  public sealed class ExpressionEqualityComparer : IEqualityComparer<Expression>
  {
    public static readonly ExpressionEqualityComparer Exact = new ExpressionEqualityComparer(reflectionNamesOnly: false);
    public static readonly ExpressionEqualityComparer ReflectionNamesOnly = new ExpressionEqualityComparer(reflectionNamesOnly: true);

    private readonly bool reflectionNamesOnly;

    private ExpressionEqualityComparer(bool reflectionNamesOnly)
    {
      this.reflectionNamesOnly = reflectionNamesOnly;
    }

    public bool Equals(Expression x, Expression y)
      => Equals(x, y, new EqualityExpressionVisitor(ShallowEquals, TypeEquals, MemberEquals));

    public bool Equals(Expression x, Expression y, EqualityExpressionVisitor visitor)
      => visitor.ShallowEquals(x, y) && DeepEquals(x, y, visitor);

    public bool ShallowEquals(Expression x, Expression y)
      => NullsOrEquals(x, y, (f, s) => f.NodeType == s.NodeType && TypeEquals(GetRepresentativeType(f), GetRepresentativeType(s)));

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

    private bool DeepEquals(Expression x, Expression y, EqualityExpressionVisitor visitor)
    {
      Contract.Requires((x == null && y == null) || (x != null && y != null));
      Contract.Requires(visitor != null);

      if (x == null && y == null)
      {
        return true;
      }

      visitor.SetOtherRoot(y);
      visitor.Visit(x);

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


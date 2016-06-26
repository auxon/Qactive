using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using Qactive.Expressions;

namespace Qactive.Tests
{
  internal sealed class TestExpressionEqualityComparer : IEqualityComparer<Expression>
  {
    private readonly bool reflectionNamesOnly;
    private readonly TestEqualityExpressionVisitor visitor;

#if READONLYCOLLECTIONS
    public IReadOnlyCollection<Expression> InequalityNodes => visitor.InequalityNodes;

    public IReadOnlyCollection<Expression> InequalityOthers => visitor.InequalityOthers;
#else
    public ReadOnlyCollection<Expression> InequalityNodes => visitor.InequalityNodes;

    public ReadOnlyCollection<Expression> InequalityOthers => visitor.InequalityOthers;
#endif

    public TestExpressionEqualityComparer(bool reflectionNamesOnly)
    {
      this.reflectionNamesOnly = reflectionNamesOnly;

      visitor = new TestEqualityExpressionVisitor(ShallowEquals, TypeEquals, MemberEquals);
    }

    public bool Equals(Expression x, Expression y)
       => (x == null && y == null)
       || (x != null && y != null
          && (reflectionNamesOnly
              ? ExpressionEqualityComparer.ReflectionNamesOnly.Equals(x, y, visitor)
              : ExpressionEqualityComparer.Exact.Equals(x, y, visitor)));

    private bool ShallowEquals(Expression x, Expression y)
      => (x == null && y == null)
      || (x != null && y != null
          && ((x.NodeType == y.NodeType && TypeEquals(ExpressionEqualityComparer.GetRepresentativeType(x), ExpressionEqualityComparer.GetRepresentativeType(y)))
            || x.IsAny(y.NodeType, y.Type)
            || y.IsAny(x.NodeType, x.Type)));

    private bool TypeEquals(Type first, Type second)
      => first == Any.Type || second == Any.Type || (reflectionNamesOnly ? first.AssemblyQualifiedName == second.AssemblyQualifiedName : first == second);

    private bool MemberEquals(MemberInfo first, MemberInfo second)
      => first == Any.Member || second == Any.Member || (reflectionNamesOnly ? first.DeclaringType == second.DeclaringType && first.Name == second.Name : first == second);

    public int GetHashCode(Expression obj)
      => reflectionNamesOnly
       ? ExpressionEqualityComparer.ReflectionNamesOnly.GetHashCode(obj)
       : ExpressionEqualityComparer.Exact.GetHashCode(obj);
  }
}

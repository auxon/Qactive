using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;
using Qactive.Expressions;

namespace Qactive.Tests
{
  internal sealed class TestEqualityExpressionVisitor : EqualityExpressionVisitor
  {
    public TestEqualityExpressionVisitor(Func<Expression, Expression, bool> shallowEquals, Func<Type, Type, bool> typeEquals, Func<MemberInfo, MemberInfo, bool> memberEquals)
      : base(shallowEquals, typeEquals, memberEquals)
    {
      Contract.Requires(shallowEquals != null);
      Contract.Requires(typeEquals != null);
      Contract.Requires(memberEquals != null);
    }

    protected override bool? AreEqualShortCircuit(Expression node, Expression other)
      => node.IsAny(other.NodeType, other.Type)
      || other.IsAny(node.NodeType, node.Type)
       ? true
       : (bool?)null;
  }
}

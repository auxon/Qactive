using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;

namespace Qactive.Expressions
{
  [Serializable]
  internal sealed class SerializableMemberExpression : SerializableExpression
  {
    public readonly Tuple<MemberInfo, Type[]> Member;
    public readonly SerializableExpression Expr;

    public SerializableMemberExpression(MemberExpression expression, SerializableExpressionConverter converter)
      : base(expression)
    {
      Contract.Requires(expression != null);
      Contract.Requires(converter != null);

      Expr = converter.TryConvert(expression.Expression);
      Member = converter.Convert(expression.Member);
    }

    internal override Expression Convert() => Expression.MakeMemberAccess(
                                                Expr.TryConvert(),
                                                SerializableExpressionConverter.Convert(Member));
  }
}
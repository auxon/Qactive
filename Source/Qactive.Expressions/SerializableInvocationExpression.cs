using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace Qactive.Expressions
{
  [Serializable]
  internal sealed class SerializableInvocationExpression : SerializableExpression
  {
    public readonly IList<SerializableExpression> Arguments;
    public readonly SerializableExpression Expr;

    public SerializableInvocationExpression(InvocationExpression expression, SerializableExpressionConverter converter)
      : base(expression)
    {
      Contract.Requires(expression != null);
      Contract.Requires(converter != null);

      Arguments = converter.TryConvert(expression.Arguments);
      Expr = converter.TryConvert(expression.Expression);
    }

    internal override Expression Convert() => Expression.Invoke(
                                                Expr.TryConvert(),
                                                Arguments.TryConvert());
  }
}
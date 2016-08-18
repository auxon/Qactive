using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace Qactive.Expressions
{
  [Serializable]
  internal sealed class SerializableBlockExpression : SerializableExpression
  {
    public readonly IList<SerializableExpression> Expressions;
    public readonly SerializableExpression Result;
    public readonly IList<SerializableParameterExpression> Variables;

    public SerializableBlockExpression(BlockExpression expression, SerializableExpressionConverter converter)
      : base(expression)
    {
      Contract.Requires(expression != null);
      Contract.Requires(converter != null);

      Expressions = converter.TryConvert(expression.Expressions);
      Result = converter.TryConvert(expression.Result);
      Variables = converter.TryConvert<SerializableParameterExpression>(expression.Variables);
    }

    internal override void Accept(SerializableExpressionVisitor visitor)
      => visitor.VisitBlock(this);

    internal override Expression ConvertBack()
      => Expression.Block(
          Result.Type,
          Variables.TryConvert<ParameterExpression>(),
          Expressions.TryConvert());
  }
}
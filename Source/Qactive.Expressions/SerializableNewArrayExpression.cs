using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace Qactive.Expressions
{
  [Serializable]
  internal sealed class SerializableNewArrayExpression : SerializableExpression
  {
    public readonly IList<SerializableExpression> Expressions;

    public SerializableNewArrayExpression(NewArrayExpression expression, SerializableExpressionConverter converter)
      : base(expression)
    {
      Contract.Requires(expression != null);
      Contract.Requires(converter != null);

      Expressions = converter.TryConvert(expression.Expressions);
    }

    internal override Expression Convert() => Expression.NewArrayInit(
                                                Type.GetElementType(),
                                                Expressions.TryConvert());
  }
}
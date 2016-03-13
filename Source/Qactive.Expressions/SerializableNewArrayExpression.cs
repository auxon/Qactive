using System;
using System.Collections.Generic;
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
      Expressions = converter.Convert(expression.Expressions);
    }

    internal override Expression Convert() => Expression.NewArrayInit(
                                                Type.GetElementType(),
                                                Expressions.TryConvert());
  }
}
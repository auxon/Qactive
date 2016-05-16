using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace Qactive.Expressions
{
  [Serializable]
  internal sealed class SerializableConditionalExpression : SerializableExpression
  {
    public readonly SerializableExpression IfFalse;
    public readonly SerializableExpression IfTrue;
    public readonly SerializableExpression Test;

    public SerializableConditionalExpression(ConditionalExpression expression, SerializableExpressionConverter converter)
      : base(expression)
    {
      Contract.Requires(expression != null);
      Contract.Requires(converter != null);

      IfFalse = converter.TryConvert(expression.IfFalse);
      IfTrue = converter.TryConvert(expression.IfTrue);
      Test = converter.TryConvert(expression.Test);
    }

    internal override Expression Convert() => Expression.Condition(
                                                Test.TryConvert(),
                                                IfTrue.TryConvert(),
                                                IfFalse.TryConvert(),
                                                Type);
  }
}
using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace Qactive.Expressions
{
  [Serializable]
  internal sealed class SerializableConstantExpression : SerializableExpression
  {
    public readonly object Value;

    public SerializableConstantExpression(ConstantExpression expression)
      : base(expression)
    {
      Contract.Requires(expression != null);

      Value = expression.Value;
    }

    internal override void Accept(SerializableExpressionVisitor visitor)
      => visitor.VisitConstant(this);

    internal override Expression ConvertBack()
      => Expression.Constant(
          Value,
          Type);
  }
}
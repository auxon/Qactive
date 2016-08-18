using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace Qactive.Expressions
{
  [Serializable]
  internal sealed class SerializableTypeBinaryExpression : SerializableExpression
  {
    public readonly SerializableExpression Expr;
    public readonly Type TypeOperand;

    public SerializableTypeBinaryExpression(TypeBinaryExpression expression, SerializableExpressionConverter converter)
      : base(expression)
    {
      Contract.Requires(expression != null);
      Contract.Requires(converter != null);

      Expr = converter.TryConvert(expression.Expression);
      TypeOperand = expression.TypeOperand;
    }

    internal override void Accept(SerializableExpressionVisitor visitor)
      => visitor.VisitTypeBinary(this);

    internal override Expression ConvertBack()
      => Expression.TypeIs(
          Expr.TryConvertBack(),
          TypeOperand);
  }
}
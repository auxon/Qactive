using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;

namespace Qactive.Expressions
{
  [Serializable]
  internal sealed class SerializableUnaryExpression : SerializableExpression
  {
    public readonly Tuple<MethodInfo, Type[]> Method;
    public readonly SerializableExpression Operand;

    public SerializableUnaryExpression(UnaryExpression expression, SerializableExpressionConverter converter)
      : base(expression)
    {
      Contract.Requires(expression != null);
      Contract.Requires(converter != null);

      Method = SerializableExpressionConverter.Convert(expression.Method);
      Operand = converter.TryConvert(expression.Operand);
    }

    internal override void Accept(SerializableExpressionVisitor visitor)
      => visitor.VisitUnary(this);

    internal override Expression ConvertBack()
      => Expression.MakeUnary(
          NodeType,
          Operand.TryConvertBack(),
          Type,
          SerializableExpressionConverter.Convert(Method));
  }
}
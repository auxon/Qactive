using System;
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
      Method = SerializableExpressionConverter.Convert(expression.Method);
      Operand = converter.Convert(expression.Operand);
    }

    internal override Expression Convert() => Expression.MakeUnary(
                                                NodeType,
                                                Operand.TryConvert(),
                                                Type,
                                                SerializableExpressionConverter.Convert(Method));
  }
}
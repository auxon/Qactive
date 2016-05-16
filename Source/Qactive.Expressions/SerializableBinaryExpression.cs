using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;

namespace Qactive.Expressions
{
  [Serializable]
  internal sealed class SerializableBinaryExpression : SerializableExpression
  {
    public readonly SerializableLambdaExpression Conversion;
    public readonly bool IsLiftedToNull;
    public readonly SerializableExpression Left;
    public readonly Tuple<MethodInfo, Type[]> Method;
    public readonly SerializableExpression Right;

    public SerializableBinaryExpression(BinaryExpression expression, SerializableExpressionConverter converter)
      : base(expression)
    {
      Contract.Requires(expression != null);
      Contract.Requires(converter != null);

      Conversion = converter.TryConvert<SerializableLambdaExpression>(expression.Conversion);
      IsLiftedToNull = expression.IsLiftedToNull;
      Left = converter.TryConvert(expression.Left);
      Method = SerializableExpressionConverter.Convert(expression.Method);
      Right = converter.TryConvert(expression.Right);
    }

    internal override Expression Convert() => Expression.MakeBinary(
                                                NodeType,
                                                Left.TryConvert(),
                                                Right.TryConvert(),
                                                IsLiftedToNull,
                                                SerializableExpressionConverter.Convert(Method),
                                                Conversion.TryConvert<LambdaExpression>());
  }
}
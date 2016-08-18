using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;

namespace Qactive.Expressions
{
  [Serializable]
  internal sealed class SerializableMethodCallExpression : SerializableExpression
  {
    public readonly IList<SerializableExpression> Arguments;
    public readonly Tuple<MethodInfo, Type[]> Method;
    public readonly SerializableExpression Object;

    public SerializableMethodCallExpression(MethodCallExpression expression, SerializableExpressionConverter converter)
      : base(expression)
    {
      Contract.Requires(expression != null);
      Contract.Requires(converter != null);

      Arguments = converter.TryConvert(expression.Arguments);
      Method = SerializableExpressionConverter.Convert(expression.Method);
      Object = converter.TryConvert(expression.Object);
    }

    internal override void Accept(SerializableExpressionVisitor visitor)
      => visitor.VisitMethodCall(this);

    internal override Expression ConvertBack()
      => Expression.Call(
          Object.TryConvertBack(),
          SerializableExpressionConverter.Convert(Method),
          Arguments.TryConvert());
  }
}
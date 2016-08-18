using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace Qactive.Expressions
{
  [Serializable]
  internal sealed class SerializableLambdaExpression : SerializableExpression
  {
    public readonly SerializableExpression Body;
    public readonly string Name;
    public readonly IList<SerializableParameterExpression> Parameters;
    public readonly bool TailCall;

    public SerializableLambdaExpression(LambdaExpression expression, SerializableExpressionConverter converter)
      : base(expression)
    {
      Contract.Requires(expression != null);
      Contract.Requires(converter != null);

      Body = converter.TryConvert(expression.Body);
      Name = expression.Name;
      Parameters = converter.TryConvert<SerializableParameterExpression>(expression.Parameters);
      TailCall = expression.TailCall;
    }

    internal override void Accept(SerializableExpressionVisitor visitor)
      => visitor.VisitLambda(this);

    internal override Expression ConvertBack()
      => Expression.Lambda(
          Type,
          Body.TryConvertBack(),
          Name,
          TailCall,
          Parameters.TryConvert<ParameterExpression>());
  }
}
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

    internal override Expression Convert() => Expression.Lambda(
                                                Type,
                                                Body.TryConvert(),
                                                Name,
                                                TailCall,
                                                Parameters.TryConvert<ParameterExpression>());
  }
}
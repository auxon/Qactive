using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Qactive.Expressions
{
  [Serializable]
  internal sealed class SerializableRuntimeVariablesExpression : SerializableExpression
  {
    public readonly IList<SerializableParameterExpression> Variables;

    public SerializableRuntimeVariablesExpression(RuntimeVariablesExpression expression, SerializableExpressionConverter converter)
      : base(expression)
    {
      Variables = converter.Convert<SerializableParameterExpression>(expression.Variables);
    }

    internal override Expression Convert() => Expression.RuntimeVariables(
                                                Variables.TryConvert<ParameterExpression>());
  }
}
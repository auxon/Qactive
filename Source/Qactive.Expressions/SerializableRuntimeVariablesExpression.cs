using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
      Contract.Requires(expression != null);
      Contract.Requires(converter != null);

      Variables = converter.TryConvert<SerializableParameterExpression>(expression.Variables);
    }

    internal override Expression Convert() => Expression.RuntimeVariables(
                                                Variables.TryConvert<ParameterExpression>());
  }
}
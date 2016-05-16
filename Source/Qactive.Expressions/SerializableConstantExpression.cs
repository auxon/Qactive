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

    internal override Expression Convert() => Expression.Constant(Value, Type);
  }
}
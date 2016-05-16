using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace Qactive.Expressions
{
  [Serializable]
  internal sealed class SerializableDefaultExpression : SerializableExpression
  {
    public SerializableDefaultExpression(DefaultExpression expression)
      : base(expression)
    {
      Contract.Requires(expression != null);
    }

    internal override Expression Convert() => Expression.Default(Type);
  }
}
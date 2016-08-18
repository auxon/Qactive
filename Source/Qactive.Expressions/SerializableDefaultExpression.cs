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

    internal override void Accept(SerializableExpressionVisitor visitor)
      => visitor.VisitDefault(this);

    internal override Expression ConvertBack()
      => Expression.Default(
          Type);
  }
}
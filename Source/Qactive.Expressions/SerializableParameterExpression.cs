using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace Qactive.Expressions
{
  [Serializable]
  internal sealed class SerializableParameterExpression : SerializableExpression
  {
    public readonly string Name;

    public bool IsByRef => Type.IsByRef;

    public SerializableParameterExpression(ParameterExpression expression)
      : base(expression)
    {
      Contract.Requires(expression != null);

      Name = expression.Name;
    }

    internal override void Accept(SerializableExpressionVisitor visitor)
      => visitor.VisitParameter(this);

    internal override Expression ConvertBack()
      => Expression.Parameter(
          Type,
          Name);
  }
}
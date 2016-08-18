using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace Qactive.Expressions
{
  [Serializable]
  internal sealed class SerializableGotoExpression : SerializableExpression
  {
    public readonly GotoExpressionKind Kind;
    public readonly string TargetName;
    public readonly Type TargetType;
    public readonly SerializableExpression Value;

    public SerializableGotoExpression(GotoExpression expression, SerializableExpressionConverter converter)
      : base(expression)
    {
      Contract.Requires(expression != null);
      Contract.Requires(converter != null);

      Kind = expression.Kind;
      TargetName = expression.Target.Name;
      TargetType = expression.Target.Type;
      Value = converter.TryConvert(expression.Value);
    }

    internal override void Accept(SerializableExpressionVisitor visitor)
      => visitor.VisitGoto(this);

    internal override Expression ConvertBack()
      => Expression.MakeGoto(
          Kind,
          Expression.Label(TargetType, TargetName),
          Value.TryConvertBack(),
          Type);
  }
}
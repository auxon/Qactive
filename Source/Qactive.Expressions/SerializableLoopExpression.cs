using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace Qactive.Expressions
{
  [Serializable]
  internal sealed class SerializableLoopExpression : SerializableExpression
  {
    public readonly SerializableExpression Body;
    public readonly string BreakLabelName;
    public readonly Type BreakLabelType;
    public readonly string ContinueLabelName;
    public readonly Type ContinueLabelType;

    public SerializableLoopExpression(LoopExpression expression, SerializableExpressionConverter converter)
      : base(expression)
    {
      Contract.Requires(expression != null);
      Contract.Requires(converter != null);

      Body = converter.TryConvert(expression.Body);
      BreakLabelType = expression.BreakLabel.Type;
      BreakLabelName = expression.BreakLabel.Name;
      ContinueLabelType = expression.ContinueLabel.Type;
      ContinueLabelName = expression.ContinueLabel.Name;
    }

    internal override void Accept(SerializableExpressionVisitor visitor)
      => visitor.VisitLoop(this);

    internal override Expression ConvertBack()
      => Expression.Loop(
          Body.TryConvertBack(),
          Expression.Label(BreakLabelType, BreakLabelName),
          Expression.Label(ContinueLabelType, ContinueLabelName));
  }
}
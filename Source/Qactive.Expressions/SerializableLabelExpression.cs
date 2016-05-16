using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace Qactive.Expressions
{
  [Serializable]
  internal sealed class SerializableLabelExpression : SerializableExpression
  {
    public readonly SerializableExpression DefaultValue;
    public readonly string TargetName;
    public readonly Type TargetType;

    public SerializableLabelExpression(LabelExpression expression, SerializableExpressionConverter converter)
      : base(expression)
    {
      Contract.Requires(expression != null);
      Contract.Requires(converter != null);

      DefaultValue = converter.TryConvert(expression.DefaultValue);
      TargetName = expression.Target.Name;
      TargetType = expression.Target.Type;
    }

    internal override Expression Convert() => Expression.Label(
                                                Expression.Label(TargetType, TargetName),
                                                DefaultValue.TryConvert());
  }
}
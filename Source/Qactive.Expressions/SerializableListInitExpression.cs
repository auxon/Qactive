using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Qactive.Expressions
{
  [Serializable]
  internal sealed class SerializableListInitExpression : SerializableExpression
  {
    public readonly IList<Tuple<Tuple<MethodInfo, Type[]>, IList<SerializableExpression>>> Initializers;
    public readonly SerializableNewExpression NewExpression;

    public SerializableListInitExpression(ListInitExpression expression, SerializableExpressionConverter converter)
      : base(expression)
    {
      Contract.Requires(expression != null);
      Contract.Requires(converter != null);

      Initializers = expression.Initializers.Select(i => Tuple.Create(SerializableExpressionConverter.Convert(i.AddMethod), converter.TryConvert(i.Arguments))).ToList();
      NewExpression = converter.TryConvert<SerializableNewExpression>(expression.NewExpression);
    }

    internal override void Accept(SerializableExpressionVisitor visitor)
      => visitor.VisitListInit(this);

    internal override Expression ConvertBack()
      => Expression.ListInit(
          NewExpression.TryConvertBack<NewExpression>(),
          Initializers.Select(i => Expression.ElementInit(SerializableExpressionConverter.Convert(i.Item1), i.Item2.TryConvert())));
  }
}
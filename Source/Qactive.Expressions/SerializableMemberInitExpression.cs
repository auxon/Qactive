using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Qactive.Expressions
{
  [Serializable]
  internal sealed class SerializableMemberInitExpression : SerializableExpression
  {
    public readonly IList<Tuple<Tuple<MemberInfo, Type[]>, MemberBindingType, SerializableExpression, List<Tuple<Tuple<MethodInfo, Type[]>, IList<SerializableExpression>>>, IList<object>>> Bindings;
    public readonly SerializableNewExpression NewExpression;

    public SerializableMemberInitExpression(MemberInitExpression expression, SerializableExpressionConverter converter)
      : base(expression)
    {
      Contract.Requires(expression != null);
      Contract.Requires(converter != null);

      Bindings = expression.Bindings.Select(converter.Convert).ToList();
      NewExpression = converter.TryConvert<SerializableNewExpression>(expression.NewExpression);
    }
    internal override void Accept(SerializableExpressionVisitor visitor)
      => visitor.VisitMemberInit(this);

    internal override Expression ConvertBack()
      => Expression.MemberInit(
          NewExpression.TryConvertBack<NewExpression>(),
          Bindings.Select(SerializableExpressionConverter.Convert));
  }
}
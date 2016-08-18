using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Qactive.Expressions
{
  [Serializable]
  internal sealed class SerializableNewExpression : SerializableExpression
  {
    public readonly IList<SerializableExpression> Arguments;
    public readonly ConstructorInfo Constructor;
    public readonly IList<Tuple<MemberInfo, Type[]>> Members;

    public SerializableNewExpression(NewExpression expression, SerializableExpressionConverter converter)
      : base(expression)
    {
      Contract.Requires(expression != null);
      Contract.Requires(converter != null);

      Arguments = converter.TryConvert(expression.Arguments);
      Constructor = expression.Constructor;
      Members = converter.TryConvert(expression.Members);
    }

    internal override void Accept(SerializableExpressionVisitor visitor)
      => visitor.VisitNew(this);

    internal override Expression ConvertBack()
      => Members.Count == 0
       ? Expression.New(Constructor, Arguments.TryConvert())
       : Expression.New(
          Constructor,
          Arguments.TryConvert(),
          Members.Select(SerializableExpressionConverter.Convert));
  }
}
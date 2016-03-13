using System;
using System.Collections.Generic;
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
      Arguments = converter.Convert(expression.Arguments);
      Constructor = expression.Constructor;
      Members = converter.Convert(expression.Members);
    }

    internal override Expression Convert() => Members.Count == 0
                                            ? Expression.New(Constructor, Arguments.TryConvert())
                                            : Expression.New(
                                                Constructor,
                                                Arguments.TryConvert(),
                                                Members.Select(SerializableExpressionConverter.Convert));
  }
}
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Qactive.Expressions
{
  internal static class SerializableExpressionExtensions
  {
    public static TExpression TryConvert<TExpression>(this SerializableExpression expression)
      where TExpression : Expression => (TExpression)expression.TryConvert();

    public static Expression TryConvert(this SerializableExpression expression) => expression?.ConvertWithCache();

    public static IEnumerable<TExpression> TryConvert<TExpression>(this IEnumerable<SerializableExpression> expressions)
      where TExpression : Expression => expressions.TryConvert().Cast<TExpression>();

    public static IEnumerable<Expression> TryConvert(this IEnumerable<SerializableExpression> expressions) => expressions?.Select(e => e.TryConvert());
  }
}
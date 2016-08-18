using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Qactive.Expressions
{
  internal static class SerializableExpressionExtensions
  {
    public static TExpression TryConvertBack<TExpression>(this SerializableExpression expression)
      where TExpression : Expression
      => (TExpression)expression?.TryConvertBack();

    public static Expression TryConvertBack(this SerializableExpression expression)
      => expression?.ConvertBackWithCache();

    public static IEnumerable<TExpression> TryConvert<TExpression>(this IEnumerable<SerializableExpression> expressions)
      where TExpression : Expression
      => expressions?.TryConvert()?.Cast<TExpression>();

    public static IEnumerable<Expression> TryConvert(this IEnumerable<SerializableExpression> expressions)
      => expressions?.Select(e => e.TryConvertBack());
  }
}
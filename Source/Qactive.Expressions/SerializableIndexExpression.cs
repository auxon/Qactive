using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;

namespace Qactive.Expressions
{
  [Serializable]
  internal sealed class SerializableIndexExpression : SerializableExpression
  {
    public readonly IList<SerializableExpression> Arguments;
    public readonly PropertyInfo Indexer;
    public readonly SerializableExpression Object;

    public SerializableIndexExpression(IndexExpression expression, SerializableExpressionConverter converter)
      : base(expression)
    {
      Contract.Requires(expression != null);
      Contract.Requires(converter != null);

      Arguments = converter.TryConvert(expression.Arguments);
      Indexer = expression.Indexer;
      Object = converter.TryConvert(expression.Object);
    }

    internal override Expression Convert() => Expression.MakeIndex(
                                                Object.TryConvert(),
                                                Indexer,
                                                Arguments.TryConvert());
  }
}
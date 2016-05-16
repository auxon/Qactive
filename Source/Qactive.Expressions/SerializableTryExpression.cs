using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;

namespace Qactive.Expressions
{
  [Serializable]
  internal sealed class SerializableTryExpression : SerializableExpression
  {
    public readonly SerializableExpression Body;
    public readonly SerializableExpression Fault;
    public readonly SerializableExpression Finally;
    public readonly IList<Tuple<SerializableExpression, SerializableExpression, Type, SerializableParameterExpression>> Handlers;

    public SerializableTryExpression(TryExpression expression, SerializableExpressionConverter converter)
      : base(expression)
    {
      Contract.Requires(expression != null);
      Contract.Requires(converter != null);

      Body = converter.TryConvert(expression.Body);
      Fault = converter.TryConvert(expression.Fault);
      Finally = converter.TryConvert(expression.Finally);
      Handlers = expression.Handlers
        .Select(h => Tuple.Create(
          converter.TryConvert(h.Body),
          converter.TryConvert(h.Filter),
          h.Test,
          converter.TryConvert<SerializableParameterExpression>(h.Variable)))
        .ToList();
    }

    internal override Expression Convert() => Expression.MakeTry(
                                                Type,
                                                Body.TryConvert(),
                                                Finally.TryConvert(),
                                                Fault.TryConvert(),
                                                Handlers.Select(h => Expression.MakeCatchBlock(
                                                  h.Item3,
                                                  h.Item4.TryConvert<ParameterExpression>(),
                                                  h.Item1.TryConvert(),
                                                  h.Item2.TryConvert())));
  }
}
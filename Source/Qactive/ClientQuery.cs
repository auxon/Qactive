using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reactive.Linq;

namespace Qactive
{
  internal sealed class ClientQuery<TResult> : QbservableBase<TResult, QactiveProvider>
  {
    private readonly object clientId;

    public ClientQuery(object clientId, QactiveProvider provider)
      : base(provider)
    {
      Contract.Requires(clientId != null);
      Contract.Requires(provider != null);

      this.clientId = clientId;
    }

    public ClientQuery(object clientId, QactiveProvider provider, Expression expression)
      : base(provider, expression)
    {
      Contract.Requires(clientId != null);
      Contract.Requires(provider != null);
      Contract.Requires(expression != null);

      this.clientId = clientId;
    }

    protected override IDisposable SubscribeCore(IObserver<TResult> observer)
      => Provider.Connect<TResult>(PrepareExpression).Subscribe(observer);

    public Expression PrepareExpression(IQbservableProtocol protocol)
    {
      Contract.Requires(protocol != null);
      Contract.Ensures(Contract.Result<Expression>() != null);

      Log.ClientSendingExpression(clientId, Expression);

      if (!Expression.Type.IsGenericType
        || (Expression.Type.GetGenericTypeDefinition() != typeof(IQbservable<>)
          && Expression.Type.GetGenericTypeDefinition() != typeof(ClientQuery<>)))
      {
        throw new InvalidOperationException("The query must end as an IQbservable<T>.");
      }

      var visitor = ReplaceConstantsVisitor.CreateForGenericTypeByDefinition(
        typeof(ClientQuery<>),
        (_, actualType) => Activator.CreateInstance(typeof(QbservableSourcePlaceholder<>).MakeGenericType(actualType.GetGenericArguments()[0]), true),
        type => typeof(IQbservable<>).MakeGenericType(type.GetGenericArguments()[0]));

      var result = visitor.Visit(Expression);

      if (visitor.ReplacedConstants == 0)
      {
        throw new InvalidOperationException("A queryable observable service was not found in the query.");
      }

      var evaluator = Provider.ClientEvaluator;

      if (!evaluator.IsKnownType(Provider.SourceType))
      {
        evaluator.AddKnownType(Provider.SourceType);
      }

      var evaluationVisitor = new LocalEvaluationVisitor(evaluator, protocol);

      var preparedExpression = evaluationVisitor.Visit(result);

      Log.ClientRewrittenExpression(clientId, preparedExpression);

      return preparedExpression;
    }
  }
}
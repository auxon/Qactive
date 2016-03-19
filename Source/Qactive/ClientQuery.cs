using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reactive.Linq;

namespace Qactive
{
  internal sealed class ClientQuery<TResult> : QbservableBase<TResult, ClientQbservableProvider>
  {
    public ClientQuery(ClientQbservableProvider provider)
      : base(provider)
    {
    }

    public ClientQuery(ClientQbservableProvider provider, Expression expression)
      : base(provider, expression)
    {
    }

    protected override IDisposable SubscribeCore(IObserver<TResult> observer)
    {
      return Provider.GetConnections<TResult>(PrepareExpression).Subscribe(observer);
    }

    public Expression PrepareExpression(QbservableProtocol protocol)
    {
      Contract.Requires(protocol != null);
      Contract.Ensures(Contract.Result<Expression>() != null);

      QbservableProviderDiagnostics.DebugPrint(Expression, "TcpClientQuery Original Expression");

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

      var evaluator = Provider.LocalEvaluator;

      if (!evaluator.IsKnownType(Provider.SourceType))
      {
        evaluator.AddKnownType(Provider.SourceType);
      }

      var evaluationVisitor = new LocalEvaluationVisitor(evaluator, protocol);

      var preparedExpression = evaluationVisitor.Visit(result);

      QbservableProviderDiagnostics.DebugPrint(preparedExpression, "TcpClientQuery Rewritten Expression");

      return preparedExpression;
    }
  }
}
using System;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Security;
using System.Security.Permissions;

namespace Qactive
{
  internal sealed class ServerQuery<TSource, TResult> : QbservableBase<TResult, ServerQbservableProvider<TSource>>
  {
    private readonly object argument;

    public ServerQuery(ServerQbservableProvider<TSource> provider, Expression expression, object argument)
      : base(provider, expression)
    {
      this.argument = argument;
    }

    protected override IDisposable SubscribeCore(IObserver<TResult> observer)
    {
      try
      {
        IQbservableProvider realProvider;

        var preparedExpression = PrepareExpression(out realProvider);

        var query = realProvider.CreateQuery<TResult>(preparedExpression);

        ISecureQbservable<TResult> secureQuery = null;

        if (RxSecureQbservable<TResult>.IsRxQuery(query))
        {
          secureQuery = new RxSecureQbservable<TResult>(query);
          query = secureQuery;
        }
        else
        {
          secureQuery = query as ISecureQbservable<TResult>;
        }

        if (secureQuery != null)
        {
          new PermissionSet(PermissionState.Unrestricted).Assert();

          try
          {
            secureQuery.PrepareUnsafe();
          }
          finally
          {
            PermissionSet.RevertAssert();
          }
        }

        return query.Subscribe(observer);
      }
      catch (ExpressionSecurityException)
      {
        throw;
      }
      catch (Exception ex)
      {
        throw new QbservableSubscriptionException(ex);
      }
    }

    private Expression PrepareExpression(out IQbservableProvider realProvider)
    {
      QbservableProviderDiagnostics.DebugPrint(Expression, "ServerQuery Received Expression");

      var source = Provider.GetSource(argument);

      realProvider = source.Provider;

      ExpressionVisitor visitor;
      Expression preparedExpression = null;

      if (!Provider.Options.AllowExpressionsUnrestricted)
      {
        visitor = new SecurityExpressionVisitor(Provider.Options);

        preparedExpression = visitor.Visit(Expression);
      }

      visitor = ReplaceConstantsVisitor.Create(
        typeof(QbservableSourcePlaceholder<TSource>),
        source,
        typeof(IQbservable<TSource>),
        (actualTypeInQuery, actualTypeInServer) =>
        {
          throw new InvalidOperationException("The client specified the wrong data type for the query." + Environment.NewLine
                                            + "Client data type: " + actualTypeInQuery.FullName + Environment.NewLine
                                            + "Actual data type: " + actualTypeInServer.FullName);
        });

      preparedExpression = visitor.Visit(preparedExpression ?? Expression);

      visitor = ReplaceConstantsVisitor.Create(
        typeof(DuplexCallback),
        (value, _) =>
        {
          var callback = (DuplexCallback)value;

          callback.SetServerProtocol(Provider.Protocol);

          return callback;
        },
        type => type);

      preparedExpression = visitor.Visit(preparedExpression);

      QbservableProviderDiagnostics.DebugPrint(preparedExpression, "ServerQuery Rewritten Expression");

      return preparedExpression;
    }
  }
}
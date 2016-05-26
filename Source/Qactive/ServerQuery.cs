using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Security;
#if CAS
using System.Security.Permissions;
#endif

namespace Qactive
{
  internal sealed class ServerQuery<TSource, TResult> : QbservableBase<TResult, ServerQbservableProvider<TSource>>
  {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Currently, clientId is only used for the full FCL build, not portable libraries.")]
    private readonly object clientId;
    private readonly object argument;

    public ServerQuery(object clientId, ServerQbservableProvider<TSource> provider, Expression expression, object argument)
      : base(provider, expression)
    {
      Contract.Requires(clientId != null);
      Contract.Requires(provider != null);
      Contract.Requires(expression != null);

      this.clientId = clientId;
      this.argument = argument;
    }

    [ContractInvariantMethod]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
    private void ObjectInvariant()
    {
      Contract.Invariant(clientId != null);
    }

    protected override IDisposable SubscribeCore(IObserver<TResult> observer)
    {
      try
      {
        IQbservableProvider realProvider;

        var preparedExpression = PrepareExpression(out realProvider);

        var query = realProvider.CreateQuery<TResult>(preparedExpression);

        ISecureQbservable<TResult> secureQuery = null;

#if REFLECTION
        if (RxSecureQbservable<TResult>.IsRxQuery(query))
        {
          secureQuery = new RxSecureQbservable<TResult>(query);
          query = secureQuery;
        }
        else
#endif
        {
          secureQuery = query as ISecureQbservable<TResult>;
        }

        if (secureQuery != null)
        {
#if CAS
          new PermissionSet(PermissionState.Unrestricted).Assert();
#endif

          try
          {
            secureQuery.PrepareUnsafe();
          }
          finally
          {
#if CAS
            PermissionSet.RevertAssert();
#endif
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
      Contract.Ensures(Contract.ValueAtReturn(out realProvider) != null);
      Contract.Ensures(Contract.Result<Expression>() != null);

      Log.ServerReceivingExpression(clientId, Expression);

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

      Log.ServerRewrittenExpression(clientId, preparedExpression);

      return preparedExpression;
    }
  }
}
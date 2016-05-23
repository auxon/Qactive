using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;

namespace Qactive
{
  [ContractClass(typeof(QbservableProtocolContract))]
  public abstract class QbservableProtocol : IQbservableProtocol, IDisposable
  {
    public bool IsClient { get; }

    public object CurrentClientId { get; internal set; }

    public QbservableServiceOptions ServiceOptions { get; }

    public IReadOnlyCollection<ExceptionDispatchInfo> Exceptions => exceptions;

    public QbservableProtocolShutdownReason ShutdownReason { get; internal set; }

    protected CancellationToken Cancel => protocolCancellation.Token;

    private readonly CancellationTokenSource protocolCancellation = new CancellationTokenSource();
    private readonly ConcurrentBag<ExceptionDispatchInfo> exceptions = new ConcurrentBag<ExceptionDispatchInfo>();

    protected QbservableProtocol(CancellationToken cancel)
    {
      Contract.Ensures(IsClient);

      IsClient = true;
      ServiceOptions = QbservableServiceOptions.Default;
      cancel.Register(protocolCancellation.Cancel, useSynchronizationContext: false);
    }

    protected QbservableProtocol(QbservableServiceOptions serviceOptions, CancellationToken cancel)
    {
      Contract.Requires(serviceOptions != null);
      Contract.Ensures(!IsClient);

      ServiceOptions = serviceOptions;
      cancel.Register(protocolCancellation.Cancel, useSynchronizationContext: false);
    }

    [ContractInvariantMethod]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
    private void ObjectInvariant()
    {
      Contract.Invariant(ServiceOptions != null);
      Contract.Invariant(protocolCancellation != null);
      Contract.Invariant(exceptions != null);
    }

    public abstract TSink FindSink<TSink>();

    public abstract TSink GetOrAddSink<TSink>(Func<TSink> createSink);

    IClientDuplexQbservableProtocolSink IQbservableProtocol.CreateClientDuplexSink()
      => CreateClientDuplexSinkInternal();

    IServerDuplexQbservableProtocolSink IQbservableProtocol.CreateServerDuplexSink()
      => CreateServerDuplexSinkInternal();

    internal abstract IClientDuplexQbservableProtocolSink CreateClientDuplexSinkInternal();

    internal abstract IServerDuplexQbservableProtocolSink CreateServerDuplexSinkInternal();

    internal abstract Task InitializeSinksAsync();

    internal abstract Task ServerReceiveAsync();

    protected abstract Task ServerSendAsync(NotificationKind kind, object data);

    protected abstract IObservable<TResult> ClientReceive<TResult>();

    protected abstract Task ClientSendQueryAsync(Expression expression, object argument);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Not worth refactoring into a discrete type just yet.")]
    protected abstract Task<Tuple<Expression, object>> ServerReceiveQueryAsync();

    public IObservable<TResult> ExecuteClient<TResult>(Expression expression, object argument)
    {
      return (from _ in InitializeSinksAsync().ToObservable()
              from __ in ClientSendQueryAsync(expression, argument).ToObservable()
              from result in ClientReceive<TResult>()
              select result)
             .Catch<TResult, OperationCanceledException>(ex => ThrowFor<TResult>(ex))
             .Catch<TResult, Exception>(
                ex =>
                {
                  // Cancellation is required in case client-side code is awaiting socket communication in the background; e.g., via a sink
                  CancelAllCommunication(ExceptionDispatchInfo.Capture(ex));

                  return Observable.Throw<TResult>(ex);
                });
    }

    public async Task ExecuteServerAsync(object clientId, IQbservableProvider provider)
    {
      Task receivingAsync = null;
      ExceptionDispatchInfo fatalException = null;

      try
      {
        await InitializeSinksAsync().ConfigureAwait(false);

        var input = await ServerReceiveQueryAsync().ConfigureAwait(false);

        receivingAsync = ServerReceiveAsync();

        try
        {
          await ExecuteServerQueryAsync(clientId, input, provider).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
          throw;
        }
        catch (Exception ex)
        {
          if (ShutdownReason == QbservableProtocolShutdownReason.None)
          {
            ShutdownReason = QbservableProtocolShutdownReason.BadClientRequest;
          }

          fatalException = ExceptionDispatchInfo.Capture(ex);

          CancelAllCommunication(fatalException);
        }
      }
      catch (OperationCanceledException ex)
      {
        var exception = TryRollupExceptions();

        if (exception != null)
        {
          if (ShutdownReason == QbservableProtocolShutdownReason.None)
          {
            ShutdownReason = QbservableProtocolShutdownReason.ServerError;
          }

          fatalException = exception;
        }

        if (ShutdownReason == QbservableProtocolShutdownReason.None)
        {
          ShutdownReason = QbservableProtocolShutdownReason.ClientTerminated;
        }

        fatalException = ExceptionDispatchInfo.Capture(ex);
      }
      catch (Exception ex)
      {
        fatalException = ExceptionDispatchInfo.Capture(ex);

        CancelAllCommunication(fatalException);
      }

      if (receivingAsync != null)
      {
        try
        {
          await receivingAsync.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
          AddError(ExceptionDispatchInfo.Capture(ex));
        }
      }

      if (fatalException != null)
      {
        fatalException.Throw();
      }
    }

    private async Task ExecuteServerQueryAsync(object clientId, Tuple<Expression, object> input, IQbservableProvider provider)
    {
      Contract.Requires(provider != null);

      var shutdownReason = QbservableProtocolShutdownReason.ObservableTerminated;

      ExceptionDispatchInfo createQueryError = null;

      if (input == null)
      {
        shutdownReason = QbservableProtocolShutdownReason.ProtocolTerminated;
      }
      else
      {
        var expression = input.Item1;
        var argument = input.Item2;

        if (expression == null)
        {
          shutdownReason = QbservableProtocolShutdownReason.ProtocolTerminated;
        }
        else
        {
          Type dataType = null;
          object observable = null;

          try
          {
            CurrentClientId = clientId;

            observable = CreateQuery(provider, expression, argument, out dataType);
          }
          catch (Exception ex)
          {
            shutdownReason = QbservableProtocolShutdownReason.ServerError;
            createQueryError = ExceptionDispatchInfo.Capture(ex);
          }

          if (createQueryError == null)
          {
            await SendObservableAsync(observable, dataType, ServiceOptions.SendServerErrorsToClients, Cancel).ConfigureAwait(false);
          }
          else if (ServiceOptions.SendServerErrorsToClients)
          {
            await ServerSendAsync(NotificationKind.OnError, createQueryError.SourceException).ConfigureAwait(false);
          }
        }
      }

      await ShutdownAsync(shutdownReason).ConfigureAwait(false);

      if (createQueryError != null)
      {
        createQueryError.Throw();
      }
    }

    private async Task SendObservableAsync(object untypedObservable, Type dataType, bool sendServerErrorsToClients, CancellationToken cancel)
    {
      Contract.Requires(untypedObservable != null);
      Contract.Requires(dataType != null);

      var networkErrors = new ConcurrentBag<ExceptionDispatchInfo>();

      ExceptionDispatchInfo expressionSecurityError = null;
      ExceptionDispatchInfo qbservableSubscriptionError = null;
      ExceptionDispatchInfo qbservableError = null;

      var terminationKind = NotificationKind.OnCompleted;

      try
      {
        var cancelSubscription = new CancellationTokenSource();

        cancel.Register(cancelSubscription.Cancel);

        IObservable<object> observable;

        new PermissionSet(PermissionState.Unrestricted).Assert();

        try
        {
          observable = dataType.UpCast(untypedObservable);
        }
        finally
        {
          PermissionSet.RevertAssert();
        }

        await observable.ForEachAsync(
          async data =>
          {
            try
            {
              await ServerSendAsync(NotificationKind.OnNext, data).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
              /* Collecting exceptions handles a possible race condition.  Since this code is using a fire-and-forget model to 
               * subscribe to the observable, due to the async OnNext handler, it's possible that more than one SendAsync task
               * can be executing concurrently.  As a result, cancelling the cancelSubscription below does not guarantee that 
               * this catch block won't run again.
               */
              networkErrors.Add(ExceptionDispatchInfo.Capture(ex));

              cancelSubscription.Cancel();
            }
          },
          cancelSubscription.Token)
          .ConfigureAwait(false);
      }
      catch (OperationCanceledException)
      {
        if (cancel.IsCancellationRequested && networkErrors.Count == 0)
        {
          throw;
        }
      }
      catch (ExpressionSecurityException ex)
      {
        ShutdownReason = QbservableProtocolShutdownReason.ExpressionSecurityViolation;

        expressionSecurityError = ExceptionDispatchInfo.Capture(ex);
      }
      catch (QbservableSubscriptionException ex)
      {
        ShutdownReason = QbservableProtocolShutdownReason.ExpressionSubscriptionException;

        qbservableSubscriptionError = ExceptionDispatchInfo.Capture(ex.InnerException ?? ex);
      }
      catch (TargetInvocationException ex)
      {
        if (ex.InnerException is QbservableSubscriptionException)
        {
          ShutdownReason = QbservableProtocolShutdownReason.ExpressionSubscriptionException;

          qbservableSubscriptionError = ExceptionDispatchInfo.Capture(ex.InnerException.InnerException ?? ex.InnerException);
        }
        else
        {
          qbservableSubscriptionError = ExceptionDispatchInfo.Capture(ex);
        }
      }
      catch (Exception ex)
      {
        terminationKind = NotificationKind.OnError;
        qbservableError = ExceptionDispatchInfo.Capture(ex);
      }

      var error = expressionSecurityError ?? qbservableSubscriptionError;

      if (error != null)
      {
        if (networkErrors.Count > 0)
        {
          // It's not technically a network error, but since the client can't receive it anyway add it so that it's thrown later
          networkErrors.Add(error);
        }
        else
        {
          if (sendServerErrorsToClients || expressionSecurityError != null)
          {
            var exception = expressionSecurityError == null
              ? error.SourceException
              : new SecurityException(error.SourceException.Message);   // Remove stack trace

            await ServerSendAsync(NotificationKind.OnError, exception).ConfigureAwait(false);
          }

          error.Throw();
        }
      }

      /* There's an acceptable race condition here whereby ForEachAsync is canceled by the external cancellation token though
       * it's still executing a fire-and-forget task.  It's possible for the fire-and-forget task to throw before seeing the 
       * cancellation, yet after the following code has already executed.  In that case, since the cancellation was requested 
       * externally, it's acceptable for the cancellation to simply beat the send error, thus the error can safely be ignored.
       */
      if (networkErrors.Count > 0)
      {
        throw new AggregateException(networkErrors.Select(e => e.SourceException));
      }
      else
      {
        try
        {
          await ServerSendAsync(terminationKind, (qbservableError == null ? null : qbservableError.SourceException)).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }

        if (qbservableError != null)
        {
          qbservableError.Throw();
        }
      }
    }

    private static object CreateQuery(IQbservableProvider provider, Expression expression, object argument, out Type type)
    {
      Contract.Requires(provider != null);
      Contract.Requires(expression != null);
      Contract.Requires(expression.Type.IsGenericType);
      Contract.Requires(!expression.Type.IsGenericTypeDefinition);
      Contract.Requires(expression.Type.GetGenericTypeDefinition() == typeof(IQbservable<>));
      Contract.Ensures(Contract.Result<object>() != null);
      Contract.Ensures(Contract.ValueAtReturn(out type) != null);

      type = expression.Type.GetGenericArguments()[0];

      var parameterized = provider as IParameterizedQbservableProvider;

      new PermissionSet(PermissionState.Unrestricted).Assert();

      try
      {
        if (parameterized != null)
        {
          return provider.GetType()
            .GetInterfaceMap(typeof(IParameterizedQbservableProvider))
            .TargetMethods
            .First()
            .MakeGenericMethod(type)
            .Invoke(provider, new[] { expression, argument });
        }
        else
        {
          return provider.GetType()
            .GetInterfaceMap(typeof(IQbservableProvider))
            .TargetMethods
            .First()
            .MakeGenericMethod(type)
            .Invoke(provider, new[] { expression });
        }
      }
      finally
      {
        PermissionSet.RevertAssert();
      }
    }

    protected Task ShutdownAsync(QbservableProtocolShutdownReason reason)
    {
      Contract.Ensures(Contract.Result<Task>() != null);

      ShutdownReason = reason;

      return ShutdownCoreAsync();
    }

    protected void ShutdownWithoutResponse(QbservableProtocolShutdownReason reason)
    {
      ShutdownReason = reason;

      CancelAllCommunication();
    }

    protected abstract Task ShutdownCoreAsync();

    public void CancelAllCommunication()
    {
      try
      {
        protocolCancellation.Cancel();
      }
      catch (AggregateException ex)
      {
        exceptions.Add(ExceptionDispatchInfo.Capture(ex));
      }
      catch (OperationCanceledException)
      {
      }
    }

    public void CancelAllCommunication(ExceptionDispatchInfo exception)
    {
      exceptions.Add(exception);

      if (!protocolCancellation.IsCancellationRequested)
      {
        if (ShutdownReason == QbservableProtocolShutdownReason.None)
        {
          ShutdownReason = QbservableProtocolShutdownReason.ServerError;
        }

        CancelAllCommunication();
      }
    }

    protected void AddError(ExceptionDispatchInfo exception)
    {
      Contract.Requires(exception != null);

      exceptions.Add(exception);
    }

    internal ExceptionDispatchInfo TryRollupExceptions()
    {
      ExceptionDispatchInfo info;
      return exceptions.Count == 1 && exceptions.TryTake(out info)
           ? info
           : exceptions.Count > 1
           ? ExceptionDispatchInfo.Capture(new AggregateException(exceptions.Select(e => e.SourceException)))
           : null;
    }

    internal IObservable<TResult> ThrowFor<TResult>(OperationCanceledException ex)
    {
      Contract.Requires(ex != null);
      Contract.Ensures(Contract.Result<IObservable<TResult>>() != null);

      return Observable.Throw<TResult>(TryRollupExceptions()?.SourceException ?? ex);
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "protocolCancellation",
      Justification = "Async usage causes ObjectDisposedExceptions to occur in unexpected places that should simply respect cancellation and stop silently.")]
    protected virtual void Dispose(bool disposing)
    {
    }
  }

  [ContractClassFor(typeof(QbservableProtocol))]
  internal abstract class QbservableProtocolContract : QbservableProtocol
  {
    public QbservableProtocolContract()
      : base(CancellationToken.None)
    {
    }

    internal override Task InitializeSinksAsync()
    {
      return null;
    }

    internal override Task ServerReceiveAsync()
    {
      return null;
    }

    internal override IClientDuplexQbservableProtocolSink CreateClientDuplexSinkInternal()
    {
      Contract.Ensures(Contract.Result<IClientDuplexQbservableProtocolSink>() != null);
      return null;
    }

    internal override IServerDuplexQbservableProtocolSink CreateServerDuplexSinkInternal()
    {
      Contract.Ensures(Contract.Result<IServerDuplexQbservableProtocolSink>() != null);
      return null;
    }

    protected override Task ServerSendAsync(NotificationKind kind, object data)
    {
      return null;
    }

    protected override IObservable<TResult> ClientReceive<TResult>()
    {
      Contract.Ensures(Contract.Result<IObservable<TResult>>() != null);
      return null;
    }

    protected override Task ClientSendQueryAsync(Expression expression, object argument)
    {
      Contract.Requires(expression != null);
      return null;
    }

    protected override Task<Tuple<Expression, object>> ServerReceiveQueryAsync()
    {
      return null;
    }

    protected override Task ShutdownCoreAsync()
    {
      return null;
    }
  }
}

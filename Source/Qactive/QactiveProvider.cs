using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reactive.Linq;

namespace Qactive
{
  [ContractClass(typeof(QactiveProviderContract))]
  public abstract class QactiveProvider : MarshalByRefObject, IQactiveProvider
  {
    public Type SourceType { get; }

    public LocalEvaluator ClientEvaluator { get; }

    public object Argument { get; }

    /// <summary>
    /// This is purely for diagnostic purposes only. The value returned may be used to identify clients in logs. It's called immediately before the provider's <c>CreateQuery</c> method is invoked.
    /// </summary>
    protected abstract object Id { get; }

    /// <summary>
    /// Constructs an instance of a server provider.
    /// </summary>
    protected QactiveProvider()
    {
    }

    /// <summary>
    /// Constructs an instance of a client provider.
    /// </summary>
    protected QactiveProvider(Type sourceType, LocalEvaluator clientEvaluator)
    {
      Contract.Requires(sourceType != null);
      Contract.Requires(clientEvaluator != null);

      SourceType = sourceType;
      ClientEvaluator = clientEvaluator;
    }

    /// <summary>
    /// Constructs an instance of a client provider.
    /// </summary>
    protected QactiveProvider(Type sourceType, LocalEvaluator clientEvaluator, object argument)
      : this(sourceType, clientEvaluator)
    {
      Contract.Requires(sourceType != null);
      Contract.Requires(clientEvaluator != null);

      Argument = argument;
    }

    public IQbservable<TResult> CreateQuery<TResult>()
      => new ClientQuery<TResult>(Id, this);

    public IQbservable<TResult> CreateQuery<TResult>(Expression expression)
      => new ClientQuery<TResult>(Id, this, expression);

    public abstract IObservable<TResult> Connect<TResult>(Func<IQbservableProtocol, Expression> prepareExpression);

    public abstract IObservable<ClientTermination> Listen(QbservableServiceOptions options, Func<IQbservableProtocol, IParameterizedQbservableProvider> providerFactory);

    void IQactiveProvider.InitializeSecureServer()
      => InitializeSecureServer();

    protected virtual void InitializeSecureServer()
    {
    }
  }

  [ContractClassFor(typeof(QactiveProvider))]
  internal abstract class QactiveProviderContract : QactiveProvider
  {
    protected override object Id
    {
      get
      {
        Contract.Ensures(Contract.Result<object>() != null);
        return null;
      }
    }

    protected QactiveProviderContract(Type sourceType, LocalEvaluator clientEvaluator)
      : base(sourceType, clientEvaluator)
    {
    }
  }
}
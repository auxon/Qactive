using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reactive.Linq;

namespace Qactive
{
  [ContractClass(typeof(IQactiveProviderContract))]
  public interface IQactiveProvider : IQbservableProvider
  {
    Type SourceType { get; }

    LocalEvaluator ClientEvaluator { get; }

    object Argument { get; }

    IObservable<TResult> Connect<TResult>(Func<IQbservableProtocol, Expression> prepareExpression);

    IObservable<ClientTermination> Listen(QbservableServiceOptions options, Func<IQbservableProtocol, IParameterizedQbservableProvider> providerFactory);

    void InitializeSecureServer();
  }

  [ContractClassFor(typeof(IQactiveProvider))]
  internal abstract class IQactiveProviderContract : IQactiveProvider
  {
    public Type SourceType { get; private set; }

    public LocalEvaluator ClientEvaluator { get; private set; }

    public object Argument { get; }

    public IObservable<TResult> Connect<TResult>(Func<IQbservableProtocol, Expression> prepareExpression)
    {
      Contract.Requires(prepareExpression != null);
      Contract.Ensures(Contract.Result<IObservable<TResult>>() != null);
      return null;
    }

    public IObservable<ClientTermination> Listen(QbservableServiceOptions options, Func<IQbservableProtocol, IParameterizedQbservableProvider> providerFactory)
    {
      Contract.Requires(providerFactory != null);
      Contract.Ensures(Contract.Result<IObservable<ClientTermination>>() != null);
      return null;
    }

    public IQbservable<TResult> CreateQuery<TResult>(Expression expression) => null;

    public void InitializeSecureServer()
    {
    }
  }
}

using System;
using System.Linq.Expressions;
using System.Reactive.Linq;

namespace Qactive
{
  public abstract class QactiveProvider : MarshalByRefObject, IQactiveProvider
  {
    public Type SourceType { get; }

    public LocalEvaluator ClientEvaluator { get; }

    public object Argument { get; }

    protected QactiveProvider()
    {
    }

    protected QactiveProvider(Type sourceType, LocalEvaluator clientEvaluator)
    {
      SourceType = sourceType;
      ClientEvaluator = clientEvaluator;
    }

    protected QactiveProvider(Type sourceType, LocalEvaluator clientEvaluator, object argument)
      : this(sourceType, clientEvaluator)
    {
      Argument = argument;
    }

    public IQbservable<TResult> CreateQuery<TResult>() => new ClientQuery<TResult>(this);

    public IQbservable<TResult> CreateQuery<TResult>(Expression expression) => new ClientQuery<TResult>(this, expression);

    public abstract IObservable<TResult> Connect<TResult>(Func<QbservableProtocol, Expression> prepareExpression);

    public abstract IObservable<ClientTermination> Listen(QbservableServiceOptions options, Func<QbservableProtocol, IParameterizedQbservableProvider> providerFactory);

    void IQactiveProvider.InitializeSecureServer() => InitializeSecureServer();

    protected virtual void InitializeSecureServer() { }
  }
}
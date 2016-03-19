using System;
using System.Linq.Expressions;
using System.Reactive.Linq;

namespace Qactive
{
  internal abstract class ClientQbservableProvider : IQbservableProvider
  {
    public Type SourceType { get; }

    public LocalEvaluator LocalEvaluator { get; }

    public object Argument { get; }

    public ClientQbservableProvider(Type sourceType, LocalEvaluator localEvaluator)
    {
      SourceType = sourceType;
      LocalEvaluator = localEvaluator;
    }

    public ClientQbservableProvider(Type sourceType, LocalEvaluator localEvaluator, object argument)
      : this(sourceType, localEvaluator)
    {
      Argument = argument;
    }

    internal IQbservable<TResult> CreateQuery<TResult>() => new ClientQuery<TResult>(this);

    public IQbservable<TResult> CreateQuery<TResult>(Expression expression) => new ClientQuery<TResult>(this, expression);

    public abstract IObservable<TResult> GetConnections<TResult>(Func<QbservableProtocol, Expression> prepareExpression);
  }
}
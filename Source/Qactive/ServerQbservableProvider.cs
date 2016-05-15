using System;
using System.Linq.Expressions;
using System.Reactive.Linq;

namespace Qactive
{
  internal sealed class ServerQbservableProvider<TSource> : IParameterizedQbservableProvider
  {
    public IQbservableProtocol Protocol { get; }

    public QbservableServiceOptions Options { get; }

    private readonly Func<object, IQbservable<TSource>> sourceSelector;

    public ServerQbservableProvider(
      IQbservableProtocol protocol,
      QbservableServiceOptions options,
      Func<object, IQbservable<TSource>> sourceSelector)
    {
      Protocol = protocol;
      Options = options;
      this.sourceSelector = sourceSelector;
    }

    public IQbservable<TSource> GetSource(object argument) => sourceSelector(argument);

    public IQbservable<TResult> CreateQuery<TResult>(Expression expression) => new ServerQuery<TSource, TResult>(Protocol.CurrentClientId, this, expression, null);

    public IQbservable<TResult> CreateQuery<TResult>(Expression expression, object argument) => new ServerQuery<TSource, TResult>(Protocol.CurrentClientId, this, expression, argument);
  }
}
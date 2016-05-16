using System;
using System.Diagnostics.Contracts;
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
      Contract.Requires(protocol != null);
      Contract.Requires(options != null);
      Contract.Requires(sourceSelector != null);

      Protocol = protocol;
      Options = options;
      this.sourceSelector = sourceSelector;
    }

    [ContractInvariantMethod]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
    private void ObjectInvariant()
    {
      Contract.Invariant(Protocol != null);
      Contract.Invariant(Options != null);
      Contract.Invariant(sourceSelector != null);
    }

    public IQbservable<TSource> GetSource(object argument)
    {
      Contract.Ensures(Contract.Result<IQbservable<TSource>>() != null);

      return sourceSelector(argument);
    }

    public IQbservable<TResult> CreateQuery<TResult>(Expression expression) => new ServerQuery<TSource, TResult>(Protocol.CurrentClientId, this, expression, null);

    public IQbservable<TResult> CreateQuery<TResult>(Expression expression, object argument) => new ServerQuery<TSource, TResult>(Protocol.CurrentClientId, this, expression, argument);
  }
}
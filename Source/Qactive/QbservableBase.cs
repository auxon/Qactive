using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Linq;

namespace Qactive
{
  public abstract class QbservableBase<TData, TProvider> : ObservableBase<TData>, IQbservable<TData>
    where TProvider : IQbservableProvider
  {
    public Type ElementType { get; } = typeof(TData);

    public Expression Expression { get; }

    public TProvider Provider { get; }

    IQbservableProvider IQbservable.Provider => Provider;

    protected QbservableBase(TProvider provider)
    {
      Contract.Requires(provider != null);

      Provider = provider;
      Expression = Expression.Constant(this);
    }

    [SuppressMessage("Microsoft.Contracts", "RequiresAtCall-typeof(IQbservable<TData>).IsAssignableFrom(expression.Type)")]
    protected QbservableBase(TProvider provider, Expression expression)
    {
      Contract.Requires(provider != null);
      Contract.Requires(expression != null);
      Contract.Requires(typeof(IQbservable<TData>).IsAssignableFrom(expression.Type));

      Provider = provider;
      Expression = expression;
    }

    protected bool IsSource(Expression candidate)
      => (candidate as ConstantExpression)?.Value == this;
  }
}
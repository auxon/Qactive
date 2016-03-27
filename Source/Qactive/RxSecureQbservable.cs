using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;

namespace Qactive
{
  // HACK: Rx's private IQbservable implementation (ObservableQuery<TSource>) compiles the expression and subscribes all at once; however, compilation
  // demands full trust. We must assert full trust for the Compile invocation so that it can create a dynamic assembly, but we don't want to assert full 
  // trust around the call to Subscribe, which would potentially allow clients' queries to execute unrestricted. This class separates compilation from 
  // subscription using reflection on Rx's underlying query implementation. If any of the implementation details change, then this class must be recompiled,
  // with one potential exception: If Rx were to adopt the ISecureQbservable<TSource> interface defined in Qactive, then we could remove the definition 
  // from this library, and the use of reflection, and simply rely on a cast instead.
  internal sealed class RxSecureQbservable<TSource> : ISecureQbservable<TSource>
  {
    private static readonly Type rxQuery = typeof(Qbservable).Assembly.GetType("System.Reactive.ObservableQuery`1", throwOnError: true, ignoreCase: false).MakeGenericType(typeof(TSource));
    private static readonly FieldInfo rxQuerySource = rxQuery.GetField("_source", BindingFlags.NonPublic | BindingFlags.Instance);

    private static readonly Type rxRewriter = rxQuery.GetNestedType("ObservableRewriter", BindingFlags.NonPublic).MakeGenericType(typeof(TSource));
    private static readonly MethodInfo rxRewriterVisit = rxRewriter.GetMethod("Visit", new[] { typeof(Expression) });

    private readonly IQbservable<TSource> original;

    public RxSecureQbservable(IQbservable<TSource> original)
    {
      Contract.Requires(original != null);

      this.original = original;
    }

    public static bool IsRxQuery(IQbservable<TSource> query)
    {
      Contract.Requires(query != null);

      return rxQuery.IsAssignableFrom(query.GetType());
    }

    public Type ElementType => original.ElementType;

    public Expression Expression => original.Expression;

    public IQbservableProvider Provider => original.Provider;

    public IDisposable Subscribe(IObserver<TSource> observer) => original.Subscribe(observer);

    public void PrepareUnsafe()
    {
      if (rxQuerySource.GetValue(original) == null)
      {
        var observableRewriter = Activator.CreateInstance(rxRewriter);
        var body = (Expression)rxRewriterVisit.Invoke(observableRewriter, new[] { original.Expression });
        var expression = Expression.Lambda<Func<IObservable<TSource>>>(body, new ParameterExpression[0]);
        var compiled = expression.Compile();

        rxQuerySource.SetValue(original, compiled());
      }
    }

    [ContractInvariantMethod]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
    private void ObjectInvariant()
    {
      Contract.Invariant(original != null);
    }
  }
}

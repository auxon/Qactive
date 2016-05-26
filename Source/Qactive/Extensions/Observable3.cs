using System;
using System.Diagnostics.Contracts;

namespace Qactive
{
#if REMOTING
  internal static class Observable3
  {
    /* Observable.Remotable cannot be used within a sandboxed AppDomain because it requires the RemotingConfiguration permission, 
     * but it doesn't assert it and it can't be asserted by user code because the assertion must not occur around the call to Subscribe;
     * otherwise, clients would be able to perform remoting configuration within queries.  Adding this permission to the granted 
     * permission set for the AppDomain would also mean that clients would be able to perform remoting configuration within queries.
     * 
     * To solve this problem, RemotableWithoutConfiguration avoids this permission.
     */
    public static IObservable<TSource> RemotableWithoutConfiguration<TSource>(this IObservable<TSource> observable)
    {
      Contract.Requires(observable != null);
      Contract.Ensures(Contract.Result<IObservable<TSource>>() != null);

      return new SerializableObservable<TSource>(new RemotableObservable<TSource>(observable));
    }

    private sealed class RemotableObservable<T> : MarshalByRefObject, IObservable<T>
    {
      private readonly IObservable<T> observable;

      public RemotableObservable(IObservable<T> observable)
      {
        Contract.Requires(observable != null);

        this.observable = observable;
      }

      [ContractInvariantMethod]
      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
      private void ObjectInvariant()
      {
        Contract.Invariant(observable != null);
      }

      public override object InitializeLifetimeService() => null;

      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We must catch any exception type otherwise we run the risk of losing the info. See the comment inline.")]
      public IDisposable Subscribe(IObserver<T> observer) => new RemotableSubscription(observable.Subscribe(
        value =>
        {
          try
          {
            observer.OnNext(value);
          }
          catch (Exception ex)
          {
#if TRACING_REF
            Log.Unsafe(ex);
#endif

            // Failure to marshal notifications across the AppDomain boundary due to serialization or permission errors are swallowed by Rx, 
            // and without any first-chance exception being thrown and without the downstream observer receiving any notification, causing it
            // to hang indenfitely without even running any Finally actions. We must at least attempt to push the notification to ensure that 
            // the exception is observed and that the downstream subscription is disposed.
            observer.OnError(ex);
          }
        },
        observer.OnError,
        observer.OnCompleted));

      private sealed class RemotableSubscription : MarshalByRefObject, IDisposable
      {
        private readonly IDisposable disposable;

        public RemotableSubscription(IDisposable disposable)
        {
          Contract.Requires(disposable != null);

          this.disposable = disposable;
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
          Contract.Invariant(disposable != null);
        }

        public override object InitializeLifetimeService() => null;

        public void Dispose() => disposable.Dispose();
      }
    }

    [Serializable]
    private sealed class SerializableObservable<T> : IObservable<T>
    {
      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2235:MarkAllNonSerializableFields", Justification = "MarshalByRefObject")]
      private readonly RemotableObservable<T> observable;

      public SerializableObservable(RemotableObservable<T> observable)
      {
        Contract.Requires(observable != null);

        this.observable = observable;
      }

      [ContractInvariantMethod]
      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
      private void ObjectInvariant()
      {
        Contract.Invariant(observable != null);
      }

      public IDisposable Subscribe(IObserver<T> observer) => observable.Subscribe(new RemotableObserver<T>(observer));
    }

    private sealed class RemotableObserver<T> : MarshalByRefObject, IObserver<T>
    {
      private readonly IObserver<T> observer;

      public RemotableObserver(IObserver<T> observer)
      {
        Contract.Requires(observer != null);

        this.observer = observer;
      }

      [ContractInvariantMethod]
      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
      private void ObjectInvariant()
      {
        Contract.Invariant(observer != null);
      }

      public override object InitializeLifetimeService() => null;

      public void OnNext(T value) => observer.OnNext(value);

      public void OnError(Exception error) => observer.OnError(error);

      public void OnCompleted() => observer.OnCompleted();
    }
  }
#endif
}
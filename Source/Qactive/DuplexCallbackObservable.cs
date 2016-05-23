using System;
using System.Reactive.Disposables;
using System.Runtime.ExceptionServices;

namespace Qactive
{
#if SERIALIZATION
  [Serializable]
#endif
  internal sealed class DuplexCallbackObservable<T> : DuplexCallback, IObservable<T>
  {
    public DuplexCallbackObservable(int id)
      : base(id)
    {
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposables are returned to the caller.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "There is no meaningful way to handle exceptions here other than passing them to a handler, and we cannot let them leave their contexts because they will be missed.")]
    public IDisposable Subscribe(IObserver<T> observer)
    {
      var protocol = Protocol ?? Sink.Protocol;
      var disposables = new CompositeDisposable();

      Action<Action> tryExecute =
        action =>
        {
          try
          {
            action();
          }
          catch (Exception ex)
          {
            protocol.CancelAllCommunication(ExceptionDispatchInfo.Capture(ex));
          }
        };

      try
      {
        disposables.Add(
          Sink.Subscribe(
            Id,
            value => tryExecute(() => observer.OnNext((T)value)),
            ex => tryExecute(() => observer.OnError(ex.SourceException)),
            () => tryExecute(observer.OnCompleted)));

        return disposables;
      }
      catch (Exception ex)
      {
        protocol.CancelAllCommunication(ExceptionDispatchInfo.Capture(ex));

        disposables.Dispose();

        return Disposable.Empty;
      }
    }
  }
}
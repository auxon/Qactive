using System;
using System.Diagnostics.Contracts;
using System.Reactive;
using System.Reactive.Disposables;
using System.Runtime.ExceptionServices;

namespace Qactive
{
#if SERIALIZATION
  [Serializable]
#endif
  internal sealed class DuplexCallbackObservable<T> : DuplexCallback, IObservableDuplexCallback, IObservable<T>
  {
#if SERIALIZATION
    [NonSerialized]
#endif
    private readonly object instance;

    public DuplexCallbackObservable(string name, int id, object clientId, object instance)
      : base(name, id, clientId)
    {
      Contract.Requires(!string.IsNullOrEmpty(name));
      Contract.Requires(clientId != null);
      Contract.Requires(instance != null);

      this.instance = instance;
    }

    /// <summary>
    /// Called client-side.
    /// </summary>
    IDisposable IObservableDuplexCallback.Subscribe(Action<object> onNext, Action<Exception> onError, Action onCompleted)
      => typeof(T).UpCast(instance).SubscribeSafe(Observer.Create(onNext, onError, onCompleted));

    /// <summary>
    /// Called server-side.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposables are returned to the caller.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "There is no meaningful way to handle exceptions here other than passing them to a handler, and we cannot let them leave their contexts because they will be missed.")]
    public IDisposable Subscribe(IObserver<T> observer)
    {
      var protocol = Protocol ?? Sink.Protocol;

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
        return Sink.Subscribe(
          Name,
          Id,
          value => tryExecute(() => observer.OnNext((T)value)),
          ex => tryExecute(() => observer.OnError(ex.SourceException)),
          () => tryExecute(observer.OnCompleted));
      }
      catch (Exception ex)
      {
        protocol.CancelAllCommunication(ExceptionDispatchInfo.Capture(ex));

        return Disposable.Empty;
      }
    }
  }
}
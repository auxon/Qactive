using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Qactive.Tests
{
  internal sealed class DuplexSubject
  {
    private readonly SemaphoreSlim leftEvent = new SemaphoreSlim(0);
    private readonly SemaphoreSlim rightEvent = new SemaphoreSlim(0);
    private readonly Queue<TestMessage> left = new Queue<TestMessage>();
    private readonly Queue<TestMessage> right = new Queue<TestMessage>();

    public DuplexSubject()
    {
      NextLeft = Observable.Create<TestMessage>(observer => Await(left, leftEvent, observer, "<-"));
      NextRight = Observable.Create<TestMessage>(observer => Await(right, rightEvent, observer, "->"));

      Left = Observer.Create<TestMessage>(message =>
      {
        Debug.WriteLine("Sending (<-): " + message);

        left.Enqueue(message);
        leftEvent.Release();
      });

      Right = Observer.Create<TestMessage>(message =>
      {
        Debug.WriteLine("Sending (->): " + message);

        right.Enqueue(message);
        rightEvent.Release();
      });
    }

    public IObserver<TestMessage> Left { get; }

    public IObserver<TestMessage> Right { get; }

    public IObservable<TestMessage> NextLeft { get; }

    public IObservable<TestMessage> NextRight { get; }

    private static IDisposable Await(Queue<TestMessage> messages, SemaphoreSlim waitEvent, IObserver<TestMessage> observer, string side, bool alreadyAcquired = false)
    {
      if (messages.Count > 0)
      {
        if (!alreadyAcquired)
        {
          waitEvent.Wait();
        }

        var message = messages.Dequeue();

        Debug.WriteLine("Receive (" + side + "): " + message);

        observer.OnNext(message);
        observer.OnCompleted();

        return Disposable.Empty;
      }
      else
      {
        var cancel = new CancellationDisposable();

        waitEvent.WaitAsync(cancel.Token).ContinueWith(task => Await(messages, waitEvent, observer, side, alreadyAcquired: true), TaskContinuationOptions.ExecuteSynchronously);

        return cancel;
      }
    }
  }
}

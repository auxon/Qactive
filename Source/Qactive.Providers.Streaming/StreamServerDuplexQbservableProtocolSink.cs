using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace Qactive
{
  internal sealed class StreamServerDuplexQbservableProtocolSink : ServerDuplexQbservableProtocolSink<Stream, StreamMessage>
  {
    private readonly StreamQbservableProtocol protocol;

    protected override QbservableProtocol<Stream, StreamMessage> Protocol => protocol;

    public StreamServerDuplexQbservableProtocolSink(StreamQbservableProtocol protocol)
    {
      Contract.Requires(protocol != null);

      this.protocol = protocol;
    }

    [ContractInvariantMethod]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
    private void ObjectInvariant()
    {
      Contract.Invariant(protocol != null);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1500:VariableNamesShouldNotMatchFieldNames", MessageId = "protocol", Justification = "It's the same reference as the field.")]
    public override Task InitializeAsync(QbservableProtocol<Stream, StreamMessage> protocol, CancellationToken cancel)
    {
      Contract.Assume(this.protocol == protocol);

#if ASYNCAWAIT
      return Task.FromResult(true);
#else
      return TaskEx.FromResult(true);
#endif
    }

    protected override IDuplexProtocolMessage TryParseDuplexMessage(StreamMessage message)
    {
      DuplexStreamMessage duplexMessage;
      return DuplexStreamMessage.TryParse(message, protocol, out duplexMessage)
           ? duplexMessage
           : null;
    }

    public override IDisposable Subscribe(string name, int clientId, Action<object> onNext, Action<ExceptionDispatchInfo> onError, Action onCompleted)
    {
      /*
      In testing, the observer permanently blocked incoming data from the client unless concurrency was introduced.
      The order of events were as follows: 

      1. The server received an OnNext notification from an I/O completion port.
      2. The server pushed the value to the observer passed into DuplexCallbackObservable.Subscribe, without introducing concurrency.
      3. The query provider continued executing the serialized query on the current thread.
      4. The query at this point required a synchronous invocation to a client-side member (i.e., duplex enabled).
      5. The server sent the new invocation to the client and then blocked the current thread waiting for an async response.
      
      Since the current thread was an I/O completion port (received for OnNext), it seems that blocking it prevented any 
      further data from being received, even via the Stream.AsyncRead method. Apparently the only solution is to ensure 
      that observable callbacks occur on pooled threads to prevent I/O completion ports from inadvertantly being blocked.

      June 25, 2016 - D.S.
      Replaced TaskPoolScheduler usage with EventLoopScheduler. Unit testing revealed that pushing values through TaskPoolScheduler
      without using ObserveOn introduced a race condition that could cause values to be received in the wrong order.
      */
      var scheduler = new EventLoopScheduler();

      return base.Subscribe(
        name,
        clientId,
        value => scheduler.Schedule(value, (_, v) => { onNext(v); return Disposable.Empty; }),
        ex => scheduler.Schedule(ex, (_, e) => { onError(e); return Disposable.Empty; }),
        () => scheduler.Schedule(onCompleted));
    }

    protected override StreamMessage CreateSubscribe(DuplexCallbackId clientId)
      => DuplexStreamMessage.CreateSubscribe(clientId, protocol);

    protected override StreamMessage CreateDisposeSubscription(int subscriptionId)
      => DuplexStreamMessage.CreateDisposeSubscription(subscriptionId, protocol);

    protected override StreamMessage CreateInvoke(DuplexCallbackId clientId, object[] arguments)
      => DuplexStreamMessage.CreateInvoke(clientId, arguments, protocol);

    protected override StreamMessage CreateGetEnumerator(DuplexCallbackId enumeratorId)
      => DuplexStreamMessage.CreateGetEnumerator(enumeratorId, protocol);

    protected override StreamMessage CreateMoveNext(DuplexCallbackId enumeratorId)
      => DuplexStreamMessage.CreateMoveNext(enumeratorId, protocol);

    protected override StreamMessage CreateResetEnumerator(DuplexCallbackId enumeratorId)
      => DuplexStreamMessage.CreateResetEnumerator(enumeratorId, protocol);

    protected override StreamMessage CreateDisposeEnumerator(int enumeratorId)
      => DuplexStreamMessage.CreateDisposeEnumerator(enumeratorId, protocol);
  }
}

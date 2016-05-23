using System;
using System.Linq.Expressions;
using System.Reactive.Threading.Tasks;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace Qactive
{
  internal sealed class TestQbservableProtocol : QbservableProtocol<IObservable<TestMessage>, TestMessage>
  {
    private readonly IObserver<TestMessage> other;

    public TestQbservableProtocol(IObservable<TestMessage> source, IObserver<TestMessage> other)
      : base(source, CancellationToken.None)
    {
      this.other = other;
    }

    public TestQbservableProtocol(IObservable<TestMessage> source, IObserver<TestMessage> other, QbservableServiceOptions serviceOptions)
      : base(source, serviceOptions, CancellationToken.None)
    {
      this.other = other;
    }

    protected override ClientDuplexQbservableProtocolSink<IObservable<TestMessage>, TestMessage> CreateClientDuplexSink()
      => new ClientDuplexSink(this);

    protected override ServerDuplexQbservableProtocolSink<IObservable<TestMessage>, TestMessage> CreateServerDuplexSink()
      => new ServerDuplexSink(this);

    protected override TestMessage CreateMessage(QbservableProtocolMessageKind kind)
      => new TestMessage(kind);

    protected override TestMessage CreateMessage(QbservableProtocolMessageKind kind, object data)
      => new TestMessage(kind, data);

    protected override TestMessage CreateSubscribeDuplexMessage(DuplexCallbackId id)
      => new TestDuplexMessage(QbservableProtocolMessageKind.DuplexSubscribe, id);

    protected override T Deserialize<T>(TestMessage message)
      => (T)message.Value;

    protected override object PrepareExpressionForMessage(Expression expression)
      => expression;

    protected override Expression GetExpressionFromMessage(TestMessage message)
      => Deserialize<Expression>(message);

    protected override QbservableProtocolShutdownReason GetShutdownReason(TestMessage message, QbservableProtocolShutdownReason defaultReason)
      => defaultReason;

    protected override Task<TestMessage> ReceiveMessageCoreAsync()
      => Source.ToTask();

    protected override Task SendMessageCoreAsync(TestMessage message)
    {
      other.OnNext(message);

      return Task.CompletedTask;
    }

    protected override Task ShutdownCoreAsync()
      => SendMessageAsync(new TestMessage(QbservableProtocolMessageKind.Shutdown, (byte)ShutdownReason));

    private sealed class ClientDuplexSink : ClientDuplexQbservableProtocolSink<IObservable<TestMessage>, TestMessage>
    {
      protected override QbservableProtocol<IObservable<TestMessage>, TestMessage> Protocol { get; }

      public ClientDuplexSink(TestQbservableProtocol protocol)
      {
        Protocol = protocol;
      }

      public override Task InitializeAsync(QbservableProtocol<IObservable<TestMessage>, TestMessage> protocol, CancellationToken cancel)
        => Task.CompletedTask;

      protected override TestMessage CreateEnumeratorError(DuplexCallbackId id, ExceptionDispatchInfo error)
        => new TestDuplexMessage(QbservableProtocolMessageKind.DuplexEnumeratorErrorResponse, id, error);

      protected override TestMessage CreateEnumeratorResponse(DuplexCallbackId id, bool result, object current)
        => new TestDuplexMessage(QbservableProtocolMessageKind.DuplexEnumeratorResponse, id, Tuple.Create(result, current));

      protected override TestMessage CreateErrorResponse(DuplexCallbackId id, ExceptionDispatchInfo error)
        => new TestDuplexMessage(QbservableProtocolMessageKind.DuplexErrorResponse, id, error);

      protected override TestMessage CreateGetEnumeratorError(DuplexCallbackId id, ExceptionDispatchInfo error)
        => new TestDuplexMessage(QbservableProtocolMessageKind.DuplexGetEnumeratorErrorResponse, id, error);

      protected override TestMessage CreateGetEnumeratorResponse(DuplexCallbackId id, int clientEnumeratorId)
        => new TestDuplexMessage(QbservableProtocolMessageKind.DuplexGetEnumeratorResponse, id, clientEnumeratorId);

      protected override TestMessage CreateOnCompleted(DuplexCallbackId id)
        => new TestDuplexMessage(QbservableProtocolMessageKind.DuplexOnCompleted, id);

      protected override TestMessage CreateOnError(DuplexCallbackId id, ExceptionDispatchInfo error)
        => new TestDuplexMessage(QbservableProtocolMessageKind.DuplexOnError, id, error);

      protected override TestMessage CreateOnNext(DuplexCallbackId id, object value)
        => new TestDuplexMessage(QbservableProtocolMessageKind.DuplexOnNext, id, value);

      protected override TestMessage CreateResponse(DuplexCallbackId id, object result)
        => new TestDuplexMessage(QbservableProtocolMessageKind.DuplexResponse, id, result);

      protected override TestMessage CreateSubscribeResponse(DuplexCallbackId id, int clientSubscriptionId)
        => new TestDuplexMessage(QbservableProtocolMessageKind.DuplexSubscribeResponse, id, clientSubscriptionId);

      protected override IDuplexProtocolMessage TryParseDuplexMessage(TestMessage message)
        => message as TestDuplexMessage;
    }

    private sealed class ServerDuplexSink : ServerDuplexQbservableProtocolSink<IObservable<TestMessage>, TestMessage>
    {
      protected override QbservableProtocol<IObservable<TestMessage>, TestMessage> Protocol { get; }

      public ServerDuplexSink(TestQbservableProtocol protocol)
      {
        Protocol = protocol;
      }

      public override Task InitializeAsync(QbservableProtocol<IObservable<TestMessage>, TestMessage> protocol, CancellationToken cancel)
        => Task.CompletedTask;

      protected override TestMessage CreateDisposeEnumerator(int enumeratorId)
        => new TestDuplexMessage(QbservableProtocolMessageKind.DuplexDisposeEnumerator, enumeratorId);

      protected override TestMessage CreateDisposeSubscription(int subscriptionId)
        => new TestDuplexMessage(QbservableProtocolMessageKind.DuplexDisposeSubscription, subscriptionId);

      protected override TestMessage CreateGetEnumerator(DuplexCallbackId enumeratorId)
        => new TestDuplexMessage(QbservableProtocolMessageKind.DuplexGetEnumerator, enumeratorId);

      protected override TestMessage CreateInvoke(DuplexCallbackId clientId, object[] arguments)
        => new TestDuplexMessage(QbservableProtocolMessageKind.DuplexInvoke, clientId, arguments);

      protected override TestMessage CreateMoveNext(DuplexCallbackId enumeratorId)
        => new TestDuplexMessage(QbservableProtocolMessageKind.DuplexMoveNext, enumeratorId);

      protected override TestMessage CreateResetEnumerator(DuplexCallbackId enumeratorId)
        => new TestDuplexMessage(QbservableProtocolMessageKind.DuplexResetEnumerator, enumeratorId);

      protected override IDuplexProtocolMessage TryParseDuplexMessage(TestMessage message)
        => message as TestDuplexMessage;
    }
  }
}

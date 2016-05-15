using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace Qactive
{
  internal sealed class StreamClientDuplexQbservableProtocolSink : ClientDuplexQbservableProtocolSink<Stream, StreamMessage>
  {
    private readonly StreamQbservableProtocol protocol;

    protected override QbservableProtocol<Stream, StreamMessage> Protocol => protocol;

    public StreamClientDuplexQbservableProtocolSink(StreamQbservableProtocol protocol)
    {
      this.protocol = protocol;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1500:VariableNamesShouldNotMatchFieldNames", MessageId = "protocol", Justification = "It's the same reference as the field.")]
    public override Task InitializeAsync(QbservableProtocol<Stream, StreamMessage> protocol, CancellationToken cancel)
    {
      Contract.Assume(this.protocol == protocol);

      return Task.CompletedTask;
    }

    protected override IDuplexProtocolMessage TryParseDuplexMessage(StreamMessage message)
    {
      DuplexStreamMessage duplexMessage;
      return DuplexStreamMessage.TryParse(message, protocol, out duplexMessage)
           ? duplexMessage
           : null;
    }

    protected override StreamMessage CreateResponse(DuplexCallbackId id, object result)
      => DuplexStreamMessage.CreateResponse(id, result, protocol);

    protected override StreamMessage CreateErrorResponse(DuplexCallbackId id, ExceptionDispatchInfo error)
      => DuplexStreamMessage.CreateErrorResponse(id, error, protocol);

    protected override StreamMessage CreateSubscribeResponse(DuplexCallbackId id, int clientSubscriptionId)
      => DuplexStreamMessage.CreateSubscribeResponse(id, clientSubscriptionId, protocol);

    protected override StreamMessage CreateOnNext(DuplexCallbackId id, object value)
      => DuplexStreamMessage.CreateOnNext(id, value, protocol);

    protected override StreamMessage CreateOnError(DuplexCallbackId id, ExceptionDispatchInfo error)
      => DuplexStreamMessage.CreateOnError(id, error, protocol);

    protected override StreamMessage CreateOnCompleted(DuplexCallbackId id)
      => DuplexStreamMessage.CreateOnCompleted(id, protocol);

    protected override StreamMessage CreateGetEnumeratorResponse(DuplexCallbackId id, int clientEnumeratorId)
      => DuplexStreamMessage.CreateGetEnumeratorResponse(id, clientEnumeratorId, protocol);

    protected override StreamMessage CreateGetEnumeratorError(DuplexCallbackId id, ExceptionDispatchInfo error)
      => DuplexStreamMessage.CreateGetEnumeratorError(id, error, protocol);

    protected override StreamMessage CreateEnumeratorResponse(DuplexCallbackId id, bool result, object current)
      => DuplexStreamMessage.CreateEnumeratorResponse(id, result, current, protocol);

    protected override StreamMessage CreateEnumeratorError(DuplexCallbackId id, ExceptionDispatchInfo error)
      => DuplexStreamMessage.CreateEnumeratorError(id, error, protocol);
  }
}
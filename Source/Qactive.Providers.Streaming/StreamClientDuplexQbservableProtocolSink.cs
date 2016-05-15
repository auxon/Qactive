using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Qactive.Properties;

namespace Qactive
{
  internal sealed class StreamClientDuplexQbservableProtocolSink : ClientDuplexQbservableProtocolSink<Stream, StreamMessage>
  {
    private readonly StreamQbservableProtocol protocol;

    public StreamClientDuplexQbservableProtocolSink(StreamQbservableProtocol protocol)
    {
      this.protocol = protocol;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1500:VariableNamesShouldNotMatchFieldNames", MessageId = "protocol", Justification = "It's the same reference as the field.")]
    public override Task InitializeAsync(QbservableProtocol<Stream, StreamMessage> protocol, CancellationToken cancel)
    {
      Contract.Assume(this.protocol == protocol);

      return Task.FromResult(false);
    }

    public override Task<StreamMessage> SendingAsync(StreamMessage message, CancellationToken cancel)
    {
      return Task.FromResult(message);
    }

    public override Task<StreamMessage> ReceivingAsync(StreamMessage message, CancellationToken cancel)
    {
      DuplexStreamMessage duplexMessage;

      if (DuplexStreamMessage.TryParse(message, protocol, out duplexMessage))
      {
        message = duplexMessage;

        switch (duplexMessage.Kind)
        {
          case QbservableProtocolMessageKind.DuplexInvoke:
            Invoke(duplexMessage.Id, (object[])duplexMessage.Value);
            break;
          case QbservableProtocolMessageKind.DuplexSubscribe:
            Subscribe(duplexMessage.Id);
            break;
          case QbservableProtocolMessageKind.DuplexDisposeSubscription:
            DisposeSubscription(duplexMessage.Id.ClientId);
            break;
          case QbservableProtocolMessageKind.DuplexGetEnumerator:
            GetEnumerator(duplexMessage.Id);
            break;
          case QbservableProtocolMessageKind.DuplexMoveNext:
            MoveNext(duplexMessage.Id);
            break;
          case QbservableProtocolMessageKind.DuplexResetEnumerator:
            ResetEnumerator(duplexMessage.Id);
            break;
          case QbservableProtocolMessageKind.DuplexDisposeEnumerator:
            DisposeEnumerator(duplexMessage.Id.ClientId);
            break;
          default:
            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Errors.ProtocolUnknownMessageKindFormat, duplexMessage.Kind));
        }

        duplexMessage.Handled = true;
      }

      return Task.FromResult(message);
    }

    protected override void SendResponse(DuplexCallbackId id, object result)
    {
      protocol.SendDuplexMessageAsync(DuplexStreamMessage.CreateResponse(id, result, protocol));
    }

    protected override void SendError(DuplexCallbackId id, ExceptionDispatchInfo error)
    {
      protocol.SendDuplexMessageAsync(DuplexStreamMessage.CreateErrorResponse(id, error, protocol));
    }

    protected override void SendSubscribeResponse(DuplexCallbackId id, int clientSubscriptionId)
    {
      protocol.SendDuplexMessageAsync(DuplexStreamMessage.CreateSubscribeResponse(id, clientSubscriptionId, protocol));
    }

    public override void SendOnNext(DuplexCallbackId id, object value)
    {
      protocol.SendDuplexMessageAsync(DuplexStreamMessage.CreateOnNext(id, value, protocol));
    }

    public override void SendOnError(DuplexCallbackId id, ExceptionDispatchInfo error)
    {
      protocol.SendDuplexMessageAsync(DuplexStreamMessage.CreateOnError(id, error, protocol));
    }

    public override void SendOnCompleted(DuplexCallbackId id)
    {
      protocol.SendDuplexMessageAsync(DuplexStreamMessage.CreateOnCompleted(id, protocol));
    }

    protected override void SendGetEnumeratorResponse(DuplexCallbackId id, int clientEnumeratorId)
    {
      protocol.SendDuplexMessageAsync(DuplexStreamMessage.CreateGetEnumeratorResponse(id, clientEnumeratorId, protocol));
    }

    protected override void SendGetEnumeratorError(DuplexCallbackId id, ExceptionDispatchInfo error)
    {
      protocol.SendDuplexMessageAsync(DuplexStreamMessage.CreateGetEnumeratorError(id, error, protocol));
    }

    protected override void SendEnumeratorResponse(DuplexCallbackId id, bool result, object current)
    {
      protocol.SendDuplexMessageAsync(DuplexStreamMessage.CreateEnumeratorResponse(id, result, current, protocol));
    }

    protected override void SendEnumeratorError(DuplexCallbackId id, ExceptionDispatchInfo error)
    {
      protocol.SendDuplexMessageAsync(DuplexStreamMessage.CreateEnumeratorError(id, error, protocol));
    }
  }
}
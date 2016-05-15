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
  internal sealed class StreamServerDuplexQbservableProtocolSink : ServerDuplexQbservableProtocolSink<Stream, StreamMessage>
  {
    private readonly StreamQbservableProtocol protocol;

    public StreamServerDuplexQbservableProtocolSink(StreamQbservableProtocol protocol)
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
          case QbservableProtocolMessageKind.DuplexResponse:
            HandleResponse(duplexMessage.Id, duplexMessage.Value);
            break;
          case QbservableProtocolMessageKind.DuplexErrorResponse:
            HandleErrorResponse(duplexMessage.Id, duplexMessage.Error);
            break;
          case QbservableProtocolMessageKind.DuplexSubscribeResponse:
            HandleSubscribeResponse(duplexMessage.Id, (int)duplexMessage.Value);
            break;
          case QbservableProtocolMessageKind.DuplexGetEnumeratorResponse:
            HandleGetEnumeratorResponse(duplexMessage.Id, (int)duplexMessage.Value);
            break;
          case QbservableProtocolMessageKind.DuplexGetEnumeratorErrorResponse:
            HandleGetEnumeratorErrorResponse(duplexMessage.Id, duplexMessage.Error);
            break;
          case QbservableProtocolMessageKind.DuplexEnumeratorResponse:
            HandleEnumeratorResponse(duplexMessage.Id, (Tuple<bool, object>)duplexMessage.Value);
            break;
          case QbservableProtocolMessageKind.DuplexEnumeratorErrorResponse:
            HandleEnumeratorErrorResponse(duplexMessage.Id, duplexMessage.Error);
            break;
          case QbservableProtocolMessageKind.DuplexOnNext:
            HandleOnNext(duplexMessage.Id, duplexMessage.Value);
            break;
          case QbservableProtocolMessageKind.DuplexOnCompleted:
            HandleOnCompleted(duplexMessage.Id);
            break;
          case QbservableProtocolMessageKind.DuplexOnError:
            HandleOnError(duplexMessage.Id, duplexMessage.Error);
            break;
          default:
            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Errors.ProtocolUnknownMessageKindFormat, duplexMessage.Kind));
        }

        duplexMessage.Handled = true;
      }

      return Task.FromResult(message);
    }

    public override object Invoke(int clientId, object[] arguments)
    {
      return protocol.ServerSendDuplexMessage(clientId, id => DuplexStreamMessage.CreateInvoke(id, arguments, protocol));
    }

    public override IDisposable Subscribe(int clientId, Action<object> onNext, Action<ExceptionDispatchInfo> onError, Action onCompleted)
    {
      return protocol.ServerSendSubscribeDuplexMessage(clientId, onNext, onError, onCompleted);
    }

    public override int GetEnumerator(int clientId)
    {
      return (int)protocol.ServerSendDuplexMessage(clientId, id => DuplexStreamMessage.CreateGetEnumerator(id, protocol));
    }

    public override Tuple<bool, object> MoveNext(int enumeratorId)
    {
      return (Tuple<bool, object>)protocol.ServerSendEnumeratorDuplexMessage(enumeratorId, id => DuplexStreamMessage.CreateMoveNext(id, protocol));
    }

    public override void ResetEnumerator(int enumeratorId)
    {
      protocol.ServerSendEnumeratorDuplexMessage(enumeratorId, id => DuplexStreamMessage.CreateResetEnumerator(id, protocol));
    }

    public override void DisposeEnumerator(int enumeratorId)
    {
      protocol.SendDuplexMessageAsync(DuplexStreamMessage.CreateDisposeEnumerator(enumeratorId, protocol));
    }
  }
}
using System;
using System.Runtime.ExceptionServices;

namespace Qactive
{
  internal sealed class DuplexStreamMessage : StreamMessage, IDuplexProtocolMessage
  {
    public DuplexCallbackId Id { get; }

    public object Value { get; }

    public ExceptionDispatchInfo Error { get; }

    private DuplexStreamMessage(QbservableProtocolMessageKind kind, DuplexCallbackId id, object value, byte[] data)
      : base(kind, data, data.Length)
    {
      Id = id;
      Value = value;
    }

    private DuplexStreamMessage(QbservableProtocolMessageKind kind, DuplexCallbackId id, ExceptionDispatchInfo error, byte[] data)
      : base(kind, data, data.Length)
    {
      Id = id;
      Error = error;
    }

    private DuplexStreamMessage(QbservableProtocolMessageKind kind, DuplexCallbackId id, object value, byte[] data, long length)
      : base(kind, data, length)
    {
      Id = id;
      Value = value;
    }

    private DuplexStreamMessage(QbservableProtocolMessageKind kind, DuplexCallbackId id, ExceptionDispatchInfo error, byte[] data, long length)
      : base(kind, data, length)
    {
      Id = id;
      Error = error;
    }

    public static bool TryParse(StreamMessage message, StreamQbservableProtocol protocol, out DuplexStreamMessage duplexMessage)
    {
      switch (message.Kind)
      {
        case QbservableProtocolMessageKind.DuplexInvoke:
        case QbservableProtocolMessageKind.DuplexResponse:
        case QbservableProtocolMessageKind.DuplexSubscribeResponse:
        case QbservableProtocolMessageKind.DuplexGetEnumeratorResponse:
        case QbservableProtocolMessageKind.DuplexEnumeratorResponse:
        case QbservableProtocolMessageKind.DuplexOnNext:
        // The following cases are handled the same as the above cases to ensure that extra data is read, though it's unexpected.
        case QbservableProtocolMessageKind.DuplexOnCompleted:
        case QbservableProtocolMessageKind.DuplexSubscribe:
        case QbservableProtocolMessageKind.DuplexDisposeSubscription:
        case QbservableProtocolMessageKind.DuplexGetEnumerator:
        case QbservableProtocolMessageKind.DuplexMoveNext:
        case QbservableProtocolMessageKind.DuplexResetEnumerator:
        case QbservableProtocolMessageKind.DuplexDisposeEnumerator:
          duplexMessage = new DuplexStreamMessage(
            message.Kind,
            BitConverter.ToInt64(message.Data, 0),
            protocol.Deserialize<object>(message.Data, offset: DuplexCallbackId.Size),
            message.Data,
            message.Length);
          return true;
        case QbservableProtocolMessageKind.DuplexErrorResponse:
        case QbservableProtocolMessageKind.DuplexGetEnumeratorErrorResponse:
        case QbservableProtocolMessageKind.DuplexEnumeratorErrorResponse:
        case QbservableProtocolMessageKind.DuplexOnError:
          duplexMessage = new DuplexStreamMessage(
            message.Kind,
            BitConverter.ToInt64(message.Data, 0),
            protocol.Deserialize<Exception>(message.Data, offset: DuplexCallbackId.Size),
            message.Data,
            message.Length);
          return true;
        default:
          duplexMessage = null;
          return false;
      }
    }

    public static DuplexStreamMessage CreateInvoke(DuplexCallbackId id, object[] arguments, StreamQbservableProtocol protocol)
    {
      return new DuplexStreamMessage(QbservableProtocolMessageKind.DuplexInvoke, id, arguments, Serialize(id, arguments, protocol));
    }

    public static DuplexStreamMessage CreateSubscribe(DuplexCallbackId id, StreamQbservableProtocol protocol)
    {
      return CreateWithoutValue(QbservableProtocolMessageKind.DuplexSubscribe, id, protocol);
    }

    public static DuplexStreamMessage CreateDisposeSubscription(int subscriptionId, StreamQbservableProtocol protocol)
    {
      return CreateWithoutValue(QbservableProtocolMessageKind.DuplexDisposeSubscription, subscriptionId, protocol);
    }

    public static DuplexStreamMessage CreateGetEnumerator(DuplexCallbackId id, StreamQbservableProtocol protocol)
    {
      return CreateWithoutValue(QbservableProtocolMessageKind.DuplexGetEnumerator, id, protocol);
    }

    public static DuplexStreamMessage CreateGetEnumeratorResponse(DuplexCallbackId id, int clientEnumeratorId, StreamQbservableProtocol protocol)
    {
      return new DuplexStreamMessage(QbservableProtocolMessageKind.DuplexGetEnumeratorResponse, id, clientEnumeratorId, Serialize(id, clientEnumeratorId, protocol));
    }

    public static DuplexStreamMessage CreateGetEnumeratorError(DuplexCallbackId id, ExceptionDispatchInfo error, StreamQbservableProtocol protocol)
    {
      return new DuplexStreamMessage(QbservableProtocolMessageKind.DuplexGetEnumeratorErrorResponse, id, error, Serialize(id, error.SourceException, protocol));
    }

    public static DuplexStreamMessage CreateEnumeratorResponse(DuplexCallbackId id, bool result, object current, StreamQbservableProtocol protocol)
    {
      var value = Tuple.Create(result, current);

      return new DuplexStreamMessage(QbservableProtocolMessageKind.DuplexEnumeratorResponse, id, value, Serialize(id, value, protocol));
    }

    public static DuplexStreamMessage CreateEnumeratorError(DuplexCallbackId id, ExceptionDispatchInfo error, StreamQbservableProtocol protocol)
    {
      return new DuplexStreamMessage(QbservableProtocolMessageKind.DuplexEnumeratorErrorResponse, id, error, Serialize(id, error.SourceException, protocol));
    }

    public static DuplexStreamMessage CreateMoveNext(DuplexCallbackId id, StreamQbservableProtocol protocol)
    {
      return CreateWithoutValue(QbservableProtocolMessageKind.DuplexMoveNext, id, protocol);
    }

    public static DuplexStreamMessage CreateResetEnumerator(DuplexCallbackId id, StreamQbservableProtocol protocol)
    {
      return CreateWithoutValue(QbservableProtocolMessageKind.DuplexResetEnumerator, id, protocol);
    }

    public static DuplexStreamMessage CreateDisposeEnumerator(int clientId, StreamQbservableProtocol protocol)
    {
      return CreateWithoutValue(QbservableProtocolMessageKind.DuplexDisposeEnumerator, clientId, protocol);
    }

    public static DuplexStreamMessage CreateResponse(DuplexCallbackId id, object value, StreamQbservableProtocol protocol)
    {
      return new DuplexStreamMessage(QbservableProtocolMessageKind.DuplexResponse, id, value, Serialize(id, value, protocol));
    }

    public static DuplexStreamMessage CreateErrorResponse(DuplexCallbackId id, ExceptionDispatchInfo error, StreamQbservableProtocol protocol)
    {
      return new DuplexStreamMessage(QbservableProtocolMessageKind.DuplexErrorResponse, id, error, Serialize(id, error.SourceException, protocol));
    }

    public static DuplexStreamMessage CreateSubscribeResponse(DuplexCallbackId id, int clientSubscriptionId, StreamQbservableProtocol protocol)
    {
      return new DuplexStreamMessage(QbservableProtocolMessageKind.DuplexSubscribeResponse, id, clientSubscriptionId, Serialize(id, clientSubscriptionId, protocol));
    }

    public static DuplexStreamMessage CreateOnNext(DuplexCallbackId id, object value, StreamQbservableProtocol protocol)
    {
      return new DuplexStreamMessage(QbservableProtocolMessageKind.DuplexOnNext, id, value, Serialize(id, value, protocol));
    }

    public static DuplexStreamMessage CreateOnCompleted(DuplexCallbackId id, StreamQbservableProtocol protocol)
    {
      return CreateWithoutValue(QbservableProtocolMessageKind.DuplexOnCompleted, id, protocol);
    }

    public static DuplexStreamMessage CreateOnError(DuplexCallbackId id, ExceptionDispatchInfo error, StreamQbservableProtocol protocol)
    {
      return new DuplexStreamMessage(QbservableProtocolMessageKind.DuplexOnError, id, error, Serialize(id, error.SourceException, protocol));
    }

    private static DuplexStreamMessage CreateWithoutValue(QbservableProtocolMessageKind kind, DuplexCallbackId id, StreamQbservableProtocol protocol)
    {
      return new DuplexStreamMessage(kind, id, value: null, data: Serialize(id, null, protocol));
    }

    private static byte[] Serialize(DuplexCallbackId id, object value, StreamQbservableProtocol protocol)
    {
      var idData = BitConverter.GetBytes(id);

      long serializedDataLength;
      var serializedData = protocol.Serialize(value, out serializedDataLength);

      var data = new byte[idData.Length + serializedDataLength];

      Array.Copy(idData, data, idData.Length);
      Array.Copy(serializedData, 0, data, idData.Length, serializedDataLength);

      return data;
    }

    public override string ToString()
    {
      return "{" + Kind + ", " + Id + ", Length = " + Length + "}";
    }
  }
}
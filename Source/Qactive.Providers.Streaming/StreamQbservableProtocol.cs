using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Runtime.Remoting.Messaging;
using System.Security;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;
using Qactive.Expressions;

namespace Qactive
{
  internal sealed class StreamQbservableProtocol : QbservableProtocol<Stream, StreamMessage>, IStreamQbservableProtocol
  {
    private readonly AsyncConsumerQueue sendQ = new AsyncConsumerQueue();
    private readonly AsyncConsumerQueue receiveQ = new AsyncConsumerQueue();
    private readonly IRemotingFormatter formatter;

    public StreamQbservableProtocol(object clientId, Stream stream, IRemotingFormatter formatter, CancellationToken cancel)
    : base(clientId, stream, cancel)
    {
      Contract.Requires(clientId != null);
      Contract.Requires(stream != null);
      Contract.Requires(formatter != null);

      this.formatter = formatter;

      sendQ.UnhandledExceptions.Subscribe(AddError);
      receiveQ.UnhandledExceptions.Subscribe(AddError);
    }

    public StreamQbservableProtocol(Stream stream, IRemotingFormatter formatter, QbservableServiceOptions serviceOptions, CancellationToken cancel)
    : base(stream, serviceOptions, cancel)
    {
      Contract.Requires(stream != null);
      Contract.Requires(formatter != null);
      Contract.Requires(serviceOptions != null);

      this.formatter = formatter;

      sendQ.UnhandledExceptions.Subscribe(AddError);
      receiveQ.UnhandledExceptions.Subscribe(AddError);
    }

    public Task SendAsync(byte[] buffer, int offset, int count)
    {
      return sendQ.EnqueueAsync(async () =>
      {
        try
        {
          await Source.WriteAsync(buffer, offset, count, Cancel).ConfigureAwait(false);
          await Source.FlushAsync(Cancel).ConfigureAwait(false);
        }
        catch (ObjectDisposedException ex)    // Occurred sometimes during testing upon cancellation
        {
          throw new OperationCanceledException(ex.Message, ex);
        }
      });
    }

    public Task ReceiveAsync(byte[] buffer, int offset, int count)
    {
      return receiveQ.EnqueueAsync(async () =>
      {
        try
        {
          int read, totalRead = 0;
          while (count > 0 && (read = await Source.ReadAsync(buffer, offset + totalRead, count, Cancel).ConfigureAwait(false)) > 0)
          {
            count -= read;
            totalRead += read;
          }

          if (count > 0)
          {
            throw new InvalidOperationException("The connection was closed without sending all of the data.");
          }
        }
        catch (ObjectDisposedException ex)    // Occurred sometimes during testing upon cancellation
        {
          throw new OperationCanceledException(ex.Message, ex);
        }
      });
    }

    protected override ClientDuplexQbservableProtocolSink<Stream, StreamMessage> CreateClientDuplexSink()
      => new StreamClientDuplexQbservableProtocolSink(this);

    protected override ServerDuplexQbservableProtocolSink<Stream, StreamMessage> CreateServerDuplexSink()
      => new StreamServerDuplexQbservableProtocolSink(this);

    protected override QbservableProtocolShutdownReason GetShutdownReason(StreamMessage message, QbservableProtocolShutdownReason defaultReason)
      => message.Data.Length > 0
       ? (QbservableProtocolShutdownReason)message.Data[0]
       : defaultReason;

    protected override Task ShutdownCoreAsync()
      => SendMessageAsync(new StreamMessage(QbservableProtocolMessageKind.Shutdown, (byte)ShutdownReason));

    protected override StreamMessage CreateMessage(QbservableProtocolMessageKind kind)
      => new StreamMessage(kind);

    protected override StreamMessage CreateMessage(QbservableProtocolMessageKind kind, object data)
    {
      long length;
      return new StreamMessage(kind, Serialize(data, out length), length);
    }

    protected override Task SendMessageCoreAsync(StreamMessage message)
    {
      var lengthBytes = BitConverter.GetBytes(message.Length);

      var buffer = new byte[1L + lengthBytes.Length + message.Length];

      buffer[0] = (byte)message.Kind;

      Array.Copy(lengthBytes, 0, buffer, 1, lengthBytes.Length);

      if (message.Length > 0)
      {
        Array.Copy(message.Data, 0L, buffer, 1L + lengthBytes.Length, message.Length);
      }

      return SendAsync(buffer, 0, buffer.Length);
    }

    protected override async Task<StreamMessage> ReceiveMessageCoreAsync()
    {
      var buffer = new byte[1024];

      await ReceiveAsync(buffer, 0, 9).ConfigureAwait(false);

      var messageKind = (QbservableProtocolMessageKind)buffer[0];
      var length = BitConverter.ToInt64(buffer, 1);

      if (length > 0)
      {
        using (var stream = new MemoryStream((int)length))
        {
          long remainder = length;

          do
          {
            int count = Math.Min(buffer.Length, remainder > int.MaxValue ? int.MaxValue : (int)remainder);

            await ReceiveAsync(buffer, 0, count).ConfigureAwait(false);

            stream.Write(buffer, 0, count);

            remainder -= count;
          }
          while (remainder > 0);

          return new StreamMessage(messageKind, stream.ToArray());
        }
      }

      return new StreamMessage(messageKind, new byte[0]);
    }

    protected override object PrepareExpressionForMessage(Expression expression)
      => new SerializableExpressionConverter().TryConvert(expression);

    protected override Expression GetExpressionFromMessage(StreamMessage message)
      => SerializableExpressionConverter.TryConvert(Deserialize<SerializableExpression>(message));

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#", Justification = "Reviewed")]
    public byte[] Serialize(object data, out long length)
    {
      Contract.Ensures(Contract.Result<byte[]>() != null);
      Contract.Ensures(Contract.Result<byte[]>().Length >= Contract.ValueAtReturn(out length));

      using (var memory = new MemoryStream())
      {
        if (data == null)
        {
          memory.WriteByte(1);
        }
        else
        {
          memory.WriteByte(0);

          new SecurityPermission(SecurityPermissionFlag.SerializationFormatter).Assert();

          try
          {
            formatter.Serialize(memory, data);
          }
          finally
          {
            CodeAccessPermission.RevertAssert();
          }
        }

        length = memory.Length;

        return memory.GetBuffer();
      }
    }

    protected override T Deserialize<T>(StreamMessage message)
      => Deserialize<T>(message.Data);

    public T Deserialize<T>(byte[] data)
    {
      return Deserialize<T>(data, offset: 0);
    }

    public T Deserialize<T>(byte[] data, int offset)
    {
      Contract.Requires(offset == 0 || data == null || data.Length > offset);

      if (data == null || data.Length == 0)
      {
        if (offset > 0)
        {
          throw new InvalidOperationException();
        }

        return (T)(object)null;
      }

      using (var memory = new MemoryStream(data))
      {
        memory.Position = offset;

        var isNullDataFlag = memory.ReadByte();

        Contract.Assume(isNullDataFlag == 0 || isNullDataFlag == 1);

        if (isNullDataFlag == 1)
        {
          return (T)(object)null;
        }
        else
        {
          new SecurityPermission(SecurityPermissionFlag.SerializationFormatter).Assert();

          try
          {
            return (T)formatter.Deserialize(memory);
          }
          finally
          {
            CodeAccessPermission.RevertAssert();
          }
        }
      }
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        sendQ.Dispose();
        receiveQ.Dispose();
      }

      base.Dispose(disposing);
    }
  }
}
using System.Diagnostics.Contracts;

namespace Qactive
{
  internal class StreamMessage : IProtocolMessage
  {
    public QbservableProtocolMessageKind Kind { get; }

    public byte[] Data { get; }

    public long Length { get; }

    public bool Handled { get; set; }

    public StreamMessage(QbservableProtocolMessageKind kind, params byte[] data)
      : this(kind, data, data == null ? 0 : data.Length)
    {
    }

    public StreamMessage(QbservableProtocolMessageKind kind, byte[] data, long length)
    {
      Contract.Requires(length >= 0);
      Contract.Requires(length == 0 || data != null);
      Contract.Requires(length == 0 || data.Length >= length);

      Kind = kind;
      Data = data;
      Length = length;
    }

    public override string ToString()
    {
      return "{" + Kind + ", Length = " + Length + "}";
    }
  }
}
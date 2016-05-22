using System.Runtime.ExceptionServices;

namespace Qactive
{
  internal class TestDuplexMessage : TestMessage, IDuplexProtocolMessage
  {
    public TestDuplexMessage(QbservableProtocolMessageKind kind, DuplexCallbackId id)
      : base(kind)
    {
      Id = id;
    }

    public TestDuplexMessage(QbservableProtocolMessageKind kind, DuplexCallbackId id, object value)
      : base(kind, value)
    {
      Id = id;
    }

    public TestDuplexMessage(QbservableProtocolMessageKind kind, DuplexCallbackId id, ExceptionDispatchInfo error)
      : base(kind)
    {
      Id = id;
      Error = error;
    }

    public DuplexCallbackId Id { get; }

    public ExceptionDispatchInfo Error { get; }

    public override string ToString() => Id.ToString();
  }
}
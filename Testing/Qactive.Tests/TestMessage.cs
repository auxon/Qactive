namespace Qactive
{
  internal class TestMessage : IProtocolMessage
  {
    public TestMessage(QbservableProtocolMessageKind kind)
    {
      Kind = kind;
    }

    public TestMessage(QbservableProtocolMessageKind kind, object value)
      : this(kind)
    {
      Value = value;
    }

    public QbservableProtocolMessageKind Kind { get; }

    public object Value { get; }

    public bool Handled { get; set; }

    public override string ToString()
    {
      return Kind + " (" + Value + "); Handled: " + Handled;
    }
  }
}
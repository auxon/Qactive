namespace Qactive
{
  public interface IProtocolMessage
  {
    QbservableProtocolMessageKind Kind { get; }

    bool Handled { get; set; }
  }
}
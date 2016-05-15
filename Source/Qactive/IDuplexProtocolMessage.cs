using System.Runtime.ExceptionServices;

namespace Qactive
{
  public interface IDuplexProtocolMessage : IProtocolMessage
  {
    DuplexCallbackId Id { get; }
    object Value { get; }
    ExceptionDispatchInfo Error { get; }
  }
}
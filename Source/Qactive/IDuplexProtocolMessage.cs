using System.Runtime.ExceptionServices;

namespace Qactive
{
  public interface IDuplexProtocolMessage : IProtocolMessage
  {
    DuplexCallbackId Id { get; }

    object Value { get; }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Error", Justification = "Standard naming in Rx.")]
    ExceptionDispatchInfo Error { get; }
  }
}
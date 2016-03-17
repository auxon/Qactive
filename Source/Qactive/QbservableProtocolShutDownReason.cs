namespace Qactive
{
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1028:EnumStorageShouldBeInt32", Justification = "Underlying type is directly serialized to a byte stream over the net.")]
  public enum QbservableProtocolShutdownReason : byte
  {
    None,
    ProtocolNegotiationCanceled,
    ProtocolNegotiationError,
    ProtocolTerminated,
    ObservableTerminated,
    ClientTerminated,
    BadClientRequest,
    ExpressionSecurityViolation,
    ExpressionSubscriptionException,
    ServerError
  }
}
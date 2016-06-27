using System;

namespace Qactive
{
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1028:EnumStorageShouldBeInt32", Justification = "Underlying type is directly serialized to a byte stream over the net.")]
  [Flags]
  public enum QbservableProtocolShutdownReason : byte
  {
    None = 0,
    ProtocolNegotiationCanceled = 1,
    ProtocolNegotiationError = 2,
    ObservableTerminated = 4,
    ClientTerminated = 8,
    BadClientRequest = 16,
    ExpressionSecurityViolation = 32,
    ExpressionSubscriptionException = 64,
    ServerError = 128
  }
}

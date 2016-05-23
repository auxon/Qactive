namespace Qactive
{
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1028:EnumStorageShouldBeInt32", Justification = "The value is serialized and using a byte rather than an int saves space and time.")]
  public enum QbservableProtocolMessageKind : byte
  {
    Unknown,
    OnNext,
    OnCompleted,
    OnError,
    Shutdown,
    Argument,
    Subscribe,
    DuplexInvoke,
    DuplexSubscribe,
    DuplexDisposeSubscription,
    DuplexGetEnumerator,
    DuplexGetEnumeratorResponse,
    DuplexGetEnumeratorErrorResponse,
    DuplexMoveNext,
    DuplexResetEnumerator,
    DuplexDisposeEnumerator,
    DuplexEnumeratorResponse,
    DuplexEnumeratorErrorResponse,
    DuplexResponse,
    DuplexErrorResponse,
    DuplexSubscribeResponse,
    DuplexOnNext,
    DuplexOnCompleted,
    DuplexOnError
  }
}
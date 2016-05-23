using System;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;

namespace Qactive
{
  /// <summary>
  /// This special exception type is required to distinguish between exceptions thrown by Subscribe and those thrown by 
  /// the IQbservable source.  It is required because ForEachAsync doesn't accept an OnError handler.  Testing has shown
  /// that it's quite possible for an IQbservable to fail when Subscribe is called due to issues with the Expression tree, 
  /// such as type resolution, missing object instances, buggy serialization, etc.  In these circumstances, the call to
  /// the Subscribe method throws and it's wrapped with this exception, which indicates a failure to subscribe to the query.
  /// </summary>
#if SERIALIZATION
  [Serializable]
#endif
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1064:ExceptionsShouldBePublic", Justification = "For internal use only. It should never be seen (or catchable) externally.")]
  internal sealed class QbservableSubscriptionException : Exception
  {
    public QbservableSubscriptionException(Exception innerException)
      : base("Subscription failed.", innerException)
    {
    }

#if SERIALIZATION
    private QbservableSubscriptionException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      Contract.Requires(info != null);
    }
#endif
  }
}
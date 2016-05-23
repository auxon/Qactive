using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.Serialization;

namespace Qactive
{
  [Serializable]
  public class ClientTermination : ISerializable
  {
    public TimeSpan Duration { get; }

    public QbservableProtocolShutdownReason Reason { get; }

    public ICollection<ExceptionDispatchInfo> Exceptions { get; }

    public ClientTermination(
      TimeSpan duration,
      QbservableProtocolShutdownReason reason,
      IEnumerable<ExceptionDispatchInfo> exceptions)
    {
      Contract.Requires(duration >= TimeSpan.Zero);

      Duration = duration;
      Reason = reason;
      Exceptions = (exceptions ?? Enumerable.Empty<ExceptionDispatchInfo>())
        .Distinct(ExceptionDispatchInfoEqualityComparer.Instance)
        .ToList()
        .AsReadOnly();
    }

    protected ClientTermination(SerializationInfo info, StreamingContext context)
    {
      Contract.Requires(info != null);

      Duration = (TimeSpan)info.GetValue("duration", typeof(TimeSpan));
      Reason = (QbservableProtocolShutdownReason)info.GetValue("reason", typeof(QbservableProtocolShutdownReason));
      Exceptions = ((List<Exception>)info.GetValue("rawExceptions", typeof(List<Exception>)))
        .Select(ExceptionDispatchInfo.Capture)
        .ToList()
        .AsReadOnly();
    }

    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("duration", Duration);
      info.AddValue("reason", Reason);

      // ExceptionDispatchInfo is not serializable.
      info.AddValue("rawExceptions", Exceptions.Select(ex => ex.SourceException).ToList());

      /* The following line is required; otherwise, the rawExceptions list contains only null 
       * references when deserialized.  The count remains correct, but the exceptions are null.
       * Only the first exception needs to be explicitly serialized in order for the entire list
       * to contain non-null references for all exceptions.  I have no idea why this behavior 
       * exists and whether it's a bug in .NET.
       */
      info.AddValue("ignored", Exceptions.Select(ex => ex.SourceException).FirstOrDefault());
    }

    [ContractInvariantMethod]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
    private void ObjectInvariant()
    {
      Contract.Invariant(Duration >= TimeSpan.Zero);
      Contract.Invariant(Exceptions != null);
    }

    public override string ToString()
      => Reason + "; Duration=" + Duration + "; " + Exceptions.Count + " exceptions(s)";
  }
}
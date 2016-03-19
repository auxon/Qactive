using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.ExceptionServices;
using System.Runtime.Serialization;

namespace Qactive
{
  [Serializable]
  public sealed class ClientTermination : ISerializable
  {
    public EndPoint LocalEndPoint { get; }

    public EndPoint RemoteEndPoint { get; }

    public TimeSpan Duration { get; }

    public QbservableProtocolShutdownReason Reason { get; }

    public ICollection<ExceptionDispatchInfo> Exceptions { get; }

    public ClientTermination(
      EndPoint localEndPoint,
      EndPoint remoteEndPoint,
      TimeSpan duration,
      QbservableProtocolShutdownReason reason,
      IEnumerable<ExceptionDispatchInfo> exceptions)
    {
      LocalEndPoint = localEndPoint;
      RemoteEndPoint = remoteEndPoint;
      Duration = duration;
      Reason = reason;
      Exceptions = (exceptions ?? Enumerable.Empty<ExceptionDispatchInfo>())
        .Distinct(ExceptionDispatchInfoEqualityComparer.Instance)
        .ToList()
        .AsReadOnly();
    }

    private ClientTermination(SerializationInfo info, StreamingContext context)
    {
      LocalEndPoint = (EndPoint)info.GetValue("localEndPoint", typeof(EndPoint));
      RemoteEndPoint = (EndPoint)info.GetValue("remoteEndPoint", typeof(EndPoint));
      Duration = (TimeSpan)info.GetValue("duration", typeof(TimeSpan));
      Reason = (QbservableProtocolShutdownReason)info.GetValue("reason", typeof(QbservableProtocolShutdownReason));
      Exceptions = ((List<Exception>)info.GetValue("rawExceptions", typeof(List<Exception>)))
        .Select(ExceptionDispatchInfo.Capture)
        .ToList()
        .AsReadOnly();
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("localEndPoint", LocalEndPoint);
      info.AddValue("remoteEndPoint", RemoteEndPoint);
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
  }
}
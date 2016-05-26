using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Runtime.ExceptionServices;
using System.Runtime.Serialization;

namespace Qactive
{
  [Serializable]
  public sealed class TcpClientTermination : ClientTermination
#if SERIALIZATION && !SERIALIZATION_REF
    , ISerializable
#endif
  {
    public EndPoint LocalEndPoint { get; }

    public EndPoint RemoteEndPoint { get; }

    public TcpClientTermination(
      EndPoint localEndPoint,
      EndPoint remoteEndPoint,
      TimeSpan duration,
      QbservableProtocolShutdownReason reason,
      IEnumerable<ExceptionDispatchInfo> exceptions)
      : base(duration, reason, exceptions)
    {
      Contract.Requires(localEndPoint != null);
      Contract.Requires(remoteEndPoint != null);
      Contract.Requires(duration >= TimeSpan.Zero);

      LocalEndPoint = localEndPoint;
      RemoteEndPoint = remoteEndPoint;
    }

    private TcpClientTermination(SerializationInfo info, StreamingContext context)
#if SERIALIZATION_REF
      : base(info, context)
#else
      : base(
          (TimeSpan)info.GetValue("duration", typeof(TimeSpan)),
          (QbservableProtocolShutdownReason)info.GetValue("reason", typeof(QbservableProtocolShutdownReason)),
          ((List<Exception>)info.GetValue("rawExceptions", typeof(List<Exception>)))
          .Select(ExceptionDispatchInfo.Capture))
#endif
    {
      Contract.Requires(info != null);

      LocalEndPoint = (EndPoint)info.GetValue("localEndPoint", typeof(EndPoint));
      RemoteEndPoint = (EndPoint)info.GetValue("remoteEndPoint", typeof(EndPoint));
    }

#if SERIALIZATION_REF
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("localEndPoint", LocalEndPoint);
      info.AddValue("remoteEndPoint", RemoteEndPoint);

      base.GetObjectData(info, context);
    }
#else
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
#endif

    [ContractInvariantMethod]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
    private void ObjectInvariant()
    {
      Contract.Invariant(LocalEndPoint != null);
      Contract.Invariant(RemoteEndPoint != null);
    }

    public override string ToString()
      => base.ToString() + "; Local: " + LocalEndPoint + "; Remote: " + RemoteEndPoint;
  }
}
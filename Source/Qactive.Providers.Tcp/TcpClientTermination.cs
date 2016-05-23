using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net;
using System.Runtime.ExceptionServices;
using System.Runtime.Serialization;

namespace Qactive
{
  [Serializable]
  public sealed class TcpClientTermination : ClientTermination
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
      : base(info, context)
    {
      Contract.Requires(info != null);

      LocalEndPoint = (EndPoint)info.GetValue("localEndPoint", typeof(EndPoint));
      RemoteEndPoint = (EndPoint)info.GetValue("remoteEndPoint", typeof(EndPoint));
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("localEndPoint", LocalEndPoint);
      info.AddValue("remoteEndPoint", RemoteEndPoint);

      base.GetObjectData(info, context);
    }

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
using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Qactive
{
  internal sealed class WebSocketStream : Stream
  {
    private readonly WebSocket socket;

    public override bool CanRead => true;

    public override bool CanWrite => true;

    public override bool CanTimeout => true;

    public override bool CanSeek => false;

    public override long Length { get { throw new NotSupportedException(); } }

    public override long Position { get { throw new NotSupportedException(); } set { throw new NotSupportedException(); } }

    public WebSocketStream(WebSocket socket)
    {
      Contract.Requires(socket != null);

      this.socket = socket;
    }

    [ContractInvariantMethod]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
    private new void ObjectInvariant()
    {
      Contract.Invariant(socket != null);
    }

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
      => (await socket.ReceiveAsync(new ArraySegment<byte>(buffer, offset, count), cancellationToken).ConfigureAwait(false)).Count;

    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
      => socket.SendAsync(new ArraySegment<byte>(buffer, offset, count), WebSocketMessageType.Binary, false, cancellationToken);

    public override Task FlushAsync(CancellationToken cancellationToken)
#if ASYNCAWAIT
      => Task.FromResult(true);
#else
      => TaskEx.FromResult(true);
#endif

    public override int ReadByte()
    {
      throw new NotSupportedException();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
      throw new NotSupportedException();
    }

    public override void WriteByte(byte value)
    {
      throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
      throw new NotSupportedException();
    }

    public override void Flush()
    {
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
      throw new NotSupportedException();
    }

    public override void SetLength(long value)
    {
      throw new NotSupportedException();
    }
  }
}

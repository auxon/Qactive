using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace Qactive.Tests
{
  internal sealed class LocalTransportStream : Stream
  {
    private readonly object gate = new object();
    private readonly BehaviorSubject<bool> dataAvailable = new BehaviorSubject<bool>(false);
    private readonly Queue<byte> data = new Queue<byte>();

    public LocalTransportStream Other { get; set; }

    public override bool CanRead => true;

    public override bool CanWrite => true;

    public override bool CanSeek => false;

    public override long Length { get { throw new NotSupportedException(); } }

    public override long Position { get { throw new NotSupportedException(); } set { throw new NotSupportedException(); } }

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
      await dataAvailable.Where(b => b).Take(1);

      return Read(buffer, offset, count);
    }

    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
      Write(buffer, offset, count);

      return Task.CompletedTask;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
      lock (gate)
      {
        for (var i = offset; i < offset + count; i++)
        {
          if (data.Count > 0)
          {
            buffer[i] = data.Dequeue();
          }
          else
          {
            dataAvailable.OnNext(false);

            return i - offset;
          }
        }

        if (data.Count == 0)
        {
          dataAvailable.OnNext(false);
        }

        return count;
      }
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
      lock (Other.gate)
      {
        for (var i = offset; i < offset + count; i++)
        {
          Other.data.Enqueue(buffer[i]);
        }

        Other.dataAvailable.OnNext(true);
      }
    }

    public override void Flush() { }

    public override long Seek(long offset, SeekOrigin origin) { throw new NotSupportedException(); }

    public override void SetLength(long value) { throw new NotSupportedException(); }

    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        dataAvailable.Dispose();
      }

      base.Dispose(disposing);
    }
  }
}

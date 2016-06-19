using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace Qactive.Tests
{
  internal sealed class TransportStream : Stream
  {
    private readonly object gate = new object();
    private readonly BehaviorSubject<bool> dataAvailable = new BehaviorSubject<bool>(false);
    private readonly Queue<byte> data = new Queue<byte>();

    public TransportStream Other { get; set; }

    public override bool CanRead => true;

    public override bool CanWrite => true;

    public override bool CanSeek => false;

    public override long Length { get { throw new NotSupportedException(); } }

    public override long Position { get { throw new NotSupportedException(); } set { throw new NotSupportedException(); } }

#if ASYNCAWAIT
    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
#else
    public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
      => ReadAsync(buffer, offset, count, CancellationToken.None).ContinueWith(task => callback(new Task<int>(_ => task.Result, state)), TaskContinuationOptions.ExecuteSynchronously);

    public override int EndRead(IAsyncResult asyncResult)
    {
      using (var task = (Task<int>)asyncResult)
      {
        return task.Result;
      }
    }

    private async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
#endif
    {
      await dataAvailable.Where(b => b).Take(1);

      return Read(buffer, offset, count);
    }

#if ASYNCAWAIT
    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
#else
    public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
      => WriteAsync(buffer, offset, count, CancellationToken.None).ContinueWith(task => callback(new Task(_ => { }, state)), TaskContinuationOptions.ExecuteSynchronously);

    public override void EndWrite(IAsyncResult asyncResult)
      => ((Task)asyncResult).Dispose();

    private Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
#endif
    {
      Write(buffer, offset, count);

#if TPL
      return Task.CompletedTask;
#elif ASYNCAWAIT
      return Task.FromResult(true);
#else
      return TaskEx.FromResult(true);
#endif
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

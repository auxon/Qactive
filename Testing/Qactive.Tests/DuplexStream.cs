using System;
using System.IO;

namespace Qactive.Tests
{
  internal sealed class DuplexStream : IDisposable
  {
    private readonly TransportStream left = new TransportStream();
    private readonly TransportStream right = new TransportStream();

    public DuplexStream()
    {
      left.Other = right;
      right.Other = left;
    }

    public Stream Left => left;

    public Stream Right => right;

    public void Dispose()
    {
      left.Dispose();
      right.Dispose();
    }
  }
}

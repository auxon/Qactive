using System;
using System.IO;

namespace Qactive.Tests
{
  public sealed class LocalTransport : IDisposable
  {
    private readonly LocalTransportStream left = new LocalTransportStream();
    private readonly LocalTransportStream right = new LocalTransportStream();

    public LocalTransport()
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

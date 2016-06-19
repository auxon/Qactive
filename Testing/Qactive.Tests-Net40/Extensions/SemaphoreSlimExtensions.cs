using System.Threading;
using System.Threading.Tasks;

namespace Qactive.Tests
{
  internal static class SemaphoreSlimExtensions
  {
    public static Task WaitAsync(this SemaphoreSlim source, CancellationToken cancel)
      => Task.Factory.StartNew(() => source.Wait(cancel), cancel, TaskCreationOptions.LongRunning, TaskScheduler.Default);
  }
}

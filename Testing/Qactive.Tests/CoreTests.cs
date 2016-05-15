using System.Threading.Tasks;
using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Qactive.Tests
{
  [TestClass]
  public class CoreTests : TestBase
  {
    [TestMethod]
    public async Task Infrastructure()
    {
      var service = new TestService();
      var results = await service.StartAsync(xs => xs, OnNext(10, 0), OnCompleted(20, 0));

      ReactiveAssert.AssertEqual(
        results.Messages,
        OnNext(10, 0),
        OnCompleted(20, 0));
    }
  }
}

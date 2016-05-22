using System.Reactive.Linq;
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
      var service = TestService.Create(OnNext(0), OnCompleted<int>());
      var results = await service.QueryAsync(xs => xs);

      results.AssertEqual(service.Notifications);
    }

    [TestMethod]
    public async Task Where()
    {
      var service = TestService.Create(OnNext(0), OnNext(1), OnNext(2), OnNext(3), OnNext(4), OnCompleted<int>());
      var results = await service.QueryAsync(xs => from x in xs
                                                   where x % 2 == 0
                                                   select x);

      results.AssertEqual(OnNext(0), OnNext(2), OnNext(4), OnCompleted<int>());
    }

    [TestMethod]
    public async Task Select()
    {
      var service = TestService.Create(OnNext(0), OnNext(1), OnNext(2), OnNext(3), OnNext(4), OnCompleted<int>());
      var results = await service.QueryAsync(xs => from x in xs
                                                   select x * 5);

      results.AssertEqual(OnNext(0), OnNext(5), OnNext(10), OnNext(15), OnNext(20), OnCompleted<int>());
    }
  }
}

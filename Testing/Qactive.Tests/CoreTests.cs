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
      var service = TestService.Create(Observable.Return(123));
      var results = await service.QueryAsync(xs => xs);

      results.AssertEqual(OnNext(123), OnCompleted<int>());
    }

    [TestMethod]
    public async Task SelectAnonymousType1()
    {
      var service = TestService.Create(TestService.UnrestrictedOptions, Observable.Return(123));
      var results = await service.QueryAsync(xs => from x in xs
                                                   select new { Value = x });

      results.AssertEqual(OnNext(new { Value = 123 }), OnCompleted(new { Value = default(int) }));
    }

    [TestMethod]
    public async Task SelectAnonymousType2()
    {
      var service = TestService.Create(TestService.UnrestrictedOptions, Observable.Return(123));
      var results = await service.QueryAsync(xs => from x in xs
                                                   select new { Value = x, ValueDoubled = x * 2 });

      results.AssertEqual(OnNext(new { Value = 123, ValueDoubled = 246 }), OnCompleted(new { Value = default(int), ValueDoubled = default(int) }));
    }
  }
}

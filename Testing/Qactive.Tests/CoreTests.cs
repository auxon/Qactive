using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
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

      AssertEqual(results, OnNext(123), OnCompleted<int>());
    }

    [TestMethod]
    public async Task SelectAnonymousType1()
    {
      var service = TestService.Create(TestService.UnrestrictedOptions, Observable.Return(123));
      var results = await service.QueryAsync(xs => from x in xs
                                                   select new { Value = x });

      AssertEqual(results, OnNext(new { Value = 123 }), OnCompleted(new { Value = default(int) }));
    }

    [TestMethod]
    public async Task SelectAnonymousType2()
    {
      var service = TestService.Create(TestService.UnrestrictedOptions, Observable.Return(123));
      var results = await service.QueryAsync(xs => from x in xs
                                                   select new { Value = x, ValueDoubled = x * 2 });

      AssertEqual(results, OnNext(new { Value = 123, ValueDoubled = 246 }), OnCompleted(new { Value = default(int), ValueDoubled = default(int) }));
    }

    [TestMethod]
    public async Task AnonymousTypeInSubQueryArray()
    {
      var service = TestService.Create(TestService.DuplexOptions, new[] { typeof(EnumerableEx) }, Observable.Return(123));

      var results = await service.QueryAsync(source =>
        from _ in source
        select new[] { new { Value = "test" } }.First());

      AssertEqual(results,
        OnNext(new { Value = "test" }),
        OnCompleted(new { Value = default(string) }));
    }

    [TestMethod]
    public async Task AnonymousTypeInSubQueryProjection()
    {
      var service = TestService.Create(TestService.DuplexOptions, new[] { typeof(EnumerableEx) }, Observable.Return(new[] { 123 }));

      var results = await service.QueryAsync(source =>
        from values in source
        select (from value in values
                from result in new[] { new { Value = "test" } }
                select result)
                .First());

      AssertEqual(results,
        OnNext(new { Value = "test" }),
        OnCompleted(new { Value = default(string) }));
    }
  }
}

using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Qactive.Tests.Queries
{
  [TestClass]
  public class WhereTests : TestBase
  {
    [TestMethod]
    public async Task Where()
    {
      var service = TestService.Create(Observable.Range(0, 5));
      var results = await service.QueryAsync(xs => from x in xs
                                                   where x % 2 == 0
                                                   select x);

      results.AssertEqual(OnNext(0), OnNext(2), OnNext(4), OnCompleted<int>());
    }

    [TestMethod]
    public async Task WhereClosure()
    {
      Func<int, bool> predicate = x => x % 2 == 0;

      var service = TestService.Create(QbservableServiceOptions.Unrestricted, Observable.Range(0, 5));
      var results = await service.QueryAsync(xs => from x in xs
                                                   where predicate(x)
                                                   select x);

      results.AssertEqual(OnNext(0), OnNext(2), OnNext(4), OnCompleted<int>());
    }
  }
}

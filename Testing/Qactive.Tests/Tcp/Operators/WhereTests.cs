using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Qactive.Tests.Tcp.Operators
{
  [TestClass]
  public class WhereTests : TestBase
  {
    [TestMethod]
    public async Task Where()
    {
      var service = TcpTestService.Create(Observable.Range(0, 5));
      var results = await service.QueryAsync(xs => from x in xs
                                                   where x % 2 == 0
                                                   select x);

      AssertEqual(results, OnNext(0), OnNext(2), OnNext(4), OnCompleted<int>());
    }

    [TestMethod]
    public async Task WhereClosure()
    {
      Func<int, bool> predicate = x => x % 2 == 0;

      var service = TcpTestService.Create(TcpTestService.UnrestrictedOptions, Observable.Range(0, 5));
      var results = await service.QueryAsync(xs => from x in xs
                                                   where predicate(x)
                                                   select x);

      AssertEqual(results, OnNext(0), OnNext(2), OnNext(4), OnCompleted<int>());
    }
  }
}

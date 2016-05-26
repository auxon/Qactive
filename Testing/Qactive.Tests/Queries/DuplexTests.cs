using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Qactive.Tests.Queries
{
  [TestClass]
  public class DuplexTests : TestBase
  {
    [TestMethod]
    public async Task ContextualObservable()
    {
      var service = TestService.Create(TestService.DuplexOptions, Observable.Return(new TestContext()));

      var closure = Observable.Range(1, 5);

      var results = await service.QueryAsync(source => from context in source
                                                       from contextValue in context.Singleton
                                                       from remote in closure
                                                       where remote % 2 == 0
                                                       select contextValue + remote);

      results.AssertEqual(OnNext(102), OnNext(104), OnCompleted<int>());
    }

    private sealed class TestContext
    {
      public IObservable<int> Singleton { get; } = Observable.Return(100);
    }
  }
}

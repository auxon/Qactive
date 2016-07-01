using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Qactive.Tests.Queries
{
  // TODO: Figure out a way to add assertions for the locality of the notifications; e.g., client-side vs. server-side
  [TestClass]
  public class DuplexTests : TestBase
  {
    [TestMethod]
    public async Task DuplexObservable()
    {
      var service = TestService.Create(TestService.UnrestrictedOptions, Observable.Return(new TestContext()));

      var local = Observable.Range(1, 5);

      var results = await service.QueryAsync(source => from context in source
                                                       from clientValue in local
                                                       where clientValue % 2 == 0
                                                       select clientValue);

      QactiveAssert.AreEqual(results, OnNext(2), OnNext(4), OnCompleted<int>());
    }

    [TestMethod]
    public async Task ContextualObservable()
    {
      var service = TestService.Create(TestService.UnrestrictedOptions, Observable.Return(new TestContext()));

      var results = await service.QueryAsync(source => from context in source
                                                       from serverValue in context.Singleton
                                                       select serverValue);

      QactiveAssert.AreEqual(results, OnNext(100), OnCompleted<int>());
    }

    [TestMethod]
    public async Task DuplexEnumerable()
    {
      var service = TestService.Create(TestService.UnrestrictedOptions, Observable.Return(new TestContext()));

      var local = Enumerable.Range(1, 5);

      var results = await service.QueryAsync(source => from context in source
                                                       from clientValue in local
                                                       where clientValue % 2 == 0
                                                       select clientValue);

      QactiveAssert.AreEqual(results, OnNext(2), OnNext(4), OnCompleted<int>());
    }

    [TestMethod]
    public async Task ContextualEnumerable()
    {
      var service = TestService.Create(TestService.UnrestrictedOptions, Observable.Return(new TestContext()));

      var results = await service.QueryAsync(source => from context in source
                                                       from serverValue in context.SingletonEnumerable
                                                       select serverValue);

      QactiveAssert.AreEqual(results, OnNext(1000), OnCompleted<int>());
    }

    [TestMethod]
    public async Task DuplexMethod()
    {
      var service = TestService.Create(TestService.UnrestrictedOptions, Observable.Return(new TestContext()));

      var local = new Func<int>(() => 123);

      var results = await service.QueryAsync(source => from context in source
                                                       select local());

      QactiveAssert.AreEqual(results, OnNext(123), OnCompleted<int>());
    }

    [TestMethod]
    public async Task ContextualMethod()
    {
      var service = TestService.Create(TestService.UnrestrictedOptions, Observable.Return(new TestContext()));

      var results = await service.QueryAsync(source => from context in source
                                                       select context.Value);

      QactiveAssert.AreEqual(results, OnNext(123), OnCompleted<int>());
    }

    [TestMethod]
    public async Task DuplexObservableWithNonSerializablePayload()
    {
      var service = TestService.Create(TestService.UnrestrictedOptions, new[] { typeof(NonSerializableObject) }, Observable.Return(new TestContext()));

      var obj = new NonSerializableObject();
      var local = Observable.Return(obj);

      var results = await service.QueryAsync(source => from context in source
                                                       from value in local
                                                       select value);

      QactiveAssert.AreEqual(results, OnNext(obj), OnCompleted<NonSerializableObject>());
    }

    [TestMethod]
    public async Task DuplexSubjectAsObservable()
    {
      var service = TestService.Create(TestService.UnrestrictedOptions, Observable.Return(new TestContext()));

      var local = new ReplaySubject<int>();

      Enumerable.Range(1, 5).ForEach(local.OnNext);
      local.OnCompleted();

      var results = await service.QueryAsync(source => from context in source
                                                       from clientValue in local
                                                       where clientValue % 2 == 0
                                                       select clientValue);

      QactiveAssert.AreEqual(results, OnNext(2), OnNext(4), OnCompleted<int>());
    }

    private sealed class TestContext
    {
      public IObservable<int> Singleton { get; } = Observable.Return(100);

      public IEnumerable<int> SingletonEnumerable { get; } = new[] { 1000 };

      public int Value => 123;
    }
  }
}

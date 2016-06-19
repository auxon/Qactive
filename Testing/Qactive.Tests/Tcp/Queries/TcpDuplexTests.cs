using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Qactive.Tests.Tcp.Queries
{
  [TestClass]
  public class TcpDuplexTests : TestBase
  {
    [TestMethod]
    public async Task DuplexObservable()
    {
      var service = TcpTestService.Create(TcpTestService.DuplexOptions, Observable.Return(new TestContext()));

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
      var service = TcpTestService.Create(TcpTestService.DuplexOptions, Observable.Return(new TestContext()));

      var results = await service.QueryAsync(source => from context in source
                                                       from serverValue in context.Singleton
                                                       select serverValue);

      QactiveAssert.AreEqual(results, OnNext(100), OnCompleted<int>());
    }

    [TestMethod]
    public async Task DuplexEnumerable()
    {
      var service = TcpTestService.Create(TcpTestService.DuplexOptions, Observable.Return(new TestContext()));

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
      var service = TcpTestService.Create(TcpTestService.DuplexOptions, Observable.Return(new TestContext()));

      var results = await service.QueryAsync(source => from context in source
                                                       from serverValue in context.SingletonEnumerable
                                                       select serverValue);

      QactiveAssert.AreEqual(results, OnNext(1000), OnCompleted<int>());
    }

    [TestMethod]
    public async Task DuplexMethod()
    {
      var service = TcpTestService.Create(TcpTestService.DuplexOptions, Observable.Return(new TestContext()));

      var local = new Func<int>(() => 123);

      var results = await service.QueryAsync(source => from context in source
                                                       select local());

      QactiveAssert.AreEqual(results, OnNext(123), OnCompleted<int>());
    }

    [TestMethod]
    public async Task ContextualMethod()
    {
      var service = TcpTestService.Create(TcpTestService.DuplexOptions, Observable.Return(new TestContext()));

      var results = await service.QueryAsync(source => from context in source
                                                       select context.Value);

      QactiveAssert.AreEqual(results, OnNext(123), OnCompleted<int>());
    }

    [TestMethod]
    public async Task DuplexSubjectAsObservable()
    {
      var service = TcpTestService.Create(TestService.DuplexOptions, Observable.Return(new TestContext()));

      var local = new ReplaySubject<int>();

      Enumerable.Range(1, 5).ForEach(local.OnNext);
      local.OnCompleted();

      var task = service.QueryAsync(source => from context in source
                                              from clientValue in local
                                              where clientValue % 2 == 0
                                              select clientValue);

      var results = await task;

      QactiveAssert.AreEqual(results, OnNext(2), OnNext(4), OnCompleted<int>());
    }

    [TestMethod]
    public async Task DuplexObservableWithNonSerializablePayload()
    {
      var service = TcpTestService.Create(TestService.DuplexOptions, new[] { typeof(NonSerializableObject) }, Observable.Return(new TestContext()));

      var local = Observable.Return(new NonSerializableObject());

      var results = await service.QueryAsync(source => from context in source
                                                       from value in local
                                                       select value);

      QactiveAssert.AreEqual(results, OnError<NonSerializableObject>(new SerializationException(Any.Message)));
    }

    private sealed class TestContext
    {
      public IObservable<int> Singleton { get; } = Observable.Return(100);

      public IEnumerable<int> SingletonEnumerable { get; } = new[] { 1000 };

      public int Value => 123;
    }
  }
}

using System;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Qactive.Tests.WebSockets.Queries
{
  [TestClass]
  public class WebSocketNestedQueryTests : TestBase
  {
    [TestMethod]
    public async Task Primitive()
    {
      var service = WebSocketTestService.Create(WebSocketTestService.UnrestrictedOptions, Observable.Return(new TestContext()));
      var results = await service.QueryAsync(source => from context in source
                                                       from value in context.PrimitiveQuery
                                                       where value == 123
                                                       select value);

      QactiveAssert.AreEqual(results, OnNext(123), OnCompleted<int>());
    }

#if RX3
    [TestMethod]
    public async Task NestedQueriesAreAppliedToTheInnerProvider()
    {
      var service = WebSocketTestService.Create(WebSocketTestService.UnrestrictedOptions, Observable.Return(new TestContext()));
      var results = await service.QueryAsync(source => from context in source
                                                       from value in from value in context.CustomQuery
                                                                     where value == 123
                                                                     select value
                                                       select value);

      QactiveAssert.AreEqual(results, OnNext(999), OnCompleted<int>());
    }
#else
    [TestMethod]
    public async Task NestedQueriesAreAppliedToTheOuterProvider()
    {
      var service = WebSocketTestService.Create(WebSocketTestService.UnrestrictedOptions, Observable.Return(new TestContext()));
      var results = await service.QueryAsync(source => from context in source
                                                       from value in from value in context.CustomQuery
                                                                     where value == 123
                                                                     select value
                                                       select value);

      QactiveAssert.AreEqual(results, OnNext(123), OnCompleted<int>());
    }
#endif

    private sealed class TestContext
    {
      public IQbservable<int> PrimitiveQuery { get; } = Observable.Return(123).AsQbservable();
      public IQbservable<int> CustomQuery { get; } = new CustomQbservable<int>(new CustomQbservableProvider());
    }

    private sealed class CustomQbservable<TResult> : QbservableBase<TResult, CustomQbservableProvider>
    {
      public CustomQbservable(CustomQbservableProvider provider)
        : base(provider)
      {
      }

      public CustomQbservable(CustomQbservableProvider provider, Expression expression)
        : base(provider, expression)
      {
      }

      protected override IDisposable SubscribeCore(IObserver<TResult> observer)
      {
        var c = Expression as ConstantExpression;

        return c != null && c.Type == typeof(CustomQbservable<int>)
             ? Observable.Return(123).Subscribe(value => observer.OnNext((TResult)(object)value), observer.OnError, observer.OnCompleted)
             : Observable.Return(999).Subscribe(value => observer.OnNext((TResult)(object)value), observer.OnError, observer.OnCompleted);
      }
    }

    private sealed class CustomQbservableProvider : IQbservableProvider
    {
      public CustomQbservableProvider(int instance = 0)
      {
        Instance = instance;
      }

      public int Instance { get; }

      public bool HasQuery { get; private set; }

      public IQbservable<TResult> CreateQuery<TResult>(Expression expression)
      {
        var c = expression as ConstantExpression;

        HasQuery = c == null || c.Type.GetGenericTypeDefinition() != typeof(CustomQbservable<>);

        return new CustomQbservable<TResult>(this, expression);
      }
    }
  }
}

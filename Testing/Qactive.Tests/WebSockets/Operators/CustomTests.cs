using System;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Qactive.Tests.WebSockets.Operators
{
  [TestClass]
  public class CustomTests : TestBase
  {
    [TestMethod]
    public async Task Basic()
    {
      var service = WebSocketTestService.Create(WebSocketTestService.UnrestrictedOptions, new[] { typeof(Qbservable2) }, Observable.Range(0, 5));
      var results = await service.QueryAsync(xs => xs.WhereEven(x => x));

      QactiveAssert.AreEqual(results, OnNext(0), OnNext(2), OnNext(4), OnCompleted<int>());
    }

    [TestMethod]
    public async Task Duplex()
    {
      var local = Observable.Range(100, 5);

      var service = WebSocketTestService.Create(WebSocketTestService.UnrestrictedOptions, new[] { typeof(Qbservable2) }, Observable.Range(50, 5));
      var results = await service.QueryAsync(xs => xs.Add(x => x, local));

      QactiveAssert.AreEqual(results, OnNext(150), OnNext(152), OnNext(154), OnNext(156), OnNext(158), OnCompleted<int>());
    }
  }

  [LocalQueryMethodImplementationType(typeof(Observable2))]
  internal static class Qbservable2
  {
    public static IQbservable<T> WhereEven<T>(this IQbservable<T> source, Expression<Func<T, int>> selector)
      => source.Provider.CreateQuery<T>(Expression.Call(((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(T)), source.Expression, selector));

    public static IQbservable<int> Add<T>(this IQbservable<T> source, Expression<Func<T, int>> selector, IObservable<int> other)
      => source.Provider.CreateQuery<int>(Expression.Call(((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(T)), source.Expression, selector, Expression.Constant(other, typeof(IObservable<int>))));
  }

  internal static class Observable2
  {
    public static IObservable<T> WhereEven<T>(this IObservable<T> source, Func<T, int> selector)
      => from value in source
         where selector(value) % 2 == 0
         select value;

    public static IObservable<int> Add<T>(this IObservable<T> source, Func<T, int> selector, IObservable<int> other)
      => source.Zip(other, (x, y) => selector(x) + y);
  }
}

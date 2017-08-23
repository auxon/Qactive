using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Qactive.Tests.WebSockets.ServiceOptions
{
  [TestClass]
  public class VisitorTests : TestBase
  {
    [TestMethod]
    public async Task SimpleVisitor()
    {
      var visitor = new TestExpressionVisitor();
      var service = WebSocketTestService.Create(QbservableServiceOptions.Default.Add(() => visitor), Observable.Range(1, 5));

      var results = await service.QueryAsync(source => from value in source
                                                       where value % 2 == 0
                                                       select value);

      QactiveAssert.AreEqual(results, OnNext(2), OnNext(4), OnCompleted<int>());
      QactiveAssert.AreEqual(visitor.Visited,
        Call<int>(typeof(Qbservable), "Where", Any.ExpressionOfType<IQbservable<int>>(), Any.LambdaExpression<Func<int, bool>>(Any.ParameterExpression<int>())),
        Any.ExpressionOfType<IQbservable<int>>(ExpressionType.Constant),
        Any.QuotedLambdaExpression<Func<int, bool>>(Any.ParameterExpression<int>()),
        Any.LambdaExpression<Func<int, bool>>(Any.ParameterExpression<int>()),
        Any.ExpressionOfType(ExpressionType.Equal),
        Any.ExpressionOfType(ExpressionType.Modulo),
        Any.ParameterExpression<int>(),
        Expression.Constant(2),
        Expression.Constant(0),
        Any.ParameterExpression<int>());
    }
  }
}

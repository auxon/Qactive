using System.Collections.Generic;
using System.Linq.Expressions;

namespace Qactive.Tests
{
  internal sealed class TestExpressionVisitor : ExpressionVisitor
  {
    public IList<Expression> Visited { get; } = new List<Expression>();

    public override Expression Visit(Expression node)
    {
      if (node != null)
      {
        Visited.Add(node);
      }

      return base.Visit(node);
    }
  }
}

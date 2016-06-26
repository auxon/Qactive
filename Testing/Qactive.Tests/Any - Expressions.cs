using System;
using System.Linq.Expressions;

namespace Qactive.Tests
{
  internal static partial class Any
  {
    public static readonly Expression Expression = new AnyExpression();

    public static Expression<TDelegate> LambdaExpression<TDelegate>(params ParameterExpression[] parameters)
      => Expression.Lambda<TDelegate>(ExpressionOfType(typeof(TDelegate).GetMethod("Invoke").ReturnType), Any.Name, parameters);

    public static Expression QuotedLambdaExpression<TDelegate>(params ParameterExpression[] parameters)
      => Expression.Quote(LambdaExpression<TDelegate>(parameters));

    public static ParameterExpression ParameterExpression<T>()
      => Expression.Parameter(typeof(T), Any.Name);

    public static Expression ExpressionOfType(ExpressionType nodeType)
      => new AnyExpression(nodeType);

    public static Expression ExpressionOfType<T>()
      => new AnyExpression(typeof(T));

    public static Expression ExpressionOfType(Type type)
      => new AnyExpression(type);

    public static Expression ExpressionOfType<T>(ExpressionType nodeType)
      => new AnyExpression(nodeType, typeof(T));

    public static Expression ExpressionOfType(ExpressionType nodeType, Type type)
      => new AnyExpression(nodeType, type);

    public static bool IsAny(this Expression expression, ExpressionType? nodeType = null, Type type = null)
      => (expression is AnyExpression || IsAnyLambdaExpression(expression) || IsAnyQuotedLambdaExpression(expression) || IsAnyParameterExpression(expression))
      && (nodeType == null || ((expression as AnyExpression)?.IsAnyNodeType ?? false) || expression.NodeType == nodeType)
      && (type == null || expression.Type == Any.Type || expression.Type == type);

    private static bool IsAnyLambdaExpression(Expression expression)
    {
      var lambda = expression as LambdaExpression;

      return lambda != null && lambda.Name == Any.Name;
    }

    private static bool IsAnyQuotedLambdaExpression(Expression expression)
    {
      var quote = expression as UnaryExpression;

      return quote != null && quote.NodeType == ExpressionType.Quote && IsAnyLambdaExpression(quote.Operand);
    }

    private static bool IsAnyParameterExpression(Expression expression)
    {
      var parameter = expression as ParameterExpression;

      return parameter != null && parameter.Name == Any.Name;
    }

    private sealed class AnyExpression : Expression
    {
      private readonly ExpressionType? nodeType;
      private readonly Type type;

      public bool IsAnyNodeType => !nodeType.HasValue;

      public AnyExpression()
      {
        this.nodeType = null;
        this.type = Any.Type;
      }

      public AnyExpression(ExpressionType nodeType)
      {
        this.nodeType = nodeType;
        this.type = Any.Type;
      }

      public AnyExpression(Type type)
      {
        this.nodeType = null;
        this.type = type;
      }

      public AnyExpression(ExpressionType nodeType, Type type)
      {
        this.nodeType = nodeType;
        this.type = type;
      }

      public override ExpressionType NodeType => nodeType ?? ExpressionType.Constant;

      public override Type Type => type;

      protected override Expression VisitChildren(ExpressionVisitor visitor)
      {
        return base.VisitChildren(visitor);
      }

      protected override Expression Accept(ExpressionVisitor visitor)
      {
        return base.Accept(visitor);
      }

      public override Expression Reduce()
      {
        return base.Reduce();
      }

      public override string ToString()
      {
        return "Any"
             + (IsAnyNodeType ? string.Empty : "(" + nodeType + ")")
             + (type == null ? string.Empty : "{" + type + "}");
      }
    }
  }
}

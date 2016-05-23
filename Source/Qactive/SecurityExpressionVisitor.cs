using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;

namespace Qactive
{
  public class SecurityExpressionVisitor : ExpressionVisitor
  {
    // TODO: Place configurable caps on the use of some kinds of types, operators and parameters (see Security Guidelines.md)

    public ExpressionOptions Options { get; }

    public ServiceEvaluationContext Context { get; }

    public SecurityExpressionVisitor(QbservableServiceOptions serviceOptions)
    {
      Contract.Requires(serviceOptions != null);

      Options = serviceOptions.ExpressionOptions;
      Context = serviceOptions.EvaluationContext;

      if (Options.HasFlag(ExpressionOptions.AllowTypeTests)
        && Options.HasFlag(ExpressionOptions.AllowExplicitConversions))
      {
        Context.EnsureHasKnownOperator("Cast");
        Context.EnsureHasKnownOperator("OfType");
      }

      if (Options.HasFlag(ExpressionOptions.AllowCatchBlocks))
      {
        Context.EnsureHasKnownOperator("Catch");
        Context.EnsureHasKnownOperator("OnErrorResumeNext");
        Context.EnsureHasKnownOperator("Retry");
      }
    }

    [ContractInvariantMethod]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
    private void ObjectInvariant()
    {
      Contract.Invariant(Context != null);
    }

    protected override Expression VisitBinary(BinaryExpression node)
    {
      if (!Options.HasFlag(ExpressionOptions.AllowAssignments))
      {
        switch (node.NodeType)
        {
          case ExpressionType.AddAssign:
          case ExpressionType.AddAssignChecked:
          case ExpressionType.AndAssign:
          case ExpressionType.Assign:
          case ExpressionType.DivideAssign:
          case ExpressionType.ExclusiveOrAssign:
          case ExpressionType.LeftShiftAssign:
          case ExpressionType.ModuloAssign:
          case ExpressionType.MultiplyAssign:
          case ExpressionType.MultiplyAssignChecked:
          case ExpressionType.OrAssign:
          case ExpressionType.PostDecrementAssign:
          case ExpressionType.PostIncrementAssign:
          case ExpressionType.PowerAssign:
          case ExpressionType.PreDecrementAssign:
          case ExpressionType.PreIncrementAssign:
          case ExpressionType.RightShiftAssign:
          case ExpressionType.SubtractAssign:
          case ExpressionType.SubtractAssignChecked:
            throw new ExpressionSecurityException("Assignments are not permitted.");
        }
      }

      return base.VisitBinary(node);
    }

    protected override Expression VisitBlock(BlockExpression node)
    {
      if (!Options.HasFlag(ExpressionOptions.AllowBlocks))
      {
        throw new ExpressionSecurityException("Blocks are not permitted.");
      }

      return base.VisitBlock(node);
    }

    protected override CatchBlock VisitCatchBlock(CatchBlock node)
    {
      if (!Options.HasFlag(ExpressionOptions.AllowCatchBlocks))
      {
        throw new ExpressionSecurityException("Catch blocks are not permitted.");
      }

      return base.VisitCatchBlock(node);
    }

    protected override Expression VisitConstant(ConstantExpression node)
    {
      if (!Context.IsKnownType(node.Type))
      {
        throw new ExpressionSecurityException("Type \"" + node.Type + "\" is not permitted.");
      }

      return base.VisitConstant(node);
    }

    protected override Expression VisitExtension(Expression node)
    {
      if (!Options.HasFlag(ExpressionOptions.AllowExtensions))
      {
        throw new ExpressionSecurityException("Extensions are not permitted.");
      }

      return base.VisitExtension(node);
    }

    protected override Expression VisitGoto(GotoExpression node)
    {
      switch (node.Kind)
      {
        case GotoExpressionKind.Goto:
          if (!Options.HasFlag(ExpressionOptions.AllowGoto))
          {
            throw new ExpressionSecurityException("Goto is not permitted.");
          }
          break;
      }

      return base.VisitGoto(node);
    }

    protected override Expression VisitInvocation(InvocationExpression node)
    {
      if (!Options.HasFlag(ExpressionOptions.AllowDelegateInvoke))
      {
        throw new ExpressionSecurityException("Delegate and lambda invocations are not permitted.");
      }

      if (!Options.HasFlag(ExpressionOptions.AllowVoidMethodCalls)
        && node.Type == typeof(void))
      {
        throw new ExpressionSecurityException("Calls to void-returning delegates are not permitted.");
      }

      return base.VisitInvocation(node);
    }

    protected override Expression VisitLabel(LabelExpression node)
    {
      if (!Options.HasFlag(ExpressionOptions.AllowGoto))
      {
        throw new ExpressionSecurityException("Goto labels are not permitted.");
      }

      return base.VisitLabel(node);
    }

    protected override Expression VisitLambda<T>(Expression<T> node)
    {
      return base.VisitLambda<T>(node);
    }

    protected override Expression VisitLoop(LoopExpression node)
    {
      if (!Options.HasFlag(ExpressionOptions.AllowLoops))
      {
        throw new ExpressionSecurityException("Loops are not permitted.");
      }

      return base.VisitLoop(node);
    }

    protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
    {
      if (!Options.HasFlag(ExpressionOptions.AllowMemberAssignments))
      {
        throw new ExpressionSecurityException("Member assignments are not permitted.");
      }

      return base.VisitMemberAssignment(node);
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
      if (!Options.HasFlag(ExpressionOptions.AllowVoidMethodCalls)
        && node.Type == typeof(void))
      {
        throw new ExpressionSecurityException("Calls to void-returning methods are not permitted.");
      }

      if (!Context.IsKnownMethod(node.Method))
      {
        throw new ExpressionSecurityException("Calls to method \"" + node.Method + "\" are not permitted.");
      }

      return base.VisitMethodCall(node);
    }

    protected override Expression VisitNew(NewExpression node)
    {
      var type = node.Constructor.DeclaringType;

      if (!Context.IsKnownType(type))
      {
        throw new ExpressionSecurityException("Type \"" + type + "\" is not permitted.");
      }

      if (!Options.HasFlag(ExpressionOptions.AllowConstructors)
        && !type.GetIsPrimitive()
        && !ServiceEvaluationContext.IsExtendedPrimitiveType(type))
      {
        throw new ExpressionSecurityException("Constructors are not permitted.");
      }

      return base.VisitNew(node);
    }

    protected override Expression VisitNewArray(NewArrayExpression node)
    {
      if (!Options.HasFlag(ExpressionOptions.AllowArrayInstantiation))
      {
        throw new ExpressionSecurityException("Array instantiation is not permitted.");
      }

      return base.VisitNewArray(node);
    }

    protected override Expression VisitTry(TryExpression node)
    {
      if (!Options.HasFlag(ExpressionOptions.AllowTryBlocks))
      {
        throw new ExpressionSecurityException("Try blocks are not permitted.");
      }

      return base.VisitTry(node);
    }

    protected override Expression VisitTypeBinary(TypeBinaryExpression node)
    {
      if (!Options.HasFlag(ExpressionOptions.AllowTypeTests))
      {
        throw new ExpressionSecurityException("Type tests are not permitted.");
      }

      return base.VisitTypeBinary(node);
    }

    protected override Expression VisitUnary(UnaryExpression node)
    {
      switch (node.NodeType)
      {
        case ExpressionType.TypeAs:
          if (!Options.HasFlag(ExpressionOptions.AllowTypeTests))
          {
            throw new ExpressionSecurityException("Type tests are not permitted.");
          }
          break;
        case ExpressionType.Convert:
        case ExpressionType.ConvertChecked:
          if (!Options.HasFlag(ExpressionOptions.AllowTypeTests)  // Conversions can be used as brute force type tests
            || !Options.HasFlag(ExpressionOptions.AllowExplicitConversions))
          {
            throw new ExpressionSecurityException("Explicit conversions are not permitted.");
          }
          break;
      }

      return base.VisitUnary(node);
    }
  }
}
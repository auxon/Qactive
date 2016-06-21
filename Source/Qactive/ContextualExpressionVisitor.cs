using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Qactive
{
  internal abstract class ContextualExpressionVisitor : ExpressionVisitor
  {
    private readonly Stack<Type> expectedTypes = new Stack<Type>();

    [ContractInvariantMethod]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
    private void ObjectInvariant()
    {
      Contract.Invariant(expectedTypes != null);
    }

    public bool HasAnyExpectedTypes => expectedTypes.Count > 0;

    protected Type CurrentExpectedType => HasAnyExpectedTypes ? expectedTypes.Peek() : null;

    /// <param name="expectedType">The actual type that is expected for this exprssion's result rather than the concrete type that may be present, or null if no particular type is expected.</param>
    protected Expression Visit(Expression expression, Type expectedType)
    {
      expectedTypes.Push(expectedType);

      try
      {
        return Visit(expression);
      }
      finally
      {
        expectedTypes.Pop();
      }
    }

    /// <param name="expectedType">The actual type that is expected for this exprssion's result rather than the concrete type that may be present, or null if no particular type is expected.</param>
    protected ReadOnlyCollection<Expression> Visit(ReadOnlyCollection<Expression> expressions, Type expectedType)
    {
      expectedTypes.Push(expectedType);

      try
      {
        return Visit(expressions);
      }
      finally
      {
        expectedTypes.Pop();
      }
    }

    /// <param name="expectedType">The actual type that is expected for this exprssion's result rather than the concrete type that may be present, or null if no particular type is expected.</param>
    protected T VisitAndConvert<T>(T node, string callerName, Type expectedType)
      where T : Expression
    {
      expectedTypes.Push(expectedType);

      try
      {
        return VisitAndConvert(node, callerName);
      }
      finally
      {
        expectedTypes.Pop();
      }
    }

    /// <param name="expectedType">The actual type that is expected for this exprssion's result rather than the concrete type that may be present, or null if no particular type is expected.</param>
    protected ReadOnlyCollection<T> VisitAndConvert<T>(ReadOnlyCollection<T> nodes, string callerName, Type expectedType)
      where T : Expression
    {
      expectedTypes.Push(expectedType);

      try
      {
        return VisitAndConvert(nodes, callerName);
      }
      finally
      {
        expectedTypes.Pop();
      }
    }

    /// <param name="expectedType">The actual type that is expected for this exprssion's result rather than the concrete type that may be present, or null if no particular type is expected.</param>
    protected TResult Visit<TSource, TResult>(TSource source, Type expectedType, Func<TSource, TResult> selector)
    {
      Contract.Requires(selector != null);

      expectedTypes.Push(expectedType);

      try
      {
        return selector(source);
      }
      finally
      {
        expectedTypes.Pop();
      }
    }

    protected ReadOnlyCollection<T> Visit<T>(ReadOnlyCollection<T> nodes, Func<T, Type> expectedTypeSelector, Func<T, T> elementVisitor)
    {
      Contract.Requires(nodes != null);
      Contract.Requires(expectedTypeSelector != null);
      Contract.Requires(elementVisitor != null);
      Contract.Ensures(Contract.Result<ReadOnlyCollection<T>>() != null);

      return Visit(nodes, value => Visit(value, expectedTypeSelector(value), elementVisitor));
    }

    private Expression VisitBase<TExpression>(TExpression node, Func<TExpression, Expression> baseCall, Type expectedType = null)
      where TExpression : Expression
    {
      Contract.Requires(baseCall != null);

      expectedTypes.Push(expectedType);

      try
      {
        return baseCall(node);
      }
      finally
      {
        expectedTypes.Pop();
      }
    }

    private static Type GetMemberType(MemberInfo member)
      => (member as FieldInfo)?.FieldType ?? (member as PropertyInfo)?.PropertyType;

    protected virtual Expression VisitBinary(BinaryExpression node, Type expectedType)
      => node.Update(Visit(node.Left, node.Method?.GetParameters()[0].ParameterType),
                     VisitAndConvert(node.Conversion, "VisitBinary", expectedType),
                     Visit(node.Right, node.Method?.GetParameters()[1].ParameterType));

    protected virtual Expression VisitBlock(BlockExpression node, Type expectedType)
      => VisitBase(node, base.VisitBlock);

    protected virtual Expression VisitConditional(ConditionalExpression node, Type expectedType)
      => node.Update(Visit(node.Test, typeof(bool)), Visit(node.IfTrue, node.Type), Visit(node.IfFalse, node.Type));

    protected virtual Expression VisitConstant(ConstantExpression node, Type expectedType)
      => base.VisitConstant(node);

    protected virtual Expression VisitDebugInfo(DebugInfoExpression node, Type expectedType)
      => base.VisitDebugInfo(node);

#if DYNAMIC
    protected virtual Expression VisitDynamic(DynamicExpression node, Type expectedType)
      => VisitBase(node, base.VisitDynamic, expectedType);
#endif

    protected virtual Expression VisitDefault(DefaultExpression node, Type expectedType)
      => base.VisitDefault(node);

    protected virtual Expression VisitExtension(Expression node, Type expectedType)
      => VisitBase(node, base.VisitExtension);

    protected virtual Expression VisitGoto(GotoExpression node, Type expectedType)
      => node.Update(VisitLabelTarget(node.Target, expectedType), Visit(node.Value, node.Target.Type == typeof(void) ? null : node.Target.Type));

    protected virtual Expression VisitInvocation(InvocationExpression node, Type expectedType)
      => VisitBase(node, base.VisitInvocation);

    protected virtual LabelTarget VisitLabelTarget(LabelTarget node, Type expectedType)
      => base.VisitLabelTarget(node);

    protected virtual Expression VisitLabel(LabelExpression node, Type expectedType)
      => node.Update(VisitLabelTarget(node.Target, expectedType), Visit(node.DefaultValue, expectedType));

    protected virtual Expression VisitLambda<T>(Expression<T> node, Type expectedType)
      => node.Update(Visit(node.Body, node.ReturnType), VisitAndConvert(node.Parameters, "VisitLambda", null));

    protected virtual Expression VisitLoop(LoopExpression node, Type expectedType)
      => node.Update(VisitLabelTarget(node.BreakLabel, expectedType), VisitLabelTarget(node.ContinueLabel, expectedType), Visit(node.Body, node.Type));

    protected virtual Expression VisitMember(MemberExpression node, Type expectedType)
      => node.Update(Visit(node.Expression, GetMemberType(node.Member)));

    protected virtual Expression VisitIndex(IndexExpression node, Type expectedType)
      => VisitBase(node, base.VisitIndex);

    protected virtual Expression VisitMethodCall(MethodCallExpression node, Type expectedType)
      => VisitBase(node, base.VisitMethodCall);

    protected virtual Expression VisitNewArray(NewArrayExpression node, Type expectedType)
      => node.Update(node.NodeType == ExpressionType.NewArrayInit
                   ? Visit(node.Expressions, node.Type)
                   : Visit(node.Expressions, null));

    protected virtual Expression VisitNew(NewExpression node, Type expectedType)
      => VisitBase(node, base.VisitNew);

    protected virtual Expression VisitParameter(ParameterExpression node, Type expectedType)
      => base.VisitParameter(node);

    protected virtual Expression VisitRuntimeVariables(RuntimeVariablesExpression node, Type expectedType)
      => VisitBase(node, base.VisitRuntimeVariables);

    protected virtual SwitchCase VisitSwitchCase(SwitchCase node, Type expectedType)
      => node.Update(Visit(node.TestValues, null), Visit(node.Body, expectedType));

    protected virtual Expression VisitSwitch(SwitchExpression node, Type expectedType)
      => node.Update(Visit(node.SwitchValue, node.Type), Visit(node.Cases, _ => node.Type, VisitSwitchCase), Visit(node.DefaultBody, expectedType));

    protected virtual CatchBlock VisitCatchBlock(CatchBlock node, Type expectedType)
      => node.Update(VisitAndConvert(node.Variable, "VisitCatchBlock", null), Visit(node.Filter, typeof(bool)), Visit(node.Body, expectedType));

    protected virtual Expression VisitTry(TryExpression node, Type expectedType)
      => node.Update(Visit(node.Body, expectedType), Visit(node.Handlers, _ => null, VisitCatchBlock), Visit(node.Finally, null), Visit(node.Fault, null));

    protected virtual Expression VisitTypeBinary(TypeBinaryExpression node, Type expectedType)
      => node.Update(Visit(node.Expression, null));

    protected virtual Expression VisitUnary(UnaryExpression node, Type expectedType)
      => node.Update(Visit(node.Operand, node.Method?.GetParameters()[0].ParameterType));

    protected virtual Expression VisitMemberInit(MemberInitExpression node, Type expectedType)
      => node.Update(VisitAndConvert(node.NewExpression, "VisitMemberInit", expectedType), Visit(node.Bindings, binding => GetMemberType(binding.Member), VisitMemberBinding));

    protected virtual Expression VisitListInit(ListInitExpression node, Type expectedType)
      => base.VisitListInit(node);

    protected virtual ElementInit VisitElementInit(ElementInit node, Type expectedType)
      => node.Update(node.Arguments.Zip(node.AddMethod.GetParameters(), (arg, param) => Visit(arg, param.ParameterType)));

    protected virtual MemberBinding VisitMemberBinding(MemberBinding node, Type expectedType)
    {
      switch (node.BindingType)
      {
        case MemberBindingType.Assignment:
          return VisitMemberAssignment((MemberAssignment)node, expectedType);
        case MemberBindingType.MemberBinding:
          return VisitMemberMemberBinding((MemberMemberBinding)node, expectedType);
        case MemberBindingType.ListBinding:
          return VisitMemberListBinding((MemberListBinding)node, expectedType);
        default:
          return base.VisitMemberBinding(node);
      }
    }

    protected virtual MemberAssignment VisitMemberAssignment(MemberAssignment node, Type expectedType)
      => node.Update(Visit(node.Expression, GetMemberType(node.Member)));

    protected virtual MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding node, Type expectedType)
      => node.Update(Visit(node.Bindings, binding => GetMemberType(binding.Member), VisitMemberBinding));

    protected virtual MemberListBinding VisitMemberListBinding(MemberListBinding node, Type expectedType)
      => node.Update(Visit(node.Initializers, init => null, VisitElementInit));

    protected sealed override Expression VisitBinary(BinaryExpression node) => VisitBinary(node, CurrentExpectedType);

    protected sealed override Expression VisitBlock(BlockExpression node) => VisitBlock(node, CurrentExpectedType);

    protected sealed override Expression VisitConditional(ConditionalExpression node) => VisitConditional(node, CurrentExpectedType);

    protected sealed override Expression VisitConstant(ConstantExpression node) => VisitConstant(node, CurrentExpectedType);

    protected sealed override Expression VisitDebugInfo(DebugInfoExpression node) => VisitDebugInfo(node, CurrentExpectedType);

#if DYNAMIC
    protected sealed override Expression VisitDynamic(DynamicExpression node) => VisitDynamic(node, CurrentExpectedType);
#endif

    protected sealed override Expression VisitDefault(DefaultExpression node) => VisitDefault(node, CurrentExpectedType);

    protected sealed override Expression VisitExtension(Expression node) => VisitExtension(node, CurrentExpectedType);

    protected sealed override Expression VisitGoto(GotoExpression node) => VisitGoto(node, CurrentExpectedType);

    protected sealed override Expression VisitInvocation(InvocationExpression node) => VisitInvocation(node, CurrentExpectedType);

    protected sealed override LabelTarget VisitLabelTarget(LabelTarget node) => VisitLabelTarget(node, CurrentExpectedType);

    protected sealed override Expression VisitLabel(LabelExpression node) => VisitLabel(node, CurrentExpectedType);

    protected sealed override Expression VisitLambda<T>(Expression<T> node) => VisitLambda<T>(node, CurrentExpectedType);

    protected sealed override Expression VisitLoop(LoopExpression node) => VisitLoop(node, CurrentExpectedType);

    protected sealed override Expression VisitMember(MemberExpression node) => VisitMember(node, CurrentExpectedType);

    protected sealed override Expression VisitIndex(IndexExpression node) => VisitIndex(node, CurrentExpectedType);

    protected sealed override Expression VisitMethodCall(MethodCallExpression node) => VisitMethodCall(node, CurrentExpectedType);

    protected sealed override Expression VisitNewArray(NewArrayExpression node) => VisitNewArray(node, CurrentExpectedType);

    protected sealed override Expression VisitNew(NewExpression node) => VisitNew(node, CurrentExpectedType);

    protected sealed override Expression VisitParameter(ParameterExpression node) => VisitParameter(node, CurrentExpectedType);

    protected sealed override Expression VisitRuntimeVariables(RuntimeVariablesExpression node) => VisitRuntimeVariables(node, CurrentExpectedType);

    protected sealed override SwitchCase VisitSwitchCase(SwitchCase node) => VisitSwitchCase(node, CurrentExpectedType);

    protected sealed override Expression VisitSwitch(SwitchExpression node) => VisitSwitch(node, CurrentExpectedType);

    protected sealed override CatchBlock VisitCatchBlock(CatchBlock node) => VisitCatchBlock(node, CurrentExpectedType);

    protected sealed override Expression VisitTry(TryExpression node) => VisitTry(node, CurrentExpectedType);

    protected sealed override Expression VisitTypeBinary(TypeBinaryExpression node) => VisitTypeBinary(node, CurrentExpectedType);

    protected sealed override Expression VisitUnary(UnaryExpression node) => VisitUnary(node, CurrentExpectedType);

    protected sealed override Expression VisitMemberInit(MemberInitExpression node) => VisitMemberInit(node, CurrentExpectedType);

    protected sealed override Expression VisitListInit(ListInitExpression node) => VisitListInit(node, CurrentExpectedType);

    protected sealed override ElementInit VisitElementInit(ElementInit node) => VisitElementInit(node, CurrentExpectedType);

    protected sealed override MemberBinding VisitMemberBinding(MemberBinding node) => VisitMemberBinding(node, CurrentExpectedType);

    protected sealed override MemberAssignment VisitMemberAssignment(MemberAssignment node) => VisitMemberAssignment(node, CurrentExpectedType);

    protected sealed override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding node) => VisitMemberMemberBinding(node, CurrentExpectedType);

    protected sealed override MemberListBinding VisitMemberListBinding(MemberListBinding node) => VisitMemberListBinding(node, CurrentExpectedType);
  }
}

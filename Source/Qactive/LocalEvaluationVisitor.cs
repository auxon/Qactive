using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Qactive.Properties;

namespace Qactive
{
  internal sealed class LocalEvaluationVisitor : ExpressionVisitor
  {
    private readonly LocalEvaluator evaluator;
    private readonly IQbservableProtocol protocol;
    private readonly Stack<Type> expectedTypes = new Stack<Type>();

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Used by Contract.Assume")]
    public bool IsStackEmpty
    {
      get { return expectedTypes.Count == 0; }
    }

    public LocalEvaluationVisitor(LocalEvaluator evaluator, IQbservableProtocol protocol)
    {
      Contract.Requires(evaluator != null);
      Contract.Requires(protocol != null);

      this.evaluator = evaluator;
      this.protocol = protocol;
    }

    [ContractInvariantMethod]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
    private void ObjectInvariant()
    {
      Contract.Invariant(evaluator != null);
      Contract.Invariant(protocol != null);
    }

    /// <param name="expectedType">The actual type that is expected for this exprssion's result rather than the concrete type that may be present, or null if no particular type is expected.</param>
    private Expression Visit(Expression expression, Type expectedType)
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
    private ReadOnlyCollection<Expression> Visit(ReadOnlyCollection<Expression> expressions, Type expectedType)
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
    private T VisitAndConvert<T>(T node, string callerName, Type expectedType)
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
    private ReadOnlyCollection<T> VisitAndConvert<T>(ReadOnlyCollection<T> nodes, string callerName, Type expectedType)
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

    protected override Expression VisitBinary(BinaryExpression node)
    {
      if (node.NodeType == ExpressionType.Assign)
      {
        MethodCallExpression newNode = null;

        if (evaluator.EnsureKnownType(
          node.Left.Type,
          replaceCompilerGeneratedType: _ => newNode = CompilerGenerated.Set(Visit(node.Left, node.Method.GetParameters()[0].ParameterType), Visit(node.Right, node.Method.GetParameters()[1].ParameterType))))
        {
          return newNode;
        }
      }

      return base.VisitBinary(node);
    }

    protected override Expression VisitBlock(BlockExpression node)
    {
      if (evaluator.EnsureKnownType(
        node.Type,
        genericArgumentsUpdated: updatedType => node = Expression.Block(
          updatedType,
          VisitAndConvert(node.Variables, "VisitBlock-Variables", null),
          Visit(node.Expressions, null))))
      {
        return node;
      }
      else
      {
        return base.VisitBlock(node);
      }
    }

    protected override CatchBlock VisitCatchBlock(CatchBlock node)
    {
      if (evaluator.EnsureKnownType(
        node.Test,
        genericArgumentsUpdated: updatedType => node = Expression.MakeCatchBlock(
          updatedType,
          VisitAndConvert(node.Variable, "VisitCatchBlock-Variable", null),
          Visit(node.Body, null),
          Visit(node.Filter, null))))
      {
        return node;
      }
      else
      {
        return base.VisitCatchBlock(node);
      }
    }

    protected override Expression VisitConditional(ConditionalExpression node)
    {
      if (evaluator.EnsureKnownType(
        node.Type,
        genericArgumentsUpdated: updatedType => node = Expression.Condition(
          Visit(node.Test, typeof(bool)),
          Visit(node.IfTrue, node.Type),
          Visit(node.IfFalse, node.Type),
          updatedType)))
      {
        return node;
      }
      else
      {
        return base.VisitConditional(node);
      }
    }

    protected override Expression VisitConstant(ConstantExpression node)
    {
      var type = node.Value == null ? null : node.Value.GetType();

      if (evaluator.EnsureKnownType(
        type,
        replaceCompilerGeneratedType: _ =>
        {
          throw new InvalidOperationException(Errors.ExpressionClosureBug);
        }))
      {
        return node;
      }
      else
      {
        return base.VisitConstant(node);
      }
    }

    protected override Expression VisitDefault(DefaultExpression node)
    {
      if (evaluator.EnsureKnownType(
        node.Type,
        genericArgumentsUpdated: updatedType => node = Expression.Default(updatedType)))
      {
        return node;
      }
      else
      {
        return base.VisitDefault(node);
      }
    }

    protected override Expression VisitGoto(GotoExpression node)
    {
      evaluator.EnsureKnownType(node.Type);
      evaluator.EnsureKnownType(node.Target);

      return base.VisitGoto(node);
    }

    protected override Expression VisitIndex(IndexExpression node)
    {
      evaluator.EnsureKnownType(node.Indexer);

      return base.VisitIndex(node);
    }

    protected override Expression VisitInvocation(InvocationExpression node)
    {
      evaluator.EnsureKnownType(node.Type);

      return base.VisitInvocation(node);
    }

    protected override Expression VisitLabel(LabelExpression node)
    {
      evaluator.EnsureKnownType(node.Target);

      return base.VisitLabel(node);
    }

    protected override Expression VisitLambda<T>(Expression<T> node)
    {
      LambdaExpression newNode = null;

      var delegateType = node.Type;

      if (evaluator.EnsureKnownType(
        node.Type,
        replaceCompilerGeneratedType: _ =>
        {
          Type unboundReturnType;

          if (!delegateType.GetIsGenericType()
            || !(unboundReturnType = node.ReturnType).IsGenericParameter)
          {
            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Errors.ExpressionDelegateReturnsNonGenericAnonymousType, Environment.NewLine, delegateType));
          }

          var genericArguments = delegateType.GetGenericArguments().ToArray();

          genericArguments[unboundReturnType.GenericParameterPosition] = typeof(CompilerGenerated);

          delegateType = delegateType.GetGenericTypeDefinition().MakeGenericType(genericArguments);

          newNode = Expression.Lambda(
            delegateType,
            Visit(node.Body, node.ReturnType),
            node.Name,
            node.TailCall,
            VisitAndConvert(node.Parameters, "VisitLambda-Parameters", null));
        },
        genericArgumentsUpdated: updatedType =>
          newNode = Expression.Lambda(
            updatedType,
            Visit(node.Body, node.ReturnType),
            node.Name,
            node.TailCall,
            VisitAndConvert(node.Parameters, "VisitLambda-Parameters", null))))
      {
        return newNode;
      }
      else
      {
        return Expression.Lambda(
          delegateType,
          Visit(node.Body, node.ReturnType),
          node.Name,
          node.TailCall,
          VisitAndConvert(node.Parameters, "VisitLambda-Parameters", null));
      }
    }

    protected override Expression VisitListInit(ListInitExpression node)
    {
      evaluator.EnsureKnownTypes(node.Initializers.Select(i => i.AddMethod));

      return base.VisitListInit(node);
    }

    protected override Expression VisitLoop(LoopExpression node)
    {
      evaluator.EnsureKnownType(node.BreakLabel);
      evaluator.EnsureKnownType(node.ContinueLabel);

      return base.VisitLoop(node);
    }

    protected override Expression VisitMember(MemberExpression node)
    {
      Expression newNode = null;

      if (evaluator.EnsureKnownType(
        node.Member,
        replaceCompilerGeneratedType: _ =>
        {
          newNode = evaluator.EvaluateCompilerGenerated(node, expectedTypes.Count == 0 ? null : expectedTypes.Peek(), protocol)
                 ?? CompilerGenerated.Get(
                      Visit(node.Expression, null),
                      node.Member,
                      type =>
                      {
                        evaluator.EnsureKnownType(
                          type,
                          replaceCompilerGeneratedType: __ => type = typeof(CompilerGenerated),
                          genericArgumentsUpdated: updatedType => type = updatedType);

                        return type;
                      });
        },
        unknownType: (_, __) => newNode = evaluator.GetValue(node, this, protocol)))
      {
        return newNode;
      }
      else
      {
        return base.VisitMember(node);
      }
    }

    protected override Expression VisitMemberInit(MemberInitExpression node)
    {
      evaluator.EnsureKnownType(node.Type, replaceCompilerGeneratedType: _ => { });

      foreach (var binding in node.Bindings)
      {
        evaluator.EnsureKnownType(binding.Member);

        var list = binding as MemberListBinding;

        if (list != null)
        {
          evaluator.EnsureKnownTypes(list.Initializers.Select(i => i.AddMethod));
        }
      }

      return base.VisitMemberInit(node);
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
      Expression newNode = null;

      if (evaluator.EnsureKnownType(
        node.Method,
        unknownType: (_, __) => newNode = evaluator.Invoke(node, this, protocol),
        genericMethodArgumentsUpdated: updatedMethod => newNode = Expression.Call(Visit(node.Object, null), updatedMethod, Visit(node.Arguments, null))))
      {
        return newNode;
      }
      else
      {
        return base.VisitMethodCall(node);
      }
    }

    protected override Expression VisitNew(NewExpression node)
    {
      NewExpression newNode = null;

      if (evaluator.EnsureKnownType(
        node.Constructor,
        replaceCompilerGeneratedType: _ => newNode = CompilerGenerated.New(node.Members, Visit(node.Arguments, null)),
        genericArgumentsUpdated: updatedType =>
        {
          /* Overload resolution should be unaffected here, so keep the same constructor index.  We're just swapping a 
           * compiler-generated type for the internal CompilerGenerated class, neither of which can be statically 
           * referenced by users.
           */
          var flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

          var oldConstructorIndex = Array.IndexOf(node.Constructor.DeclaringType.GetConstructors(flags), node.Constructor);
          var newConstructor = updatedType.GetConstructors(flags)[oldConstructorIndex];

          newNode = Expression.New(newConstructor, Visit(node.Arguments, null), node.Members);
        }))
      {
        return newNode;
      }

      return base.VisitNew(node);
    }

    protected override Expression VisitNewArray(NewArrayExpression node)
    {
      NewArrayExpression newNode = null;

      if (evaluator.EnsureKnownType(
        node.Type.GetElementType(),
        replaceCompilerGeneratedType: updatedType => newNode = node.NodeType == ExpressionType.NewArrayInit
                                                   ? Expression.NewArrayInit(typeof(CompilerGenerated), Visit(node.Expressions, node.Type))
                                                   : Expression.NewArrayBounds(typeof(CompilerGenerated), Visit(node.Expressions, null))))
      {
        return newNode;
      }
      else
      {
        return base.VisitNewArray(node);
      }
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
      // TODO: The current caching implementation is weak.  It must support name scopes, instead of just globally keying by name.
      if (evaluator.ReplacedParameters.ContainsKey(node.Name))
      {
        return evaluator.ReplacedParameters[node.Name];
      }

      ParameterExpression newNode = null;

      if (evaluator.EnsureKnownType(
        node.Type,
        replaceCompilerGeneratedType: _ => newNode = Expression.Parameter(typeof(CompilerGenerated), node.Name),
        genericArgumentsUpdated: updatedType => newNode = Expression.Parameter(updatedType, node.Name)))
      {
        evaluator.ReplacedParameters.Add(newNode.Name, newNode);

        return newNode;
      }
      else
      {
        return base.VisitParameter(node);
      }
    }

    protected override Expression VisitSwitch(SwitchExpression node)
    {
      evaluator.EnsureKnownType(node.Comparison);

      SwitchExpression newNode = null;

      if (evaluator.EnsureKnownType(
        node.Type,
        genericArgumentsUpdated: updatedType => newNode = Expression.Switch(
          updatedType,
          Visit(node.SwitchValue, node.Type),
          Visit(node.DefaultBody, node.Type),
          node.Comparison,
          Visit(node.Cases, VisitSwitchCase))))
      {
        return newNode;
      }
      else
      {
        return base.VisitSwitch(node);
      }
    }

    protected override Expression VisitTry(TryExpression node)
    {
      TryExpression newNode = null;

      if (evaluator.EnsureKnownType(
        node.Type,
        genericArgumentsUpdated: updatedType => newNode = Expression.MakeTry(
          updatedType,
          Visit(node.Body, node.Type),
          Visit(node.Finally, null),
          Visit(node.Fault, null),
          Visit(node.Handlers, VisitCatchBlock))))
      {
        return newNode;
      }
      else
      {
        return base.VisitTry(node);
      }
    }

    protected override Expression VisitTypeBinary(TypeBinaryExpression node)
    {
      TypeBinaryExpression newNode = null;

      if (evaluator.EnsureKnownType(
        node.TypeOperand,
        genericArgumentsUpdated: updatedType => Expression.TypeIs(Visit(node.Expression, null), updatedType)))
      {
        return newNode;
      }
      else
      {
        return base.VisitTypeBinary(node);
      }
    }

    protected override Expression VisitUnary(UnaryExpression node)
    {
      evaluator.EnsureKnownType(node.Method);

      UnaryExpression newNode = null;

      if (node.NodeType != ExpressionType.Quote
        && evaluator.EnsureKnownType(
          node.Type,
          genericArgumentsUpdated: updatedType => newNode = Expression.MakeUnary(
            node.NodeType,
            Visit(node.Operand, node.Method.GetParameters()[0].ParameterType),
            updatedType,
            node.Method)))
      {
        return newNode;
      }
      else
      {
        return base.VisitUnary(node);
      }
    }
  }
}
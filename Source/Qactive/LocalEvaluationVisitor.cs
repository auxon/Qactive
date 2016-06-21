using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Qactive.Properties;

namespace Qactive
{
  internal sealed class LocalEvaluationVisitor : ContextualExpressionVisitor
  {
    private readonly LocalEvaluator evaluator;
    private readonly IQbservableProtocol protocol;

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

    protected override Expression VisitBinary(BinaryExpression node, Type expectedType)
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

      return base.VisitBinary(node, expectedType);
    }

    protected override Expression VisitBlock(BlockExpression node, Type expectedType)
    {
      if (evaluator.EnsureKnownType(
        node.Type,
        genericArgumentsUpdated: updatedType => node = Expression.Block(
          updatedType,
          VisitAndConvert(node.Variables, "VisitBlock-Variables", null),
          Visit(node.Expressions, (Type)null))))
      {
        return node;
      }
      else
      {
        return base.VisitBlock(node, expectedType);
      }
    }

    protected override CatchBlock VisitCatchBlock(CatchBlock node, Type expectedType)
    {
      if (evaluator.EnsureKnownType(
        node.Test,
        genericArgumentsUpdated: updatedType => node = Expression.MakeCatchBlock(
          updatedType,
          VisitAndConvert(node.Variable, "VisitCatchBlock-Variable", null),
          Visit(node.Body, expectedType),
          Visit(node.Filter, typeof(bool)))))
      {
        return node;
      }
      else
      {
        return base.VisitCatchBlock(node, expectedType);
      }
    }

    protected override Expression VisitConditional(ConditionalExpression node, Type expectedType)
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
        return base.VisitConditional(node, expectedType);
      }
    }

    protected override Expression VisitConstant(ConstantExpression node, Type expectedType)
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
        return evaluator.TryEvaluateSequences((node.Value?.GetType() ?? node.Type).Name, node.Value, node.Type, expectedType, protocol)
            ?? base.VisitConstant(node, expectedType);
      }
    }

    protected override Expression VisitDefault(DefaultExpression node, Type expectedType)
    {
      if (evaluator.EnsureKnownType(
        node.Type,
        genericArgumentsUpdated: updatedType => node = Expression.Default(updatedType)))
      {
        return node;
      }
      else
      {
        return base.VisitDefault(node, expectedType);
      }
    }

    protected override Expression VisitGoto(GotoExpression node, Type expectedType)
    {
      evaluator.EnsureKnownType(node.Type);
      evaluator.EnsureKnownType(node.Target);

      return base.VisitGoto(node, expectedType);
    }

    protected override Expression VisitIndex(IndexExpression node, Type expectedType)
    {
      evaluator.EnsureKnownType(node.Indexer);

      return base.VisitIndex(node, expectedType);
    }

    protected override Expression VisitInvocation(InvocationExpression node, Type expectedType)
    {
      evaluator.EnsureKnownType(node.Type);

      return base.VisitInvocation(node, expectedType);
    }

    protected override Expression VisitLabel(LabelExpression node, Type expectedType)
    {
      evaluator.EnsureKnownType(node.Target);

      return base.VisitLabel(node, expectedType);
    }

    protected override Expression VisitLambda<T>(Expression<T> node, Type expectedType)
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

    protected override Expression VisitListInit(ListInitExpression node, Type expectedType)
    {
      evaluator.EnsureKnownTypes(node.Initializers.Select(i => i.AddMethod));

      return base.VisitListInit(node, expectedType);
    }

    protected override Expression VisitLoop(LoopExpression node, Type expectedType)
    {
      evaluator.EnsureKnownType(node.BreakLabel);
      evaluator.EnsureKnownType(node.ContinueLabel);

      return base.VisitLoop(node, expectedType);
    }

    protected override Expression VisitMember(MemberExpression node, Type expectedType)
    {
      Expression newNode = null;

      if (evaluator.EnsureKnownType(
        node.Member,
        replaceCompilerGeneratedType: _ =>
        {
          newNode = evaluator.EvaluateCompilerGenerated(node, expectedType, protocol)
                 ?? CompilerGenerated.Get(
                      Visit(node.Expression, (node.Member as FieldInfo)?.FieldType ?? (node.Member as PropertyInfo)?.PropertyType),
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
        return base.VisitMember(node, expectedType);
      }
    }

    protected override Expression VisitMemberInit(MemberInitExpression node, Type expectedType)
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

      return base.VisitMemberInit(node, expectedType);
    }

    protected override Expression VisitMethodCall(MethodCallExpression node, Type expectedType)
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
        return base.VisitMethodCall(node, expectedType);
      }
    }

    protected override Expression VisitNew(NewExpression node, Type expectedType)
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

      return base.VisitNew(node, expectedType);
    }

    protected override Expression VisitNewArray(NewArrayExpression node, Type expectedType)
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
        return base.VisitNewArray(node, expectedType);
      }
    }

    protected override Expression VisitParameter(ParameterExpression node, Type expectedType)
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
        return base.VisitParameter(node, expectedType);
      }
    }

    protected override Expression VisitSwitch(SwitchExpression node, Type expectedType)
    {
      evaluator.EnsureKnownType(node.Comparison);

      SwitchExpression newNode = null;

      if (evaluator.EnsureKnownType(
        node.Type,
        genericArgumentsUpdated: updatedType => newNode = Expression.Switch(
          updatedType,
          Visit(node.SwitchValue, node.Type),
          Visit(node.DefaultBody, expectedType),
          node.Comparison,
          Visit(node.Cases, _ => node.Type, VisitSwitchCase))))
      {
        return newNode;
      }
      else
      {
        return base.VisitSwitch(node, expectedType);
      }
    }

    protected override Expression VisitTry(TryExpression node, Type expectedType)
    {
      TryExpression newNode = null;

      if (evaluator.EnsureKnownType(
        node.Type,
        genericArgumentsUpdated: updatedType => newNode = Expression.MakeTry(
          updatedType,
          Visit(node.Body, expectedType),
          Visit(node.Finally, null),
          Visit(node.Fault, null),
          Visit(node.Handlers, _ => null, VisitCatchBlock))))
      {
        return newNode;
      }
      else
      {
        return base.VisitTry(node, expectedType);
      }
    }

    protected override Expression VisitTypeBinary(TypeBinaryExpression node, Type expectedType)
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
        return base.VisitTypeBinary(node, expectedType);
      }
    }

    protected override Expression VisitUnary(UnaryExpression node, Type expectedType)
    {
      evaluator.EnsureKnownType(node.Method);

      UnaryExpression newNode = null;

      if (node.NodeType != ExpressionType.Quote
        && evaluator.EnsureKnownType(
          node.Type,
          genericArgumentsUpdated: updatedType => newNode = Expression.MakeUnary(
            node.NodeType,
            Visit(node.Operand, node.Method?.GetParameters()[0].ParameterType),
            updatedType,
            node.Method)))
      {
        return newNode;
      }
      else
      {
        return base.VisitUnary(node, expectedType);
      }
    }
  }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Qactive.Expressions
{
  public class EqualityExpressionVisitor : ExpressionVisitor
  {
    private readonly Stack<Expression> others = new Stack<Expression>();
    private readonly Func<Expression, Expression, bool> shallowEquals;
    private readonly Func<Type, Type, bool> typeEquals;
    private readonly Func<MemberInfo, MemberInfo, bool> memberEquals;

#if READONLYCOLLECTIONS
    private IReadOnlyCollection<Expression> inequalityNodes, inequalityOthers;
#else
    private ReadOnlyCollection<Expression> inequalityNodes, inequalityOthers;
#endif

    public bool AreEqual { get; private set; } = true;

#if READONLYCOLLECTIONS
    public IReadOnlyCollection<Expression> InequalityNodes => inequalityNodes ?? new Expression[0];

    public IReadOnlyCollection<Expression> InequalityOthers => inequalityOthers ?? new Expression[0];
#else
    public ReadOnlyCollection<Expression> InequalityNodes => inequalityNodes ?? new ReadOnlyCollection<Expression>(new Expression[0]);

    public ReadOnlyCollection<Expression> InequalityOthers => inequalityOthers ?? new ReadOnlyCollection<Expression>(new Expression[0]);
#endif

    public int StackCount => others.Count;

    protected internal EqualityExpressionVisitor(Func<Expression, Expression, bool> shallowEquals, Func<Type, Type, bool> typeEquals, Func<MemberInfo, MemberInfo, bool> memberEquals)
    {
      Contract.Requires(shallowEquals != null);
      Contract.Requires(typeEquals != null);
      Contract.Requires(memberEquals != null);

      this.shallowEquals = shallowEquals;
      this.typeEquals = typeEquals;
      this.memberEquals = memberEquals;
    }

    [ContractInvariantMethod]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
    private void ObjectInvariant()
    {
      Contract.Invariant(others != null);
    }

    internal bool ShallowEquals(Expression first, Expression second) => shallowEquals(first, second);

    internal bool TypeEquals(Type first, Type second) => typeEquals(first, second);

    internal bool MemberEquals(MemberInfo first, MemberInfo second) => memberEquals(first, second);

    public void Break(IEnumerable<Expression> inequalityNodes, IEnumerable<Expression> inequalityOthers)
    {
      AreEqual = false;

      this.inequalityNodes = inequalityNodes.ToList().AsReadOnly();
      this.inequalityOthers = inequalityOthers.ToList().AsReadOnly();
    }

    internal void SetOtherRoot(Expression other)
    {
      Contract.Requires(other != null);

      others.Push(other);
    }

    protected virtual bool? AreEqualShortCircuit(Expression node, Expression other)
    {
      Contract.Requires(node != null);
      Contract.Requires(other != null);
      return null;
    }

    private Expression VisitIfEqual<TExpression>(TExpression node, Func<TExpression, bool> areEqual, params Action<TExpression>[] visits)
      where TExpression : Expression
    {
      Contract.Requires(node != null);
      Contract.Requires(areEqual != null);
      Contract.Requires(visits != null);

      var other = others.Peek();
      var areEqualOrUndecided = other == null ? false : AreEqualShortCircuit(node, other);

      if (areEqualOrUndecided.HasValue)
      {
        if (!areEqualOrUndecided.Value)
        {
          Break(new[] { node }, new[] { other });
        }
      }
      else
      {
        var otherCast = (TExpression)other;

        if (areEqual(otherCast))
        {
          for (var i = 0; AreEqual && i < visits.Length; i++)
          {
            visits[i](otherCast);
          }
        }
        else
        {
          Break(new[] { node }, new[] { other });
        }
      }

      return node;
    }

    private Expression VisitChildren<TExpression>(TExpression source, params Action<TExpression>[] visits)
      where TExpression : Expression
    {
      Contract.Requires(source != null);
      Contract.Requires(visits != null);

      return VisitIfEqual(source, _ => true, visits);
    }

    private void Visit(Expression node, Expression other)
    {
      if (AreEqual)
      {
        if (!shallowEquals(node, other))
        {
          Break(new[] { node }, new[] { other });
          return;
        }

        others.Push(other);
        try
        {
          base.Visit(node);
        }
        finally
        {
          others.Pop();
        }
      }
    }

    private void Visit<TExpression>(ICollection<TExpression> nodes, ICollection<TExpression> others)
      where TExpression : Expression
    {
      if (AreEqual)
      {
        if (!(ExpressionEqualityComparer.NullsOrEquals(nodes, others, (n, o) => n.Count == o.Count)))
        {
          Break(nodes, others);
        }
        else if (nodes != null && others != null)
        {
          foreach (var pair in nodes.Zip(others, (node, other) => new { node, other }))
          {
            Visit(pair.node, pair.other);
          }
        }
      }
    }

    protected override Expression VisitBinary(BinaryExpression node)
      => VisitIfEqual(node, other => memberEquals(node.Method, other.Method), other => Visit(node.Left, other.Left), other => Visit(node.Conversion, other.Conversion), other => Visit(node.Right, other.Right));

    protected override Expression VisitBlock(BlockExpression node)
      => VisitChildren(node, other => Visit(node.Variables, other.Variables), other => Visit(node.Expressions, other.Expressions));

    protected override Expression VisitConditional(ConditionalExpression node)
      => VisitChildren(node, other => Visit(node.Test, other.Test), other => Visit(node.IfFalse, other.IfFalse), other => Visit(node.IfTrue, other.IfTrue));

    protected override Expression VisitConstant(ConstantExpression node)
      => VisitIfEqual(node, other => object.Equals(node.Value, other.Value));

    protected override Expression VisitDebugInfo(DebugInfoExpression node)
      => VisitIfEqual(node, other => node.EndColumn == other.EndColumn
                                  && node.EndLine == other.EndLine
                                  && node.IsClear == other.IsClear
                                  && node.StartColumn == other.StartColumn
                                  && node.StartLine == other.StartLine
                                  && ExpressionEqualityComparer.NullsOrEquals(node.Document, other.Document, (n, o) =>
                                       n.DocumentType == o.DocumentType
                                    && n.FileName == o.FileName
                                    && n.Language == o.Language
                                    && n.LanguageVendor == o.LanguageVendor));

#if DYNAMIC
    protected override Expression VisitDynamic(DynamicExpression node)
      => VisitIfEqual(node, other => typeEquals(node.DelegateType, other.DelegateType) && ExpressionEqualityComparer.NullsOrEquals(node.Binder, other.Binder, (n, o) => n.Equals(o)), other => Visit(node.Arguments, other.Arguments));
#endif

    protected override Expression VisitDefault(DefaultExpression node)
      => node;

    protected override Expression VisitExtension(Expression node)
      => base.VisitExtension(node);

    protected override Expression VisitGoto(GotoExpression node)
      => VisitIfEqual(node, other => node.Kind == other.Kind && Equals(node.Target, other.Target), other => Visit(node.Value, other.Value));

    protected override Expression VisitInvocation(InvocationExpression node)
      => VisitChildren(node, other => Visit(node.Expression, other.Expression), other => Visit(node.Arguments, other.Arguments));

    protected override LabelTarget VisitLabelTarget(LabelTarget node)
    {
      throw new InvalidOperationException("Bug: LabelTarget must not be visited since it's compared within the expressions that contain it.");
    }

    protected override Expression VisitLabel(LabelExpression node)
      => VisitIfEqual(node, other => Equals(node.Target, other.Target), other => Visit(node.DefaultValue, other.DefaultValue));

    protected override Expression VisitLambda<T>(Expression<T> node)
      => VisitIfEqual(node, other => node.Name == other.Name && typeEquals(node.ReturnType, other.ReturnType) && node.TailCall == other.TailCall, other => Visit(node.Parameters, other.Parameters), other => Visit(node.Body, other.Body));

    protected override Expression VisitLoop(LoopExpression node)
      => VisitIfEqual(node, other => Equals(node.BreakLabel, other.BreakLabel) && Equals(node.ContinueLabel, other.ContinueLabel), other => Visit(node.Body, other.Body));

    protected override Expression VisitMember(MemberExpression node)
      => VisitIfEqual(node, other => memberEquals(node.Member, other.Member), other => Visit(node.Expression, other.Expression));

    protected override Expression VisitIndex(IndexExpression node)
      => VisitIfEqual(node, other => memberEquals(node.Indexer, other.Indexer), other => Visit(node.Object, other.Object), other => Visit(node.Arguments, other.Arguments));

    protected override Expression VisitMethodCall(MethodCallExpression node)
      => VisitIfEqual(node, other => memberEquals(node.Method, other.Method), other => Visit(node.Object, other.Object), other => Visit(node.Arguments, other.Arguments));

    protected override Expression VisitNewArray(NewArrayExpression node)
      => VisitChildren(node, other => Visit(node.Expressions, node.Expressions));

    protected override Expression VisitNew(NewExpression node)
      => VisitIfEqual(node, other => memberEquals(node.Constructor, other.Constructor) && ExpressionEqualityComparer.NullsOrEquals(node.Members, other.Members, (n, o) => n.Count == o.Count && n.Zip(o, memberEquals).All(b => b)), other => Visit(node.Arguments, other.Arguments));

    protected override Expression VisitParameter(ParameterExpression node)
      => VisitIfEqual(node, other => node.IsByRef == other.IsByRef && node.Name == other.Name);

    protected override Expression VisitRuntimeVariables(RuntimeVariablesExpression node)
      => VisitChildren(node, other => Visit(node.Variables, other.Variables));

    protected override SwitchCase VisitSwitchCase(SwitchCase node)
    {
      throw new InvalidOperationException("Bug: SwitchCase must not be visited since it's compared within the expressions that contain it.");
    }

    protected override Expression VisitSwitch(SwitchExpression node)
      => VisitIfEqual(node, other => memberEquals(node.Comparison, other.Comparison) && ExpressionEqualityComparer.NullsOrEquals(node.Cases, other.Cases, (n, o) => n.Count == o.Count),
                            other => Visit((node.Cases ?? Enumerable.Empty<SwitchCase>()).SelectMany(n => n.TestValues).ToList(), (other.Cases ?? Enumerable.Empty<SwitchCase>()).SelectMany(n => n.TestValues).ToList()),
                            other => Visit((node.Cases ?? Enumerable.Empty<SwitchCase>()).Select(n => n.Body).ToList(), (other.Cases ?? Enumerable.Empty<SwitchCase>()).Select(n => n.Body).ToList()),
                            other => Visit(node.DefaultBody, other.DefaultBody),
                            other => Visit(node.SwitchValue, other.SwitchValue));

    protected override CatchBlock VisitCatchBlock(CatchBlock node)
    {
      throw new InvalidOperationException("Bug: CatchBlock must not be visited since it's compared within the expressions that contain it.");
    }

    protected override Expression VisitTry(TryExpression node)
      => VisitIfEqual(node, other => ExpressionEqualityComparer.NullsOrEquals(node.Handlers, other.Handlers, (n, o) => n.Select(h => h.Test).SequenceEqual(o.Select(h => h.Test))),
                            other => Visit((node.Handlers ?? Enumerable.Empty<CatchBlock>()).Select(n => n.Body).ToList(), (other.Handlers ?? Enumerable.Empty<CatchBlock>()).Select(n => n.Body).ToList()),
                            other => Visit((node.Handlers ?? Enumerable.Empty<CatchBlock>()).Select(n => n.Filter).ToList(), (other.Handlers ?? Enumerable.Empty<CatchBlock>()).Select(n => n.Filter).ToList()),
                            other => Visit((node.Handlers ?? Enumerable.Empty<CatchBlock>()).Select(n => n.Variable).ToList(), (other.Handlers ?? Enumerable.Empty<CatchBlock>()).Select(n => n.Variable).ToList()),
                            other => Visit(node.Body, other.Body),
                            other => Visit(node.Fault, other.Fault),
                            other => Visit(node.Finally, other.Finally));

    protected override Expression VisitTypeBinary(TypeBinaryExpression node)
      => VisitIfEqual(node, other => typeEquals(node.TypeOperand, other.TypeOperand), other => Visit(node.Expression, other.Expression));

    protected override Expression VisitUnary(UnaryExpression node)
      => VisitIfEqual(node, other => node.IsLifted == other.IsLifted && node.IsLiftedToNull == other.IsLiftedToNull && memberEquals(node.Method, other.Method), other => Visit(node.Operand, other.Operand));

    protected override Expression VisitMemberInit(MemberInitExpression node)
      => VisitIfEqual(node, other => Equals(node.Bindings, other.Bindings),
                            other => Visit(GetExpressions(node.Bindings), GetExpressions(other.Bindings)),
                            other => Visit(node.NewExpression, other.NewExpression));

    protected override Expression VisitListInit(ListInitExpression node)
      => VisitIfEqual(node, other => ExpressionEqualityComparer.NullsOrEquals(node.Initializers, other.Initializers, (n, o) => n.Count == o.Count && n.Select(i => i.AddMethod).Zip(o.Select(i => i.AddMethod), memberEquals).All(b => b)),
                            other => Visit((node.Initializers ?? Enumerable.Empty<ElementInit>()).SelectMany(n => n.Arguments).ToList(), (other.Initializers ?? Enumerable.Empty<ElementInit>()).SelectMany(n => n.Arguments).ToList()),
                            other => Visit(node.NewExpression, other.NewExpression));

    protected override ElementInit VisitElementInit(ElementInit node)
    {
      throw new InvalidOperationException("Bug: ElementInit must not be visited since it's compared within the expressions that contain it.");
    }

    protected override MemberBinding VisitMemberBinding(MemberBinding node)
    {
      throw new InvalidOperationException("Bug: MemberBinding must not be visited since it's compared within the expressions that contain it.");
    }

    protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
    {
      throw new InvalidOperationException("Bug: MemberAssignment must not be visited since it's compared within the expressions that contain it.");
    }

    protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding node)
    {
      throw new InvalidOperationException("Bug: MemberMemberBinding must not be visited since it's compared within the expressions that contain it.");
    }

    protected override MemberListBinding VisitMemberListBinding(MemberListBinding node)
    {
      throw new InvalidOperationException("Bug: MemberListBinding must not be visited since it's compared within the expressions that contain it.");
    }

    private bool Equals(LabelTarget target, LabelTarget other)
      => ExpressionEqualityComparer.NullsOrEquals(target, other, (n, o) => n.Name == o.Name && n.Type == o.Type);

    private bool Equals(ICollection<MemberBinding> binding, ICollection<MemberBinding> other)
      => ExpressionEqualityComparer.NullsOrEquals(binding, other, (n, o) => n.Count == o.Count && n.Zip(o, Equals).All(b => b));

    private bool Equals(MemberBinding binding, MemberBinding other)
      => ExpressionEqualityComparer.NullsOrEquals(binding, other, (n, o) => n.BindingType == o.BindingType
                                                                         && n.Member == o.Member
                                                                         && (EqualsIfType<MemberMemberBinding>(n, o, Equals)
                                                                           ?? EqualsIfType<MemberListBinding>(n, o, Equals)
                                                                           ?? false));

    private bool Equals(MemberMemberBinding binding, MemberMemberBinding other)
      => Equals(binding.Bindings, other.Bindings);

    private bool Equals(MemberListBinding binding, MemberListBinding other)
      => ExpressionEqualityComparer.NullsOrEquals(binding.Initializers, other.Initializers, (n, o) => n.Select(i => i.AddMethod).SequenceEqual(o.Select(i => i.AddMethod)));

    private bool? EqualsIfType<T>(object first, object second, Func<T, T, bool> comparer)
      where T : class
    {
      var x = first as T;
      var y = second as T;

      return x != null && y != null ? comparer(x, y) : (bool?)null;
    }

    private ICollection<Expression> GetExpressions(ReadOnlyCollection<MemberBinding> bindings)
      => bindings.OfType<MemberAssignment>().Select(assignment => assignment.Expression)
                 .Concat(
         bindings.OfType<MemberListBinding>().SelectMany(binding => binding.Initializers).SelectMany(init => init.Arguments))
                 .ToList();
  }
}

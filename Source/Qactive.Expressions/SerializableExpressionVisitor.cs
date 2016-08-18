using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Qactive.Expressions
{
  internal abstract class SerializableExpressionVisitor
  {
    public void Visit(SerializableExpression node)
    {
      if (node != null)
      {
        node.Accept(this);
      }
    }

    protected void VisitMemberBinding(Tuple<Tuple<MemberInfo, Type[]>, MemberBindingType, SerializableExpression, List<Tuple<Tuple<MethodInfo, Type[]>, IList<SerializableExpression>>>, IList<object>> node)
    {
      switch (node.Item2)
      {
        case MemberBindingType.Assignment:
          VisitMemberAssignment(Tuple.Create(node.Item1, node.Item3));
          break;
        case MemberBindingType.MemberBinding:
          VisitMemberMemberBinding(Tuple.Create(node.Item1, node.Item5));
          break;
        case MemberBindingType.ListBinding:
          VisitMemberListBinding(Tuple.Create(node.Item1, node.Item4));
          break;
        default:
          throw new ArgumentException();
      }
    }

    protected internal abstract void VisitBinary(SerializableBinaryExpression node);

    protected internal abstract void VisitParameter(SerializableParameterExpression node);

    protected internal abstract void VisitLambda(SerializableLambdaExpression node);

    protected internal abstract void VisitListInit(SerializableListInitExpression node);

    protected internal abstract void VisitConditional(SerializableConditionalExpression node);

    protected internal abstract void VisitConstant(SerializableConstantExpression node);

    protected internal abstract void VisitRuntimeVariables(SerializableRuntimeVariablesExpression node);

    protected internal abstract void VisitMember(SerializableMemberExpression node);

    protected internal abstract void VisitMemberInit(SerializableMemberInitExpression node);

    protected internal abstract void VisitMemberAssignment(Tuple<Tuple<MemberInfo, Type[]>, SerializableExpression> assignment);

    protected internal abstract void VisitMemberListBinding(Tuple<Tuple<MemberInfo, Type[]>, List<Tuple<Tuple<MethodInfo, Type[]>, IList<SerializableExpression>>>> binding);

    protected internal abstract void VisitMemberMemberBinding(Tuple<Tuple<MemberInfo, Type[]>, IList<object>> binding);

    protected internal abstract void VisitElementInit(Tuple<Tuple<MethodInfo, Type[]>, IList<SerializableExpression>> initializer);

    protected internal abstract void VisitInvocation(SerializableInvocationExpression node);

    protected internal abstract void VisitMethodCall(SerializableMethodCallExpression node);

    protected internal abstract void VisitNewArray(SerializableNewArrayExpression node);

    protected internal abstract void VisitNew(SerializableNewExpression node);

    protected internal abstract void VisitTypeBinary(SerializableTypeBinaryExpression node);

    protected internal abstract void VisitUnary(SerializableUnaryExpression node);

    protected internal abstract void VisitBlock(SerializableBlockExpression node);

    protected internal abstract void VisitDefault(SerializableDefaultExpression node);

    protected internal abstract void VisitLabel(SerializableLabelExpression node);

    protected internal abstract void VisitGoto(SerializableGotoExpression node);

    protected internal abstract void VisitLoop(SerializableLoopExpression node);

    protected internal abstract void VisitSwitchCase(Tuple<SerializableExpression, IList<SerializableExpression>> node);

    protected internal abstract void VisitSwitch(SerializableSwitchExpression node);

    protected internal abstract void VisitCatchBlock(Tuple<SerializableExpression, SerializableExpression, Type, SerializableParameterExpression> node);

    protected internal abstract void VisitTry(SerializableTryExpression node);

    protected internal abstract void VisitIndex(SerializableIndexExpression node);

    protected internal abstract void VisitExtension(SerializableExpression node);
  }
}

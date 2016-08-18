/* Retrieved from 
 * https://github.com/dotnet/corefx/blob/master/src/System.Linq.Expressions/src/System/Linq/Expressions/ExpressionStringBuilder.cs
 * on Aug 15, 2016
 * and changed to support SerializableExpression instead of Expression.
 * 
 * License retrieved from
 * https://github.com/dotnet/corefx/blob/master/LICENSE
 * on Aug 15, 2016
 * 
The MIT License (MIT)

Copyright (c) .NET Foundation and Contributors

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Qactive.Expressions
{
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
  internal sealed class SerializableExpressionStringBuilder : SerializableExpressionVisitor
  {
    private StringBuilder _out;

    // Associate every unique label or anonymous parameter in the tree with an integer.
    // The label is displayed as Label_#.
    private Dictionary<object, int> _ids;

    private SerializableExpressionStringBuilder()
    {
      _out = new StringBuilder();
    }

    public override string ToString()
    {
      return _out.ToString();
    }

    private void AddLabel(string targetName, Type targetType)
    {
      var label = Tuple.Create(targetName, targetType);

      if (_ids == null)
      {
        _ids = new Dictionary<object, int>();
        _ids.Add(label, 0);
      }
      else
      {
        if (!_ids.ContainsKey(label))
        {
          _ids.Add(label, _ids.Count);
        }
      }
    }

    private int GetLabelId(string targetName, Type targetType)
    {
      if (_ids == null)
      {
        _ids = new Dictionary<object, int>();
        AddLabel(targetName, targetType);
        return 0;
      }
      else
      {
        int id;
        if (!_ids.TryGetValue(Tuple.Create(targetName, targetType), out id))
        {
          //label is met the first time
          id = _ids.Count;
          AddLabel(targetName, targetType);
        }
        return id;
      }
    }

    private void AddParam(SerializableParameterExpression p)
    {
      if (_ids == null)
      {
        _ids = new Dictionary<object, int>();
        _ids.Add(_ids, 0);
      }
      else
      {
        if (!_ids.ContainsKey(p))
        {
          _ids.Add(p, _ids.Count);
        }
      }
    }

    private int GetParamId(SerializableParameterExpression p)
    {
      if (_ids == null)
      {
        _ids = new Dictionary<object, int>();
        AddParam(p);
        return 0;
      }
      else
      {
        int id;
        if (!_ids.TryGetValue(p, out id))
        {
          // p is met the first time
          id = _ids.Count;
          AddParam(p);
        }
        return id;
      }
    }

    #region The printing code

    private void Out(string s)
    {
      _out.Append(s);
    }

    private void Out(char c)
    {
      _out.Append(c);
    }

    #endregion

    #region Output an expression tree to a string

    /// <summary>
    /// Output a given expression tree to a string.
    /// </summary>
    internal static string ExpressionToString(SerializableExpression node)
    {
      Debug.Assert(node != null);
      SerializableExpressionStringBuilder esb = new SerializableExpressionStringBuilder();
      esb.Visit(node);
      return esb.ToString();
    }

    internal static string CatchBlockToString(Tuple<SerializableExpression, SerializableExpression, Type, SerializableParameterExpression> node)
    {
      Debug.Assert(node != null);
      SerializableExpressionStringBuilder esb = new SerializableExpressionStringBuilder();
      esb.VisitCatchBlock(node);
      return esb.ToString();
    }

    internal static string SwitchCaseToString(Tuple<SerializableExpression, IList<SerializableExpression>> node)
    {
      Debug.Assert(node != null);
      SerializableExpressionStringBuilder esb = new SerializableExpressionStringBuilder();
      esb.VisitSwitchCase(node);
      return esb.ToString();
    }

    /// <summary>
    /// Output a given member binding to a string.
    /// </summary>
    internal static string MemberBindingToString(Tuple<Tuple<MemberInfo, Type[]>, MemberBindingType, SerializableExpression, List<Tuple<Tuple<MethodInfo, Type[]>, IList<SerializableExpression>>>, IList<object>> node)
    {
      Debug.Assert(node != null);
      SerializableExpressionStringBuilder esb = new SerializableExpressionStringBuilder();
      esb.VisitMemberBinding(node);
      return esb.ToString();
    }

    /// <summary>
    /// Output a given ElementInit to a string.
    /// </summary>
    internal static string ElementInitBindingToString(Tuple<Tuple<MethodInfo, Type[]>, IList<SerializableExpression>> node)
    {
      Debug.Assert(node != null);
      SerializableExpressionStringBuilder esb = new SerializableExpressionStringBuilder();
      esb.VisitElementInit(node);
      return esb.ToString();
    }

    private void VisitExpressions<T>(char open, IList<T> expressions, char close)
      where T : SerializableExpression
    {
      VisitExpressions(open, expressions, close, ", ");
    }

    private void VisitExpressions<T>(char open, IList<T> expressions, char close, string seperator)
      where T : SerializableExpression
    {
      Out(open);
      if (expressions != null)
      {
        bool isFirst = true;
        foreach (T e in expressions)
        {
          if (isFirst)
          {
            isFirst = false;
          }
          else
          {
            Out(seperator);
          }
          Visit(e);
        }
      }
      Out(close);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    protected internal override void VisitBinary(SerializableBinaryExpression node)
    {
      if (node.NodeType == ExpressionType.ArrayIndex)
      {
        Visit(node.Left);
        Out("[");
        Visit(node.Right);
        Out("]");
      }
      else
      {
        string op;
        switch (node.NodeType)
        {
          // AndAlso and OrElse were unintentionally changed in
          // CLR 4. We changed them to "AndAlso" and "OrElse" to
          // be 3.5 compatible, but it turns out 3.5 shipped with
          // "&&" and "||". Oops.
          case ExpressionType.AndAlso:
            op = "AndAlso";
            break;
          case ExpressionType.OrElse:
            op = "OrElse";
            break;
          case ExpressionType.Assign: op = "="; break;
          case ExpressionType.Equal:
            op = "==";
            break;
          case ExpressionType.NotEqual: op = "!="; break;
          case ExpressionType.GreaterThan: op = ">"; break;
          case ExpressionType.LessThan: op = "<"; break;
          case ExpressionType.GreaterThanOrEqual: op = ">="; break;
          case ExpressionType.LessThanOrEqual: op = "<="; break;
          case ExpressionType.Add: op = "+"; break;
          case ExpressionType.AddAssign: op = "+="; break;
          case ExpressionType.AddAssignChecked: op = "+="; break;
          case ExpressionType.AddChecked: op = "+"; break;
          case ExpressionType.Subtract: op = "-"; break;
          case ExpressionType.SubtractAssign: op = "-="; break;
          case ExpressionType.SubtractAssignChecked: op = "-="; break;
          case ExpressionType.SubtractChecked: op = "-"; break;
          case ExpressionType.Divide: op = "/"; break;
          case ExpressionType.DivideAssign: op = "/="; break;
          case ExpressionType.Modulo: op = "%"; break;
          case ExpressionType.ModuloAssign: op = "%="; break;
          case ExpressionType.Multiply: op = "*"; break;
          case ExpressionType.MultiplyAssign: op = "*="; break;
          case ExpressionType.MultiplyAssignChecked: op = "*="; break;
          case ExpressionType.MultiplyChecked: op = "*"; break;
          case ExpressionType.LeftShift: op = "<<"; break;
          case ExpressionType.LeftShiftAssign: op = "<<="; break;
          case ExpressionType.RightShift: op = ">>"; break;
          case ExpressionType.RightShiftAssign: op = ">>="; break;
          case ExpressionType.And:
            if (node.Type == typeof(bool) || node.Type == typeof(bool?))
            {
              op = "And";
            }
            else
            {
              op = "&";
            }
            break;
          case ExpressionType.AndAssign:
            if (node.Type == typeof(bool) || node.Type == typeof(bool?))
            {
              op = "&&=";
            }
            else
            {
              op = "&=";
            }
            break;
          case ExpressionType.Or:
            if (node.Type == typeof(bool) || node.Type == typeof(bool?))
            {
              op = "Or";
            }
            else
            {
              op = "|";
            }
            break;
          case ExpressionType.OrAssign:
            if (node.Type == typeof(bool) || node.Type == typeof(bool?))
            {
              op = "||=";
            }
            else { op = "|="; }
            break;
          case ExpressionType.ExclusiveOr: op = "^"; break;
          case ExpressionType.ExclusiveOrAssign: op = "^="; break;
          case ExpressionType.Power: op = "^"; break;
          case ExpressionType.PowerAssign: op = "**="; break;
          case ExpressionType.Coalesce: op = "??"; break;

          default:
            throw new InvalidOperationException();
        }
        Out("(");
        Visit(node.Left);
        Out(' ');
        Out(op);
        Out(' ');
        Visit(node.Right);
        Out(")");
      }
    }

    protected internal override void VisitParameter(SerializableParameterExpression node)
    {
      if (node.IsByRef)
      {
        Out("ref ");
      }
      string name = node.Name;
      if (String.IsNullOrEmpty(name))
      {
        Out("Param_" + GetParamId(node));
      }
      else
      {
        Out(name);
      }
    }

    protected internal override void VisitLambda(SerializableLambdaExpression node)
    {
      if (node.Parameters.Count == 1)
      {
        // p => body
        Visit(node.Parameters[0]);
      }
      else
      {
        // (p1, p2, ..., pn) => body
        VisitExpressions('(', node.Parameters, ')');
      }
      Out(" => ");
      Visit(node.Body);
    }

    protected internal override void VisitListInit(SerializableListInitExpression node)
    {
      Visit(node.NewExpression);
      Out(" {");
      for (int i = 0, n = node.Initializers.Count; i < n; i++)
      {
        if (i > 0)
        {
          Out(", ");
        }
        Out(node.Initializers[i].ToString());
      }
      Out("}");
    }

    protected internal override void VisitConditional(SerializableConditionalExpression node)
    {
      Out("IIF(");
      Visit(node.Test);
      Out(", ");
      Visit(node.IfTrue);
      Out(", ");
      Visit(node.IfFalse);
      Out(")");
    }

    protected internal override void VisitConstant(SerializableConstantExpression node)
    {
      if (node.Value != null)
      {
        string sValue = node.Value.ToString();
        if (node.Value is string)
        {
          Out("\"");
          Out(sValue);
          Out("\"");
        }
        else if (sValue == node.Value.GetType().ToString())
        {
          Out("value(");
          Out(sValue);
          Out(")");
        }
        else
        {
          Out(sValue);
        }
      }
      else
      {
        Out("null");
      }
    }

    //protected internal override void VisitDebugInfo(SerializableDebugInfoExpression node)
    //{
    //  string s = String.Format(
    //      CultureInfo.CurrentCulture,
    //      "<DebugInfo({0}: {1}, {2}, {3}, {4})>",
    //      node.Document.FileName,
    //      node.StartLine,
    //      node.StartColumn,
    //      node.EndLine,
    //      node.EndColumn
    //  );
    //  Out(s);
    //}

    protected internal override void VisitRuntimeVariables(SerializableRuntimeVariablesExpression node)
    {
      VisitExpressions('(', node.Variables, ')');
    }

    // Prints ".instanceField" or "declaringType.staticField"
    private void OutMember(SerializableExpression instance, Tuple<MemberInfo, Type[]> member)
    {
      if (instance != null)
      {
        Visit(instance);
        Out("." + member.Item1.Name);
      }
      else
      {
        // For static members, include the type name
        Out(member.Item1.DeclaringType.Name + "." + member.Item1.Name);
      }

      // TODO: Include the parameters? (member.Item2)
    }

    protected internal override void VisitMember(SerializableMemberExpression node)
    {
      OutMember(node.Expr, node.Member);
    }

    protected internal override void VisitMemberInit(SerializableMemberInitExpression node)
    {
      if (node.NewExpression.Arguments.Count == 0 &&
          node.NewExpression.Type.Name.Contains("<"))
      {
        // anonymous type constructor
        Out("new");
      }
      else
      {
        Visit(node.NewExpression);
      }
      Out(" {");
      for (int i = 0, n = node.Bindings.Count; i < n; i++)
      {
        Tuple<Tuple<MemberInfo, Type[]>, MemberBindingType, SerializableExpression, List<Tuple<Tuple<MethodInfo, Type[]>, IList<SerializableExpression>>>, IList<object>> b = node.Bindings[i];
        if (i > 0)
        {
          Out(", ");
        }
        VisitMemberBinding(b);
      }
      Out("}");
    }

    protected internal override void VisitMemberAssignment(Tuple<Tuple<MemberInfo, Type[]>, SerializableExpression> assignment)
    {
      Out(assignment.Item1.Item1.Name);
      Out(" = ");
      Visit(assignment.Item2);
    }

    protected internal override void VisitMemberListBinding(Tuple<Tuple<MemberInfo, Type[]>, List<Tuple<Tuple<MethodInfo, Type[]>, IList<SerializableExpression>>>> binding)
    {
      Out(binding.Item1.Item1.Name);
      Out(" = {");
      for (int i = 0, n = binding.Item2.Count; i < n; i++)
      {
        if (i > 0)
        {
          Out(", ");
        }
        VisitElementInit(binding.Item2[i]);
      }
      Out("}");
    }

    protected internal override void VisitMemberMemberBinding(Tuple<Tuple<MemberInfo, Type[]>, IList<object>> binding)
    {
      Out(binding.Item1.Item1.Name);
      Out(" = {");
      for (int i = 0, n = binding.Item2.Count; i < n; i++)
      {
        if (i > 0)
        {
          Out(", ");
        }
        VisitMemberBinding((Tuple<Tuple<MemberInfo, Type[]>, MemberBindingType, SerializableExpression, List<Tuple<Tuple<MethodInfo, Type[]>, IList<SerializableExpression>>>, IList<object>>)binding.Item2[i]);
      }
      Out("}");
    }

    protected internal override void VisitElementInit(Tuple<Tuple<MethodInfo, Type[]>, IList<SerializableExpression>> initializer)
    {
      Out(initializer.Item1.Item1.ToString());
      string sep = ", ";
      VisitExpressions('(', initializer.Item2, ')', sep);
    }

    protected internal override void VisitInvocation(SerializableInvocationExpression node)
    {
      Out("Invoke(");
      Visit(node.Expr);
      string sep = ", ";
      for (int i = 0, n = node.Arguments.Count; i < n; i++)
      {
        Out(sep);
        Visit(node.Arguments[i]);
      }
      Out(")");
    }

    protected internal override void VisitMethodCall(SerializableMethodCallExpression node)
    {
      int start = 0;
      SerializableExpression ob = node.Object;

      if (node.Method.Item1.GetCustomAttributes(typeof(ExtensionAttribute), inherit: true).Any())
      {
        start = 1;
        ob = node.Arguments[0];
      }

      if (ob != null)
      {
        Visit(ob);
        Out(".");
      }
      Out(node.Method.Item1.Name);
      Out("(");
      for (int i = start, n = node.Arguments.Count; i < n; i++)
      {
        if (i > start)
          Out(", ");
        Visit(node.Arguments[i]);
      }
      Out(")");
    }

    protected internal override void VisitNewArray(SerializableNewArrayExpression node)
    {
      switch (node.NodeType)
      {
        case ExpressionType.NewArrayBounds:
          // new MyType[](expr1, expr2)
          Out("new " + node.Type.ToString());
          VisitExpressions('(', node.Expressions, ')');
          break;
        case ExpressionType.NewArrayInit:
          // new [] {expr1, expr2}
          Out("new [] ");
          VisitExpressions('{', node.Expressions, '}');
          break;
      }
    }

    protected internal override void VisitNew(SerializableNewExpression node)
    {
      Out("new " + node.Type.Name);
      Out("(");
      var members = node.Members;
      for (int i = 0; i < node.Arguments.Count; i++)
      {
        if (i > 0)
        {
          Out(", ");
        }
        if (members != null)
        {
          string name = members[i].Item1.Name;
          Out(name);
          Out(" = ");
        }
        Visit(node.Arguments[i]);
      }
      Out(")");
    }

    protected internal override void VisitTypeBinary(SerializableTypeBinaryExpression node)
    {
      Out("(");
      Visit(node.Expr);
      switch (node.NodeType)
      {
        case ExpressionType.TypeIs:
          Out(" Is ");
          break;
        case ExpressionType.TypeEqual:
          Out(" TypeEqual ");
          break;
      }
      Out(node.TypeOperand.Name);
      Out(")");
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    protected internal override void VisitUnary(SerializableUnaryExpression node)
    {
      switch (node.NodeType)
      {
        case ExpressionType.TypeAs:
          Out("(");
          break;
        case ExpressionType.Not:
          Out("Not(");
          break;
        case ExpressionType.Negate:
        case ExpressionType.NegateChecked:
          Out("-");
          break;
        case ExpressionType.UnaryPlus:
          Out("+");
          break;
        case ExpressionType.Quote:
          break;
        case ExpressionType.Throw:
          Out("throw(");
          break;
        case ExpressionType.Increment:
          Out("Increment(");
          break;
        case ExpressionType.Decrement:
          Out("Decrement(");
          break;
        case ExpressionType.PreIncrementAssign:
          Out("++");
          break;
        case ExpressionType.PreDecrementAssign:
          Out("--");
          break;
        case ExpressionType.OnesComplement:
          Out("~(");
          break;
        default:
          Out(node.NodeType.ToString());
          Out("(");
          break;
      }

      Visit(node.Operand);

      switch (node.NodeType)
      {
        case ExpressionType.Negate:
        case ExpressionType.NegateChecked:
        case ExpressionType.UnaryPlus:
        case ExpressionType.PreDecrementAssign:
        case ExpressionType.PreIncrementAssign:
        case ExpressionType.Quote:
          break;
        case ExpressionType.TypeAs:
          Out(" As ");
          Out(node.Type.Name);
          Out(")");
          break;
        case ExpressionType.PostIncrementAssign:
          Out("++");
          break;
        case ExpressionType.PostDecrementAssign:
          Out("--");
          break;
        default:
          Out(")");
          break;
      }
    }

    protected internal override void VisitBlock(SerializableBlockExpression node)
    {
      Out("{");
      foreach (var v in node.Variables)
      {
        Out("var ");
        Visit(v);
        Out(";");
      }
      Out(" ... }");
    }

    protected internal override void VisitDefault(SerializableDefaultExpression node)
    {
      Out("default(");
      Out(node.Type.Name);
      Out(")");
    }

    protected internal override void VisitLabel(SerializableLabelExpression node)
    {
      Out("{ ... } ");
      DumpLabel(node.TargetName, node.TargetType);
      Out(":");
    }

    protected internal override void VisitGoto(SerializableGotoExpression node)
    {
      Out(node.Kind.ToString().ToLower());
      DumpLabel(node.TargetName, node.TargetType);
      if (node.Value != null)
      {
        Out(" (");
        Visit(node.Value);
        Out(") ");
      }
    }

    protected internal override void VisitLoop(SerializableLoopExpression node)
    {
      Out("loop { ... }");
    }

    protected internal override void VisitSwitchCase(Tuple<SerializableExpression, IList<SerializableExpression>> node)
    {
      Out("case ");
      VisitExpressions('(', node.Item2, ')');
      Out(": ...");
    }

    protected internal override void VisitSwitch(SerializableSwitchExpression node)
    {
      Out("switch ");
      Out("(");
      Visit(node.SwitchValue);
      Out(") { ... }");
    }

    protected internal override void VisitCatchBlock(Tuple<SerializableExpression, SerializableExpression, Type, SerializableParameterExpression> node)
    {
      Out("catch (" + node.Item3.Name);
      if (node.Item4 != null && !string.IsNullOrEmpty(node.Item4.Name))
      {
        Out(' ');
        Out(node.Item4.Name);
      }
      Out(") { ... }");
    }

    protected internal override void VisitTry(SerializableTryExpression node)
    {
      Out("try { ... }");
    }

    protected internal override void VisitIndex(SerializableIndexExpression node)
    {
      if (node.Object != null)
      {
        Visit(node.Object);
      }
      else
      {
        Debug.Assert(node.Indexer != null);
        Out(node.Indexer.DeclaringType.Name);
      }
      if (node.Indexer != null)
      {
        Out(".");
        Out(node.Indexer.Name);
      }

      VisitExpressions('[', node.Arguments, ']');
    }

    protected internal override void VisitExtension(SerializableExpression node)
    {
      // Prefer an overridden ToString, if available.
      var toString = node.GetType().GetMethod("ToString", Type.EmptyTypes);
      if (toString.DeclaringType != typeof(SerializableExpression) && !toString.IsStatic)
      {
        Out(node.ToString());
        return;
      }

      Out("[");
      // For 3.5 subclasses, print the NodeType.
      // For Extension nodes, print the class name.
      if (node.NodeType == ExpressionType.Extension)
      {
        Out(node.GetType().FullName);
      }
      else
      {
        Out(node.NodeType.ToString());
      }
      Out("]");
    }

    private void DumpLabel(string targetName, Type targetType)
    {
      if (!String.IsNullOrEmpty(targetName))
      {
        Out(targetName);
      }
      else
      {
        int labelId = GetLabelId(targetName, targetType);
        Out("UnamedLabel_" + labelId);
      }
    }
    #endregion
  }
}

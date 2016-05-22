using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Qactive.Properties;

namespace Qactive
{
  public class DuplexLocalEvaluator : LocalEvaluator
  {
    public DuplexLocalEvaluator(params Type[] knownTypes)
      : base(knownTypes)
    {
    }

    public override Expression GetValue(PropertyInfo property, MemberExpression member, ExpressionVisitor visitor, IQbservableProtocol protocol)
      => DuplexCallback.Create(
          protocol,
          Evaluate(member.Expression, visitor, _ => Errors.ExpressionMemberMissingLocalInstanceFormat, member.Member),
          property);

    public override Expression GetValue(FieldInfo field, MemberExpression member, ExpressionVisitor visitor, IQbservableProtocol protocol)
      => DuplexCallback.Create(
          protocol,
          Evaluate(member.Expression, visitor, _ => Errors.ExpressionMemberMissingLocalInstanceFormat, member.Member),
          field);

    public override Expression Invoke(MethodCallExpression call, ExpressionVisitor visitor, IQbservableProtocol protocol)
    {
      object instance;

      if (call.Method.ReturnType == typeof(void))
      {
        instance = null;
      }
      else
      {
        instance = Evaluate(call.Object, visitor, _ => Errors.ExpressionCallMissingLocalInstanceFormat, call.Method);
      }

      return DuplexCallback.Create(protocol, instance, call.Method, visitor.Visit(call.Arguments));
    }

    internal static object Evaluate(Expression expression, ExpressionVisitor visitor, Func<Expression, string> errorMessageFormatSelector, MemberInfo method)
    {
      Contract.Requires(visitor != null);
      Contract.Requires(errorMessageFormatSelector != null);
      Contract.Requires(method != null);

      if (expression == null)
      {
        return null;
      }

      expression = visitor.Visit(expression);

      var constant = expression as ConstantExpression;

      if (constant == null)
      {
        throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, errorMessageFormatSelector(expression), method.Name, method.DeclaringType));
      }

      return constant.Value;
    }

    protected override Either<object, Expression> TryEvaluateEnumerable(object value, Type type, IQbservableProtocol protocol)
    {
      Expression expression = null;

      if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
      {
        expression = DuplexCallback.CreateEnumerable(protocol, value, type.GetGenericArguments()[0], type);
      }
      else if (type == typeof(IEnumerable))
      {
        var enumerable = (IEnumerable)value;

        expression = DuplexCallback.CreateEnumerable(protocol, enumerable.Cast<object>(), typeof(object), type);
      }

      return expression == null ? null : Either.Right<object, Expression>(expression);
    }

    protected override Expression TryEvaluateObservable(object value, Type type, IQbservableProtocol protocol)
    {
      var observableType = value.GetType().GetGenericInterfaceFromDefinition(typeof(IObservable<>));

      if (observableType != null)
      {
        return DuplexCallback.CreateObservable(protocol, value, observableType.GetGenericArguments()[0], type);
      }

      return null;
    }
  }
}
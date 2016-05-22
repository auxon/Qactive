using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Qactive.Properties;

namespace Qactive
{
  public class ImmediateLocalEvaluator : DuplexLocalEvaluator
  {
    public ImmediateLocalEvaluator(params Type[] knownTypes)
      : base(knownTypes)
    {
    }

    public override Expression GetValue(PropertyInfo property, MemberExpression member, ExpressionVisitor visitor, IQbservableProtocol protocol)
    {
      object instance = Evaluate(member.Expression, visitor, _ => Errors.ExpressionMemberMissingLocalInstanceFormat, member.Member);

      var value = property.GetValue(instance);

      var either = TryEvaluateSequences(value, property.PropertyType, protocol);

      return either == null
        ? Expression.Constant(value, property.PropertyType)
        : either.IsLeft
          ? Expression.Constant(either.Left, property.PropertyType)
          : either.Right;
    }

    public override Expression GetValue(FieldInfo field, MemberExpression member, ExpressionVisitor visitor, IQbservableProtocol protocol)
    {
      object instance = Evaluate(member.Expression, visitor, _ => Errors.ExpressionMemberMissingLocalInstanceFormat, member.Member);

      var value = field.GetValue(instance);

      var either = TryEvaluateSequences(value, field.FieldType, protocol);

      return either == null
        ? Expression.Constant(value, field.FieldType)
        : either.IsLeft
          ? Expression.Constant(either.Left, field.FieldType)
          : either.Right;
    }

    public override Expression Invoke(MethodCallExpression call, ExpressionVisitor visitor, IQbservableProtocol protocol)
    {
      if (call.Method.ReturnType == typeof(void))
      {
        throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Errors.ExpressionCallLocalVoidFormat, call.Method, call.Object));
      }

      object instance = Evaluate(call.Object, visitor, _ => Errors.ExpressionCallMissingLocalInstanceFormat, call.Method);

      var result = call.Method.Invoke(instance, EvaluateArguments(call, visitor).ToArray());

      var either = TryEvaluateSequences(result, call.Type, protocol);

      return either == null
          ? Expression.Constant(result, call.Type)
          : either.IsLeft
            ? Expression.Constant(either.Left, call.Type)
            : either.Right;
    }

    private static object[] EvaluateArguments(MethodCallExpression call, ExpressionVisitor visitor)
    {
      Contract.Requires(call != null);
      Contract.Requires(visitor != null);

      return call.Arguments
                ?.Select(e => Evaluate(e, visitor, exp => IsSourceInScope(exp) ? Errors.ExpressionCallOnUnknownTypeFormat : Errors.ExpressionCallMissingLocalArgumentFormat, call.Method))
                 .ToArray();
    }

    private static bool IsSourceInScope(Expression expression)
    {
      var isParameter = false;

      while (expression != null && !(isParameter = expression is ParameterExpression))
      {
        expression = (expression as MemberExpression)?.Expression;
      }

      return isParameter;
    }

    protected override Either<object, Expression> TryEvaluateEnumerable(object value, Type type, IQbservableProtocol protocol)
    {
      var iterator = value as IEnumerable;

      if (iterator != null)
      {
        var iteratorType = iterator.GetType();

        if (iteratorType.GetCustomAttribute<CompilerGeneratedAttribute>(true) != null
          || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
          || type == typeof(IEnumerable))
        {
          value = EvaluateIterator(iterator);

          return Either.Left<object, Expression>(value);
        }
      }

      return null;
    }

    private static object EvaluateIterator(IEnumerable iterator)
    {
      Contract.Requires(iterator != null);
      Contract.Ensures(Contract.Result<object>() != null);

      var genericIterator = iterator.GetType().GetGenericInterfaceFromDefinition(typeof(IEnumerable<>));

      var dataType = genericIterator == null ? typeof(object) : genericIterator.GetGenericArguments()[0];

      var list = Activator.CreateInstance(typeof(List<>).MakeGenericType(dataType));

      var add = list.GetType().GetMethod("Add");

      foreach (var item in iterator)
      {
        add.Invoke(list, new[] { item });
      }

      return list;
    }
  }
}
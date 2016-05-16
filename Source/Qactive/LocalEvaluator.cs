using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;

namespace Qactive
{
  [ContractClass(typeof(LocalEvaluatorContract))]
  public abstract class LocalEvaluator : LocalEvaluationContext
  {
    protected LocalEvaluator(params Type[] knownTypes)
      : base(knownTypes)
    {
    }

    public Expression EvaluateCompilerGenerated(MemberExpression member, IQbservableProtocol protocol)
    {
      Contract.Requires(member != null);
      Contract.Requires(protocol != null);

      var closure = member.Expression as ConstantExpression;

      if (closure == null)
      {
        return null;
      }

      var instance = closure.Value;

      object value;
      Type type;

      var field = member.Member as FieldInfo;

      if (field != null)
      {
        type = field.FieldType;
        value = field.GetValue(instance);
      }
      else
      {
        var property = (PropertyInfo)member.Member;

        type = property.PropertyType;
        value = property.GetValue(instance);
      }

      var result = TryEvaluateSequences(value, type, protocol);

      return result == null
           ? Expression.Constant(value, type)
           : result.IsLeft
             ? Expression.Constant(result.Left, type)
             : result.Right;
    }

    internal Expression GetValue(MemberExpression member, ExpressionVisitor visitor, IQbservableProtocol protocol)
    {
      Contract.Requires(member != null);
      Contract.Requires(visitor != null);
      Contract.Requires(protocol != null);
      Contract.Ensures(Contract.Result<Expression>() != null);

      var property = member.Member as PropertyInfo;

      if (property != null)
      {
        return GetValue(property, member, visitor, protocol);
      }
      else
      {
        return GetValue((FieldInfo)member.Member, member, visitor, protocol);
      }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Property", Justification = "Reviewed")]
    public abstract Expression GetValue(PropertyInfo property, MemberExpression member, ExpressionVisitor visitor, IQbservableProtocol protocol);

    public abstract Expression GetValue(FieldInfo field, MemberExpression member, ExpressionVisitor visitor, IQbservableProtocol protocol);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Call", Justification = "Reviewed")]
    public abstract Expression Invoke(MethodCallExpression call, ExpressionVisitor visitor, IQbservableProtocol protocol);

    protected Either<object, Expression> TryEvaluateSequences(object value, Type type, IQbservableProtocol protocol)
    {
      Contract.Requires(type != null);
      Contract.Requires(protocol != null);

      if (value != null)
      {
        var isSequence = type == typeof(IEnumerable)
                      || (type.IsGenericType
                         && (type.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                          || type.GetGenericTypeDefinition() == typeof(IObservable<>)));

        if (isSequence || !IsTypeKnown(value))
        {
          var result = TryEvaluateEnumerable(value, type, protocol);

          if (result != null)
          {
            return result;
          }
          else
          {
            var expression = TryEvaluateObservable(value, type, protocol);

            if (expression != null)
            {
              return Either.Right<object, Expression>(expression);
            }
          }
        }
      }

      return null;
    }

    protected abstract Either<object, Expression> TryEvaluateEnumerable(object value, Type type, IQbservableProtocol protocol);

    protected abstract Expression TryEvaluateObservable(object value, Type type, IQbservableProtocol protocol);
  }

  [ContractClassFor(typeof(LocalEvaluator))]
  internal abstract class LocalEvaluatorContract : LocalEvaluator
  {
    protected LocalEvaluatorContract()
      : base(null)
    {
    }

    public override Expression GetValue(PropertyInfo property, MemberExpression member, ExpressionVisitor visitor, IQbservableProtocol protocol)
    {
      Contract.Requires(property != null);
      Contract.Requires(member != null);
      Contract.Requires(visitor != null);
      Contract.Requires(protocol != null);
      Contract.Ensures(Contract.Result<Expression>() != null);
      return null;
    }

    public override Expression GetValue(FieldInfo field, MemberExpression member, ExpressionVisitor visitor, IQbservableProtocol protocol)
    {
      Contract.Requires(field != null);
      Contract.Requires(member != null);
      Contract.Requires(visitor != null);
      Contract.Requires(protocol != null);
      Contract.Ensures(Contract.Result<Expression>() != null);
      return null;
    }

    public override Expression Invoke(MethodCallExpression call, ExpressionVisitor visitor, IQbservableProtocol protocol)
    {
      Contract.Requires(call != null);
      Contract.Requires(visitor != null);
      Contract.Requires(protocol != null);
      Contract.Ensures(Contract.Result<Expression>() != null);
      return null;
    }

    protected override Either<object, Expression> TryEvaluateEnumerable(object value, Type type, IQbservableProtocol protocol)
    {
      Contract.Requires(value != null);
      Contract.Requires(type != null);
      Contract.Requires(protocol != null);
      return null;
    }

    protected override Expression TryEvaluateObservable(object value, Type type, IQbservableProtocol protocol)
    {
      Contract.Requires(value != null);
      Contract.Requires(type != null);
      Contract.Requires(protocol != null);
      return null;
    }
  }
}
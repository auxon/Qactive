using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Concurrency;
using System.Reflection;
using Qactive.Properties;

namespace Qactive
{
  public class DuplexLocalEvaluator : LocalEvaluator
  {
    private readonly IScheduler scheduler;

    /*
    In testing, the observer permanently blocked incoming data from the client unless concurrency was introduced for an 
    observable closure (client to server communication).

    The order of events were as follows: 
     
    1. The server received an OnNext notification from an I/O completion port.
    2. The server pushed the value to the observer passed into DuplexCallbackObservable.Subscribe, without introducing concurrency.
    3. The query provider continued executing the serialized query on the current thread.
    4. The query at this point required a synchronous invocation to a client-side member (i.e., duplex enabled).
    5. The server sent the new invocation to the client and then blocked the current thread waiting for an async response.
     
    Since the current thread was an I/O completion port (received for OnNext), it seems that blocking it prevented any 
    further data from being received, even via the Stream.AsyncRead method.  Apparently the only solution is to ensure 
    that observable callbacks occur on pooled threads to prevent I/O completion ports from inadvertantly being blocked.
    */
    public DuplexLocalEvaluator(params Type[] knownTypes)
      : this(TaskPoolScheduler.Default, knownTypes)
    {
    }

    public DuplexLocalEvaluator(IScheduler scheduler, params Type[] knownTypes)
      : base(knownTypes)
    {
      this.scheduler = scheduler;
    }

    public override Expression GetValue(PropertyInfo property, MemberExpression member, ExpressionVisitor visitor, IQbservableProtocol protocol)
    {
      object instance = Evaluate(member.Expression, visitor, Errors.ExpressionMemberMissingLocalInstanceFormat, member.Member);

      return DuplexCallback.Create(protocol, instance, property, scheduler);
    }

    public override Expression GetValue(FieldInfo field, MemberExpression member, ExpressionVisitor visitor, IQbservableProtocol protocol)
    {
      object instance = Evaluate(member.Expression, visitor, Errors.ExpressionMemberMissingLocalInstanceFormat, member.Member);

      return DuplexCallback.Create(protocol, instance, field, scheduler);
    }

    public override Expression Invoke(MethodCallExpression call, ExpressionVisitor visitor, IQbservableProtocol protocol)
    {
      object instance;

      if (call.Method.ReturnType == typeof(void))
      {
        instance = null;
      }
      else
      {
        instance = Evaluate(call.Object, visitor, Errors.ExpressionCallMissingLocalInstanceFormat, call.Method);
      }

      return DuplexCallback.Create(protocol, instance, call.Method, visitor.Visit(call.Arguments), scheduler);
    }

    internal static object Evaluate(Expression expression, ExpressionVisitor visitor, string errorMessageFormat, MemberInfo method)
    {
      if (expression == null)
      {
        return null;
      }

      expression = visitor.Visit(expression);

      var constant = expression as ConstantExpression;

      if (constant == null)
      {
        throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, errorMessageFormat, method.Name, method.DeclaringType));
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
        return DuplexCallback.CreateObservable(protocol, value, observableType.GetGenericArguments()[0], type, scheduler);
      }

      return null;
    }
  }
}
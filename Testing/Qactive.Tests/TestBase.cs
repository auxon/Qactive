using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive;
using Microsoft.Reactive.Testing;

namespace Qactive.Tests
{
  public abstract class TestBase : ReactiveTest
  {
    public static Notification<T> OnCompleted<T>()
      => Notification.CreateOnCompleted<T>();

    public static Notification<T> OnCompleted<T>(T witness)
      => Notification.CreateOnCompleted<T>();

    public static Notification<T> OnError<T>(Exception exception)
      => Notification.CreateOnError<T>(exception);

    public static Notification<T> OnError<T>(Exception exception, T witness)
      => Notification.CreateOnError<T>(exception);

    public static Notification<T> OnNext<T>(T value)
      => Notification.CreateOnNext(value);

    public static MethodCallExpression Call(Type declaringType, string methodName, params Expression[] arguments)
      => Call(declaringType, new Type[0], methodName, arguments);

    public static MethodCallExpression Call<T0>(Type declaringType, string methodName, params Expression[] arguments)
      => Call(declaringType, new[] { typeof(T0) }, methodName, arguments);

    public static MethodCallExpression Call<T0, T1>(Type declaringType, string methodName, params Expression[] arguments)
      => Call(declaringType, new[] { typeof(T0), typeof(T1) }, methodName, arguments);

    public static MethodCallExpression Call<T0, T1, T2>(Type declaringType, string methodName, params Expression[] arguments)
      => Call(declaringType, new[] { typeof(T0), typeof(T1), typeof(T2) }, methodName, arguments);

    public static MethodCallExpression Call<T0, T1, T2, T3>(Type declaringType, string methodName, params Expression[] arguments)
      => Call(declaringType, new[] { typeof(T0), typeof(T1), typeof(T2), typeof(T3) }, methodName, arguments);

    public static MethodCallExpression Call(Type declaringType, Type[] typeArguments, string methodName, params Expression[] arguments)
      => typeArguments != null && typeArguments.Length > 0
       ? Expression.Call(declaringType.GetMethods().First(m => m.Name == methodName).MakeGenericMethod(typeArguments), arguments)
       : Expression.Call(declaringType.GetMethods().First(m => m.Name == methodName), arguments);
  }
}

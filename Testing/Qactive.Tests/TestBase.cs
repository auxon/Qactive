using System;
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
  }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

    public static void AssertEqual<T>(IEnumerable<Notification<T>> actual, IEnumerable<Notification<T>> expected)
    {
      if (expected.All(e => e.Kind != NotificationKind.OnError))
      {
        AssertNoOnError(actual);
      }

      actual.AssertEqual(expected);
    }

    public static void AssertEqual<T>(IEnumerable<Notification<T>> actual, params Notification<T>[] expected)
      => AssertEqual(actual, (IEnumerable<Notification<T>>)expected);

    public static void AssertNoOnError<T>(IEnumerable<Notification<T>> results)
    {
      var error = TryGetError(results);

      if (error != null)
      {
        Assert.Fail(error.ToString());
      }
    }

    public static Exception TryGetError<T>(IEnumerable<Notification<T>> results)
    {
      return (from result in results
              where result.Kind == NotificationKind.OnError
              select result.Exception)
              .SingleOrDefault();
    }
  }
}
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
      var expectedErrors = expected.Where(n => n.Kind == NotificationKind.OnError).Select((n, index) => new { Error = n.Exception, index }).ToList();

      if (expectedErrors.Count == 0)
      {
        AssertNoOnError(actual);
      }
      else
      {
        var actualErrors = actual.Where(n => n.Kind == NotificationKind.OnError).Select((n, index) => new { Error = n.Exception, index }).ToList();

        Func<string> buildMessageDetails = () =>
            "Actual Errors: " + Environment.NewLine + string.Join(Environment.NewLine, actualErrors.Select(x => x.index + ": " + x.Error.Message)) + Environment.NewLine
          + "Expected Errors: " + Environment.NewLine + string.Join(Environment.NewLine, expectedErrors.Select(x => x.index + ": " + x.Error.Message));

        if (actualErrors.Count != expectedErrors.Count)
        {
          Assert.Fail($"Actual error count ({actualErrors.Count}) does not equal the expected error count ({expectedErrors.Count})." + Environment.NewLine + buildMessageDetails());
        }
        else
        {
          var pairs = actualErrors.Zip(expectedErrors, (x, y) => new { Actual = x, Expected = y });

          if (!pairs.All(pair => pair.Actual.index == pair.Expected.index))
          {
            Assert.Fail($"Actual errors are not in the same positions in the sequence as the expected errors." + Environment.NewLine + buildMessageDetails());
          }
          else if (!pairs.All(pair => pair.Expected.Error.GetType().IsAssignableFrom(pair.Actual.Error.GetType()) && pair.Expected.Error.Message.Equals(pair.Actual.Error.Message)))
          {
            Assert.Fail($"Actual errors are not equal to the expected errors." + Environment.NewLine + buildMessageDetails());
          }
          else
          {
            var list = actual as IList<Notification<T>>;

            if (list == null || list.IsReadOnly)
            {
              list = actual.ToList();
              actual = list;
            }

            foreach (var pair in pairs)
            {
              list[pair.Actual.index] = OnError<T>(pair.Expected.Error);
            }
          }
        }
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
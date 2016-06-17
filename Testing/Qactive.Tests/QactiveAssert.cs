using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Qactive.Tests
{
  internal static class QactiveAssert
  {
    public static void AreEqual<T>(IEnumerable<Notification<T>> actual, IEnumerable<Notification<T>> expected)
    {
      var expectedErrors = expected.Where(n => n.Kind == NotificationKind.OnError).Select(n => n.Exception).ToList();

      if (expectedErrors.Count == 0)
      {
        NoOnError(actual);
      }
      else
      {
        var actualErrors = actual.Where(n => n.Kind == NotificationKind.OnError).Select(n => n.Exception).ToList();

        AreEqual(actualErrors, expectedErrors);

        var list = actual as IList<Notification<T>>;

        if (list == null || list.IsReadOnly)
        {
          list = actual.ToList();
          actual = list;
        }

        foreach (var pair in actualErrors.Zip(expectedErrors, (x, y) => y).Select((y, index) => new { Expected = y, Index = index }))
        {
          list[pair.Index] = TestBase.OnError<T>(pair.Expected);
        }
      }

      actual.AssertEqual(expected);
    }

    public static void AreEqual(IEnumerable<Exception> actual, IEnumerable<Exception> expected)
    {
      var expectedErrors = (expected ?? new Exception[0]).Select((ex, index) => new { Error = ex, index }).ToList();
      var actualErrors = (actual ?? new Exception[0]).Select((ex, index) => new { Error = ex, index }).ToList();

      Func<string> buildMessageDetails = () =>
          "Actual Errors: " + Environment.NewLine + string.Join(Environment.NewLine, actualErrors.Select(x => x.index + ": " + x.Error.Message)) + Environment.NewLine
        + "Expected Errors: " + Environment.NewLine + string.Join(Environment.NewLine, expectedErrors.Select(x => x.index + ": " + x.Error.Message));

      if (expectedErrors.Count == 0)
      {
        Assert.AreEqual(0, actualErrors.Count, "Unexpected error(s) encountered." + Environment.NewLine + buildMessageDetails());
      }
      else if (actualErrors.Count != expectedErrors.Count)
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
        else if (!pairs.All(pair => GetEquality(pair.Actual.Error, pair.Expected.Error)))
        {
          Assert.Fail($"Actual errors are not equal to the expected errors." + Environment.NewLine + buildMessageDetails());
        }
      }
    }

    public static void AreEqual<T>(IEnumerable<Notification<T>> actual, params Notification<T>[] expected)
      => AreEqual(actual, (IEnumerable<Notification<T>>)expected);

    public static void AreEqual(Exception actual, Exception expected, string message = "The actual exception differs from the expected exception.")
      => Assert.IsTrue(GetEquality(actual, expected), message);

    public static bool GetEquality(Exception actual, Exception expected)
      => (actual == null && expected == null)
      || (actual != null && expected != null
        && (expected == Any.Exception || expected.GetType().IsAssignableFrom(actual.GetType()))
        && (expected.Message == Any.Message || expected.Message.Equals(actual.Message)));

    public static void NoOnError<T>(IEnumerable<Notification<T>> results)
    {
      var error = TryGetError(results);

      if (error != null)
      {
        Assert.Fail(error.ToString());
      }
    }

    public static Exception TryGetError<T>(IEnumerable<Notification<T>> results)
      => (from result in results
          where result.Kind == NotificationKind.OnError
          select result.Exception)
          .SingleOrDefault();
  }
}

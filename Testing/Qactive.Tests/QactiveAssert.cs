using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive;
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

      if (!expected.SequenceEqual(actual))
      {
        Assert.Fail(CreateMessage(actual, expected, "The sequences are not equal."));
      }
    }

    public static void AreEqual<T>(IEnumerable<Notification<T>> actual, params Notification<T>[] expected)
      => AreEqual(actual, (IEnumerable<Notification<T>>)expected);

    public static void AreEqual(IEnumerable<Expression> actual, IEnumerable<Expression> expected, bool reflectionNamesOnly = true)
    {
      if (actual == null && expected == null)
      {
        return;
      }

      var actualList = actual?.ToList() ?? new List<Expression>(0);
      var expectedList = expected?.ToList() ?? new List<Expression>(0);

      if (actualList.Count != expectedList.Count)
      {
        Assert.Fail(CreateMessage(
          actualList,
          expectedList,
          "The actual expressions count is different from the expected expressions count.",
          label: "Expressions",
          includeIndices: true,
          includeTypes: true));
      }

      TestExpressionEqualityComparer lastComparer = null;
      var fail = false;
      var index = 0;
      foreach (var pair in actualList.Zip(expectedList, (a, e) => new { a, e }))
      {
        lastComparer = new TestExpressionEqualityComparer(reflectionNamesOnly);

        if (!lastComparer.Equals(pair.a, pair.e))
        {
          fail = true;
          break;
        }

        index++;
      }

      if (fail)
      {
        Assert.Fail(CreateMessage(
         actualList,
         expectedList,
         $"The actual expression at index {index} is different from the expected expression.",
         label: "Expressions",
         includeIndices: true,
         includeTypes: true,
         actualDiffs: lastComparer.InequalityNodes,
         expectedDiffs: lastComparer.InequalityOthers));
      }
    }

    public static void AreEqual(IEnumerable<Expression> actual, params Expression[] expected)
      => AreEqual(actual, (IEnumerable<Expression>)expected);

    public static void AreEqual(IEnumerable<Exception> actual, IEnumerable<Exception> expected)
    {
      var expectedErrors = (expected ?? new Exception[0]).Select((ex, index) => new { Error = ex, index }).ToList();
      var actualErrors = (actual ?? new Exception[0]).Select((ex, index) => new { Error = ex, index }).ToList();

      if (expectedErrors.Count == 0)
      {
        Assert.AreEqual(0, actualErrors.Count, CreateMessage(
          actualErrors.Select(e => GetMessage(e.Error)),
          expectedErrors.Select(e => GetMessage(e.Error)),
          "Unexpected error(s) encountered.",
          label: "Exceptions",
          includeIndices: true));
      }
      else if (actualErrors.Count != expectedErrors.Count)
      {
        Assert.Fail(CreateMessage(
          actualErrors.Select(e => GetMessage(e.Error)),
          expectedErrors.Select(e => GetMessage(e.Error)),
          $"Actual error count ({actualErrors.Count}) does not equal the expected error count ({expectedErrors.Count}).",
          label: "Exceptions",
          includeIndices: true));
      }
      else
      {
        var pairs = actualErrors.Zip(expectedErrors, (x, y) => new { Actual = x, Expected = y });

        if (!pairs.All(pair => pair.Actual.index == pair.Expected.index))
        {
          Assert.Fail(CreateMessage(
          actualErrors.Select(e => GetMessage(e.Error)),
          expectedErrors.Select(e => GetMessage(e.Error)),
          "Actual errors are not in the same positions in the sequence as the expected errors.",
          label: "Exceptions",
          includeIndices: true));
        }
        else if (!pairs.All(pair => GetEquality(pair.Actual.Error, pair.Expected.Error)))
        {
          Assert.Fail(CreateMessage(
          actualErrors.Select(e => GetMessage(e.Error)),
          expectedErrors.Select(e => GetMessage(e.Error)),
          "Actual errors are not equal to the expected errors.",
          label: "Exceptions",
          includeIndices: true));
        }
      }
    }

    public static void AreEqual(Exception actual, Exception expected, string message = "The actual exception differs from the expected exception.")
      => Assert.IsTrue(GetEquality(actual, expected), message);

    public static bool GetEquality(Exception actual, Exception expected)
      => (actual == null && expected == null)
      || (actual != null && expected != null
        && (expected.IsAny() || expected.GetType().IsAssignableFrom(actual.GetType()))
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

    public static string CreateMessage<T>(
      IEnumerable<T> actual,
      IEnumerable<T> expected,
      string message = null,
      string label = null,
      bool includeIndices = false,
      bool includeTypes = false,
      IEnumerable<object> actualDiffs = null,
      IEnumerable<object> expectedDiffs = null)
      => (string.IsNullOrWhiteSpace(message) ? string.Empty : message + Environment.NewLine)
       + $"Actual{(string.IsNullOrWhiteSpace(label) ? string.Empty : " " + label)}: " + Environment.NewLine
       + string.Join(Environment.NewLine, actual?.Select((x, i) => (includeIndices ? i + ": " : string.Empty) + (includeTypes ? x.GetType().Name + ": " : string.Empty) + x) ?? Enumerable.Empty<string>()) + Environment.NewLine
       + $"Expected{(string.IsNullOrWhiteSpace(label) ? string.Empty : " " + label)}: " + Environment.NewLine
       + string.Join(Environment.NewLine, expected?.Select((x, i) => (includeIndices ? i + ": " : string.Empty) + (includeTypes ? x.GetType().Name + ": " : string.Empty) + x) ?? Enumerable.Empty<string>())
       + (actualDiffs == null && expectedDiffs == null
       ? string.Empty
       : Environment.NewLine + "Differences" + Environment.NewLine
       + "Actual: " + Environment.NewLine
       + string.Join(Environment.NewLine, actualDiffs?.Select(x => (includeTypes ? x.GetType().Name + ": " : string.Empty) + x) ?? Enumerable.Empty<string>()) + Environment.NewLine
       + "Expected: " + Environment.NewLine
       + string.Join(Environment.NewLine, expectedDiffs?.Select(x => (includeTypes ? x.GetType().Name + ": " : string.Empty) + x) ?? Enumerable.Empty<string>()));

    private static string GetMessage(Exception error)
         => error is AggregateException e ? string.Join(Environment.NewLine, e.InnerExceptions.Select(GetMessage)) : error.Message;
  }
}

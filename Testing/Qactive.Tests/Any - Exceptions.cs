using System;
using System.Linq.Expressions;

namespace Qactive.Tests
{
  internal static partial class Any
  {
    public static readonly Exception Exception = new AnyException();

    public static Exception ExceptionWithMessage(string message)
      => new AnyException(message);

    public static bool IsAny(this Exception exception, string message = null)
      => exception is AnyException
      && (message == null || message == Any.Message || exception.Message == Any.Message || exception.Message == message);

    private sealed class AnyException : Exception
    {
      public AnyException(string message = null)
        : base(message ?? Any.Message)
      {
      }
    }
  }
}

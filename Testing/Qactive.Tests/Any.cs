using System;

namespace Qactive.Tests
{
  internal static class Any
  {
    public const string Message = "Any.Message";
    public static readonly Exception Exception = new AnyException();

    public static Exception ExceptionWithMessage(string message)
      => new AnyException(message);

    private sealed class AnyException : Exception
    {
      public AnyException(string message = null)
        : base(message ?? Any.Message)
      {
      }
    }
  }
}

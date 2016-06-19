using System;
using System.Diagnostics.Contracts;

// TODO: Is there a real implementation of this class for .NET 4.0?
// The MONO source on GitHub depends too much on internal members and I'm not comfortable using reflection for this scenario.
namespace Qactive
{
  /// <summary>
  /// This class merely avoids having to use excessive conditional compilation just to backport to .NET 4.0.
  /// The actual implementation of this class does not preserve the stack trace when <see cref="Throw"/> is called, unlike newer versions of .NET.
  /// </summary>
  public sealed class ExceptionDispatchInfo
  {
    private ExceptionDispatchInfo(Exception exception)
    {
      Contract.Requires(exception != null);

      SourceException = exception;
    }

    [ContractInvariantMethod]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
    private void ObjectInvariant()
    {
      Contract.Invariant(SourceException != null);
    }

    public static ExceptionDispatchInfo Capture(Exception source)
    {
      Contract.Requires(source != null);
      Contract.Ensures(Contract.Result<ExceptionDispatchInfo>() != null);

      return new ExceptionDispatchInfo(source);
    }

    // Return the exception object represented by this ExceptionDispatchInfo instance
    public Exception SourceException { get; }

    /// <summary>
    /// Throws the original exception, without preseving the original stack trace. This is just a loose backport to .NET 4.0 to avoid 
    /// excessive conditional compilation.
    /// </summary>
    public void Throw()
    {
      throw SourceException;
    }
  }
}

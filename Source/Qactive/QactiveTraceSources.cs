using System.Diagnostics;

namespace Qactive
{
  public static class QactiveTraceSources
  {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Using a similar pattern to PresentationTraceSources. This is a cross-cutting property that must only be changed by the main application.")]
    public static readonly TraceSource Qactive = new TraceSource("Qactive", SourceLevels.Information);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Using a similar pattern to PresentationTraceSources. This is a cross-cutting property that must only be changed by the main application.")]
    public static readonly TraceSource QactiveExpressions = new TraceSource("Qactive.Expressions", SourceLevels.Off);
  }
}

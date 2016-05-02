using System.Diagnostics;

namespace Qactive
{
  public static class QactiveTraceSources
  {
    public static readonly TraceSource Qactive = new TraceSource("Qactive", SourceLevels.Information);
    public static readonly TraceSource QactiveExpressions = new TraceSource("Qactive.Expressions", SourceLevels.Off);
  }
}

using System;
using System.Diagnostics;

namespace SharedLibrary
{
  public sealed class AbbreviatedConsoleTraceListener : ConsoleTraceListener
  {
    public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
    {
      ConsoleTrace.WriteLine(ConsoleColor.DarkGray, format, args);
    }

    public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
    {
      ConsoleTrace.WriteLine(ConsoleColor.DarkGray, message);
    }
  }
}
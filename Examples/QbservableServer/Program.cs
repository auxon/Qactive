using System;
using System.Diagnostics;
using SharedLibrary;

namespace QbservableServer
{
  class Program
  {
    static void Main()
    {
      Console.WriteLine("Server starting...");
      Console.WriteLine();

      var trace = new TraceSource("Custom", SourceLevels.All);

      trace.Listeners.Add(new AbbreviatedConsoleTraceListener());

      using (new TimerService().Start(trace))
      using (new BasicService().Start(trace))
      using (new AdvancedService().Start(trace))
      using (new ChatService().Start(trace))
      using (new SandboxedService().Start(trace))
      using (new LimitedService().Start(trace))
      {
        Console.WriteLine("Server started.  Waiting for clients...");
        Console.WriteLine();

        Console.ReadKey(intercept: true);
      }
    }
  }
}
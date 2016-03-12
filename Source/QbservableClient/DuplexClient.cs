using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reflection;
using ReactiveQ;
using SharedLibrary;

namespace QbservableClient
{
  class DuplexClient
  {
    public void Run()
    {
      Console.WriteLine();
      Console.WriteLine("Duplex client starting...");

      var trace = new TraceSource("Custom", SourceLevels.All);

      trace.Listeners.Add(new AbbreviatedConsoleTraceListener());

      var suffix = ")";
      var localObj = new LocalObject();
      var localObservable = Observable
        .Interval(TimeSpan.FromSeconds(2))
        .Take(2)
        .Do(value => trace.TraceInformation("localObservable: {0}", value));

      var client = new QbservableTcpClient<long>(Program.BasicServiceEndPoint, new DuplexLocalEvaluator());

      IQbservable<string> query =
        (from serviceValue in client.Query()
         let prefix = LocalStaticMethod(localObj.LocalInstanceMethod()) + localObj.LocalInstanceProperty
         from clientValue in localObservable
          .Do(value => Console.WriteLine("{0} received value: {1}", Assembly.GetEntryAssembly().EntryPoint.DeclaringType.Namespace, value))
         from random in LocalIterator()
         select prefix + clientValue + "/" + serviceValue + ", R=" + random + suffix)
        .Take(21)
        .Do(value => Console.WriteLine("{0} pushing value: {1}", Assembly.GetEntryAssembly().EntryPoint.DeclaringType.Namespace, value));

      using (query.Subscribe(
        value => ConsoleTrace.WriteLine(ConsoleColor.Green, "Duplex client observed: {0}", value),
        ex => ConsoleTrace.WriteLine(ConsoleColor.Red, "Duplex client error: {0}", ex.Message),
        () => ConsoleTrace.WriteLine(ConsoleColor.DarkCyan, "Duplex client completed")))
      {
        Console.WriteLine();
        Console.WriteLine("Duplex client started.  Waiting for basic service notifications...");
        Console.WriteLine();
        Console.WriteLine("(Press any key to stop)");
        Console.WriteLine();

        Console.ReadKey(intercept: true);
      }
    }

    static IEnumerable<int> LocalIterator()
    {
      ConsoleTrace.PrintCurrentMethod();

      var rnd = new Random();

      for (int i = 0; i < 3; i++)
      {
        yield return rnd.Next(1, 101);
      }
    }

    static string LocalStaticMethod(int ignoredParameter)
    {
      ConsoleTrace.PrintCurrentMethod();

      return "(N";
    }
  }
}
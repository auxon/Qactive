using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using Qactive;
using SharedLibrary;

namespace QbservableClient
{
  class AdvancedClient
  {
    public void Run()
    {

      Console.WriteLine();
      Console.WriteLine("Advanced client starting...");
      Console.WriteLine();

      var suffix = ")";
      var localObj = new LocalObject();
      var localTransferableObj = new SharedLibrary.TransferableObject(100);

      var client = new TcpQbservableClient<long>(Program.BasicServiceEndPoint, typeof(SharedLibrary.TransferableObject));

      IQbservable<string> query =
        (from value in client.Query()
         let r = new TransferableObject(42)
         let numberBase = new { Remote = r.Value, Local = localTransferableObj.Value }
         from n in Observable.Range(1, 5)
         let number = (value + 1) * n
         where number % 2 == 0
         let result = numberBase.Remote + numberBase.Local + number
         let prefix = LocalStaticMethod(localObj.LocalInstanceMethod()) + localObj.LocalInstanceProperty
         let list = (from i in LocalIterator()
                     select i * 2)
                     .Aggregate("", (acc, cur) => acc + cur + ",", acc => acc.Substring(0, acc.Length - 1))
         select prefix + result + ", [" + list + "]" + suffix)
        .Take(16)
        .Do(value => Console.WriteLine("{0} pushing value: {1}", Assembly.GetEntryAssembly().EntryPoint.DeclaringType.Namespace, value));

      using (query.Subscribe(
        value => ConsoleTrace.WriteLine(ConsoleColor.Green, "Advanced client observed: {0}", value),
        ex => ConsoleTrace.WriteLine(ConsoleColor.Red, "Advanced client error: {0}", ex.Message),
        () => ConsoleTrace.WriteLine(ConsoleColor.DarkCyan, "Advanced client completed")))
      {
        Console.WriteLine();
        Console.WriteLine("Advanced client started.  Waiting for basic service notifications...");
        Console.WriteLine();
        Console.WriteLine("(Press any key to continue)");
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
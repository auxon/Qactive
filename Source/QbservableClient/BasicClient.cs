using System;
using System.Reactive.Linq;
using System.Reflection;
using ReactiveQ;
using SharedLibrary;

namespace QbservableClient
{
	class BasicClient
	{
		public void Run()
		{
			var client = new QbservableTcpClient<long>(Program.BasicServiceEndPoint);

			var query =
				(from value in client.Query()
				 where value % 2 == 0
				 select value)
				.Do(value => Console.WriteLine("{0} pushing value: {1}", Assembly.GetEntryAssembly().EntryPoint.DeclaringType.Namespace, value));

			using (query.Subscribe(
				value => ConsoleTrace.WriteLine(ConsoleColor.Green, "Basic client observed: {0}", value),
				ex => ConsoleTrace.WriteLine(ConsoleColor.Red, "Basic client error: {0}", ex.Message),
				() => ConsoleTrace.WriteLine(ConsoleColor.DarkCyan, "Basic client completed")))
			{
				Console.WriteLine();
				Console.WriteLine("Basic client started.  Waiting for basic service notifications...");
				Console.WriteLine();
				Console.WriteLine("(Press any key to stop)");
				Console.WriteLine();

				Console.ReadKey(intercept: true);
			}
		}
	}
}
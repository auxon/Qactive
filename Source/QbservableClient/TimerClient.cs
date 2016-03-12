using System;
using System.Net;
using System.Reactive.Linq;
using QbservableProvider;
using SharedLibrary;

namespace QbservableClient
{
	class TimerClient
	{
		public void Run()
		{
			var client = new QbservableTcpClient<long>(Program.TimerServiceEndPoint);

			IQbservable<int> query =
				(from value in client.Query(TimeSpan.FromSeconds(1))
				 from page in new WebClient().DownloadStringTaskAsync(new Uri("http://blogs.msdn.com/b/rxteam"))
				 select page.Length)
				.Do(result => Console.WriteLine("Where am I?  Result = " + result));

			using (query.Subscribe(
				value => ConsoleTrace.WriteLine(ConsoleColor.Green, "Timer client observed: " + value),
				ex => ConsoleTrace.WriteLine(ConsoleColor.Red, "Timer client error: " + ex.Message),
				() => ConsoleTrace.WriteLine(ConsoleColor.DarkCyan, "Timer client completed")))
			{
				Console.WriteLine();
				Console.WriteLine("Timer client started.  Waiting for timer service notification...");
				Console.WriteLine();
				Console.WriteLine("(Press any key to stop)");
				Console.WriteLine();

				Console.ReadKey(intercept: true);
			}
		}
	}
}
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reflection;
using QbservableProvider;
using SharedLibrary;

namespace QbservableClient
{
	class BasicFeedAggregationClient
	{
		public void Run()
		{

			Console.WriteLine();
			Console.WriteLine("Feed aggregation client starting...");

			var feedAggregatorServiceArgs = new List<FeedServiceArgument>()
			{
				new FeedServiceArgument() { IsAtom = false, Url = new Uri("http://rss.cnn.com/rss/cnn_topstories.rss") }, 
				new FeedServiceArgument() { IsAtom = false, Url = new Uri("http://blogs.msdn.com/b/rxteam/rss.aspx") }, 
				new FeedServiceArgument() { IsAtom = true, Url = new Uri("http://social.msdn.microsoft.com/Forums/en-US/rx/threads?outputAs=atom") }
			};

			var client = new QbservableTcpClient<FeedItem>(Program.AdvancedServiceEndPoint, typeof(FeedItem));

			IQbservable<FeedItem> query =
				(from item in client.Query(feedAggregatorServiceArgs)
				 where item.PublishDate >= DateTimeOffset.UtcNow.AddHours(-DateTimeOffset.UtcNow.TimeOfDay.TotalHours)
				 select item)
				.Do(value => Console.WriteLine("{0} pushing value: {1}", Assembly.GetEntryAssembly().EntryPoint.DeclaringType.Namespace, value.Title));

			using (query.Subscribe(
				value => ConsoleTrace.WriteLine(ConsoleColor.Green, "Feed client observed: ({0}) {1}", value.FeedUrl.Host, value.Title),
				ex => ConsoleTrace.WriteLine(ConsoleColor.Red, "Feed client error: {0}", ex.Message),
				() => ConsoleTrace.WriteLine(ConsoleColor.DarkCyan, "Feed client completed")))
			{
				Console.WriteLine();
				Console.WriteLine("Feed client started.  Waiting for advanced service notifications...");
				Console.WriteLine();
				Console.WriteLine("(Press any key to stop)");
				Console.WriteLine();

				Console.ReadKey(intercept: true);
			}
		}
	}
}
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Qactive;
using SharedLibrary;

namespace QbservableClient
{
  class ChatClient
  {
    public void Run()
    {
      var client = new TcpQbservableClient<ChatServiceHooks>(Program.ChatServiceEndPoint);

      Console.WriteLine();
      Console.Write("Enter your user name> ");

      string userName = Console.ReadLine();

      Console.Write("Enter names to ignore separated by commas> ");

      var userNamesToIgnore = Console.ReadLine().Split(',').Select(name => name.Trim() + ' ');

      var myMessages = new Subject<string>();

      IObservable<string> outgoingMessages = myMessages;

      IQbservable<string> query =
        (from hooks in client.Query(userName)
           .Do(hooks => outgoingMessages.Subscribe(hooks.IncomingMessages))
         from message in hooks.OutgoingMessages
         where !userNamesToIgnore.Any(message.StartsWith)
         select message);

      using (query.Subscribe(
        message => ConsoleTrace.WriteLine(ConsoleColor.Green, message),
        ex => ConsoleTrace.WriteLine(ConsoleColor.Red, "Chat client error: " + ex.Message),
        () => ConsoleTrace.WriteLine(ConsoleColor.DarkCyan, "Chat client completed")))
      {
        Console.WriteLine();
        Console.WriteLine("Chat client started.  You may begin entering messages...");
        Console.WriteLine();
        Console.WriteLine("(Enter a blank line to stop)");
        Console.WriteLine();

        string message;
        while ((message = Console.ReadLine()).Length > 0)
        {
          myMessages.OnNext(message);
        }
      }
    }
  }
}
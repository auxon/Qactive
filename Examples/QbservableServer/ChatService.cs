using System;
using System.Diagnostics;
using System.Net;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Qactive;
using SharedLibrary;

namespace QbservableServer
{
  class ChatService
  {
    private static readonly IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, port: 19841);

    public IDisposable Start(TraceSource trace)
    {
      var messageDispatch = new Subject<string>();

      messageDispatch.Subscribe(message => ConsoleTrace.WriteLine(ConsoleColor.DarkGray, message));

      var service = Qactive.TcpQbservableServer.CreateService<string, ChatServiceHooks>(
        endPoint,
        new QbservableServiceOptions() { EnableDuplex = true, AllowExpressionsUnrestricted = true },
        (IObservable<string> request) =>
          (from userName in request
           from hooks in Observable.Create<ChatServiceHooks>(
            (IObserver<ChatServiceHooks> observer) =>
            {
              messageDispatch.OnNext(userName + " is online.");

              var hooks = new ChatServiceHooks(userName, messageDispatch);

              Scheduler.CurrentThread.Schedule(() => observer.OnNext(hooks));

              return () => messageDispatch.OnNext(userName + " is offline.");
            })
           select hooks));

      return service.Subscribe(
        terminatedClient =>
        {
          foreach (var ex in terminatedClient.Exceptions)
          {
            ConsoleTrace.WriteLine(ConsoleColor.Magenta, "Chat service error: " + ex.SourceException.Message);
          }

          ConsoleTrace.WriteLine(ConsoleColor.Yellow, "Chat client shutdown: " + terminatedClient);
        },
        ex => ConsoleTrace.WriteLine(ConsoleColor.Red, "Chat service fatal error: " + ex.Message),
        () => Console.WriteLine("This will never be printed because a service host never completes."));
    }
  }
}
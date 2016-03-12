using System;
using System.Diagnostics;
using System.Net;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using ReactiveQ;
using SharedLibrary;

namespace QbservableServer
{
  public class LimitedService
  {
    private static readonly IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, port: 9105);

    public IDisposable Start(TraceSource trace)
    {
      IObservable<int> source = Observable.Range(1, 3, ThreadPoolScheduler.Instance)
        .Do(value => ConsoleTrace.WriteLine(ConsoleColor.Green, "Limited service generated value: {0}", value));

      var service = source.ServeQbservableTcp(endPoint);

      return service.Subscribe(
        terminatedClient =>
        {
          foreach (var ex in terminatedClient.Exceptions)
          {
            ConsoleTrace.WriteLine(ConsoleColor.Magenta, "Limited service error: " + ex.SourceException.Message);
          }

          ConsoleTrace.WriteLine(ConsoleColor.Yellow, "Malicious client shutdown: " + terminatedClient.Reason);
        },
        ex => ConsoleTrace.WriteLine(ConsoleColor.Red, "Limited service fatal error: " + ex.Message),
        () => Console.WriteLine("This will never be printed because a service host never completes."));
    }
  }
}
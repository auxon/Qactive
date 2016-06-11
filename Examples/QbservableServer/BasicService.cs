using System;
using System.Diagnostics;
using System.Net;
using System.Reactive.Linq;
using Qactive;
using SharedLibrary;

namespace QbservableServer
{
  class BasicService
  {
    private static readonly IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, port: 38245);

    public IDisposable Start(TraceSource trace)
    {
      IObservable<long> source = Observable.Interval(TimeSpan.FromSeconds(1))
        .Do(value => ConsoleTrace.WriteLine(ConsoleColor.Green, "Basic service generated value: {0}", value));

      var service = source.ServeQbservableTcp(
        endPoint,
        new QbservableServiceOptions() { SendServerErrorsToClients = true, EnableDuplex = true, AllowExpressionsUnrestricted = true });

      return service.Subscribe(
        terminatedClient =>
        {
          foreach (var ex in terminatedClient.Exceptions)
          {
            ConsoleTrace.WriteLine(ConsoleColor.Magenta, "Basic service error: {0}", ex.SourceException.Message);
          }

          ConsoleTrace.WriteLine(ConsoleColor.Yellow, "Basic client shutdown: " + terminatedClient);
        },
        ex => ConsoleTrace.WriteLine(ConsoleColor.Red, "Basic fatal service error: {0}", ex.Message),
        () => Console.WriteLine("This will never be printed because a service host never completes."));
    }
  }
}
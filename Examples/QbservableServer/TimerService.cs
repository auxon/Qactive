using System;
using System.Diagnostics;
using System.Net;
using System.Reactive.Linq;
using Qactive;
using SharedLibrary;

namespace QbservableServer
{
  class TimerService
  {
    private static readonly IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, port: 49593);

    public IDisposable Start(TraceSource trace)
    {
      var service = Qactive.TcpQbservableServer.CreateService<TimeSpan, long>(
        endPoint,
        new QbservableServiceOptions() { AllowExpressionsUnrestricted = true },
        (IObservable<TimeSpan> request) =>
          (from duration in request.Do(arg => Console.WriteLine("Timer client sent arg: " + arg))
           from value in Observable.Timer(duration)
           select value));

      return service.Subscribe(
        terminatedClient =>
        {
          foreach (var ex in terminatedClient.Exceptions)
          {
            ConsoleTrace.WriteLine(ConsoleColor.Magenta, "Timer service error: " + ex.SourceException.Message);
          }

          ConsoleTrace.WriteLine(ConsoleColor.Yellow, "Timer client shutdown: " + terminatedClient);
        },
        ex => ConsoleTrace.WriteLine(ConsoleColor.Red, "Timer service fatal error: " + ex.Message),
        () => Console.WriteLine("This will never be printed because a service host never completes."));
    }
  }
}
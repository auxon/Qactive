using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Security;
using System.Security.Permissions;
using Qactive;
using SharedLibrary;

namespace QbservableServer
{
  public class SandboxedService
  {
    private static readonly IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, port: 7223);

    public IDisposable Start(TraceSource trace)
    {
      var appBase = Path.GetDirectoryName(new Uri(Assembly.GetEntryAssembly().CodeBase).LocalPath);

      // Excluded this logic because unless the exe is configured to load all assemblies from bin, you end up loading 
      // two different copies of the same assembly into the AppDomains and the types are incompatible.
      //
      //#if DEBUG
      //      var newAppBase = appBase;
      //#else
      //			/* Released example apps have all of their dependencies in a bin\ folder.
      //			 * It's more secure setting the bin folder as the app domain's base path 
      //			 * instead of it being the same as the host app with a probing path.
      //			 */
      //			var newAppBase = Path.Combine(appBase, "bin");
      //#endif

      var service = TcpQbservableServer.CreateService<object, int, TransportInitializer>(
        new AppDomainSetup() { ApplicationBase = appBase },
        endPoint,
        new QbservableServiceOptions() { AllowExpressionsUnrestricted = true },
        new Func<IObservable<object>, IObservable<int>>(CreateService));

      return service.Subscribe(
        terminatedClient => DoUnrestricted(() =>
        {
          foreach (var ex in terminatedClient.Exceptions)
          {
            var security = ex.SourceException as SecurityException;

            ConsoleTrace.WriteLine(ConsoleColor.Magenta, $"Sandboxed service error: {security?.Demanded ?? ex.SourceException.Message}");
          }

          ConsoleTrace.WriteLine(ConsoleColor.Yellow, $"Malicious client shutdown: {terminatedClient}");
        }),
        ex => DoUnrestricted(() => ConsoleTrace.WriteLine(ConsoleColor.Red, $"Sandboxed service fatal error: {ex.Message}")),
        () => Console.WriteLine("This will never be printed because a service host never completes."));
    }

    public static IObservable<int> CreateService(IObservable<object> request)
    {
      return Observable.Range(1, 3, ThreadPoolScheduler.Instance);
    }

    private void DoUnrestricted(Action action)
    {
      new PermissionSet(PermissionState.Unrestricted).Assert();

      try
      {
        action();
      }
      finally
      {
        PermissionSet.RevertAssert();
      }
    }

    /// <summary>
    /// This type is entirely optional. You can omit the corresponding type arg when 
    /// calling <see cref="TcpQbservableServer.CreateService"/> if you don't need to 
    /// prepare sockets or create formatters.
    /// </summary>
    private sealed class TransportInitializer : ITcpQactiveProviderTransportInitializer
    {
      public void Prepare(Socket socket)
      {
        new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Assert();

        try
        {
          socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
          socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, true);
        }
        finally
        {
          CodeAccessPermission.RevertAssert();
        }
      }

      public IRemotingFormatter CreateFormatter()
      {
        return null;
      }

      public void StartedListener(int serverNumber, EndPoint endPoint)
      {
      }

      public void StoppedListener(int serverNumber, EndPoint endPoint)
      {
      }
    }
  }
}
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reflection;
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

#if DEBUG
			var newAppBase = appBase;
#else
			/* Released example apps have all of their dependencies in a bin\ folder.
			 * It's more secure setting the bin folder as the app domain's base path 
			 * instead of it being the same as the host app with a probing path.
			 */
			var newAppBase = Path.Combine(appBase, "bin");
#endif

			var service = QbservableTcpServer.CreateService<object, int>(
				new AppDomainSetup() { ApplicationBase = newAppBase },
				endPoint,
				new QbservableServiceOptions() { AllowExpressionsUnrestricted = true },
				new Func<IObservable<object>, IObservable<int>>(CreateService));

			return service.Subscribe(
				terminatedClient => DoUnrestricted(() =>
				{
					foreach (var ex in terminatedClient.Exceptions)
					{
						ConsoleTrace.WriteLine(ConsoleColor.Magenta, "Sandboxed service error: " + ex.SourceException.Message);
					}

					ConsoleTrace.WriteLine(ConsoleColor.Yellow, "Malicious client shutdown: " + terminatedClient.Reason);
				}),
				ex => DoUnrestricted(() => ConsoleTrace.WriteLine(ConsoleColor.Red, "Sandboxed service fatal error: " + ex.Message)),
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
	}
}
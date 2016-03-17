using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization.Formatters.Binary;

namespace Qactive
{
  public static partial class QbservableTcpServer
  {
    // This method must be public otherwise CreateService fails inside of a new AppDomain - see CreateServiceProxy comments
    public static IRemotingFormatter CreateDefaultFormatter()
    {
      return new BinaryFormatter();
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<TcpClientTermination> CreateService<TSource, TResult>(
      IPEndPoint endPoint,
      Func<IObservable<TSource>, IObservable<TResult>> service)
    {
      return CreateService<TSource, TResult>(endPoint, request => service(request).AsQbservable());
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<TcpClientTermination> CreateService<TSource, TResult>(
      IPEndPoint endPoint,
      QbservableServiceOptions options,
      Func<IObservable<TSource>, IObservable<TResult>> service)
    {
      return CreateService<TSource, TResult>(endPoint, options, request => service(request).AsQbservable());
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<TcpClientTermination> CreateService<TSource, TResult>(
      IPEndPoint endPoint,
      IRemotingFormatter formatter,
      Func<IObservable<TSource>, IObservable<TResult>> service)
    {
      return CreateService<TSource, TResult>(endPoint, formatter, request => service(request).AsQbservable());
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<TcpClientTermination> CreateService<TSource, TResult>(
      IPEndPoint endPoint,
      IRemotingFormatter formatter,
      QbservableServiceOptions options,
      Func<IObservable<TSource>, IObservable<TResult>> service)
    {
      return CreateService<TSource, TResult>(endPoint, formatter, options, request => service(request).AsQbservable());
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<TcpClientTermination> CreateService<TSource, TResult>(
      IPEndPoint endPoint,
      Func<IObservable<TSource>, IQbservable<TResult>> service)
    {
      return CreateService<TSource, TResult>(endPoint, CreateDefaultFormatter, QbservableServiceOptions.Default, service);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<TcpClientTermination> CreateService<TSource, TResult>(
      IPEndPoint endPoint,
      QbservableServiceOptions options,
      Func<IObservable<TSource>, IQbservable<TResult>> service)
    {
      return CreateService<TSource, TResult>(endPoint, CreateDefaultFormatter, options, service);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<TcpClientTermination> CreateService<TSource, TResult>(
      IPEndPoint endPoint,
      IRemotingFormatter formatter,
      Func<IObservable<TSource>, IQbservable<TResult>> service)
    {
      return CreateService<TSource, TResult>(endPoint, () => formatter, QbservableServiceOptions.Default, service);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<TcpClientTermination> CreateService<TSource, TResult>(
      IPEndPoint endPoint,
      IRemotingFormatter formatter,
      QbservableServiceOptions options,
      Func<IObservable<TSource>, IQbservable<TResult>> service)
    {
      return CreateService<TSource, TResult>(endPoint, () => formatter, options, service);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    private static IObservable<TcpClientTermination> CreateService<TSource, TResult>(
      IPEndPoint endPoint,
      Func<IRemotingFormatter> formatterFactory,
      QbservableServiceOptions options,
      Func<IObservable<TSource>, IQbservable<TResult>> service)
    {
      var listener = new TcpListener(endPoint);

      listener.Start();

      return Observable
        .FromAsync(listener.AcceptTcpClientAsync)
        .Repeat()
        .Finally(listener.Stop)
        .SelectMany(client => Observable
          .StartAsync(async cancel =>
          {
            var watch = Stopwatch.StartNew();

            var localEndPoint = client.Client.LocalEndPoint;
            var remoteEndPoint = client.Client.RemoteEndPoint;

            var exceptions = new List<ExceptionDispatchInfo>();
            var shutdownReason = QbservableProtocolShutdownReason.None;

            try
            {
              using (var stream = client.GetStream())
              using (var protocol = await QbservableProtocol.NegotiateServerAsync(stream, formatterFactory(), options, cancel).ConfigureAwait(false))
              {
                var provider = new TcpServerQbservableProvider<TResult>(
                  protocol,
                  options,
                  argument =>
                  {
                    if (argument == null && typeof(TSource).IsValueType)
                    {
                      return service(Observable.Return(default(TSource)));
                    }
                    else
                    {
                      return service(Observable.Return((TSource)argument));
                    }
                  });

                try
                {
                  await protocol.ExecuteServerAsync(provider).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception ex)
                {
                  exceptions.Add(ExceptionDispatchInfo.Capture(ex));
                }

                var protocolExceptions = protocol.Exceptions;

                if (protocolExceptions != null)
                {
                  foreach (var exception in protocolExceptions)
                  {
                    exceptions.Add(exception);
                  }
                }

                shutdownReason = protocol.ShutdownReason;
              }
            }
            catch (OperationCanceledException)
            {
              shutdownReason = QbservableProtocolShutdownReason.ProtocolNegotiationCanceled;
            }
            catch (Exception ex)
            {
              shutdownReason = QbservableProtocolShutdownReason.ProtocolNegotiationError;

              exceptions.Add(ExceptionDispatchInfo.Capture(ex));
            }

            return new TcpClientTermination(localEndPoint, remoteEndPoint, watch.Elapsed, shutdownReason, exceptions);
          })
          .Finally(client.Close));
    }
  }
}
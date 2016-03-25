using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Runtime.ExceptionServices;
using System.Runtime.Remoting.Messaging;
using System.Threading;

namespace Qactive
{
  internal sealed class TcpQactiveProvider : QactiveProvider
  {
    public IPEndPoint EndPoint { get; }

    private readonly Func<IRemotingFormatter> formatterFactory;
    private readonly Action<Socket> prepareSocket;

    private TcpQactiveProvider(IPEndPoint endPoint, Action<Socket> prepareSocket, Func<IRemotingFormatter> formatterFactory)
    {
      EndPoint = endPoint;
      this.formatterFactory = formatterFactory;
    }

    private TcpQactiveProvider(Type sourceType, IPEndPoint endPoint, Action<Socket> prepareSocket, IRemotingFormatter formatter, LocalEvaluator localEvaluator)
      : base(sourceType, localEvaluator)
    {
      EndPoint = endPoint;
      formatterFactory = () => formatter;
    }

    private TcpQactiveProvider(Type sourceType, IPEndPoint endPoint, Action<Socket> prepareSocket, IRemotingFormatter formatter, LocalEvaluator localEvaluator, object argument)
      : base(sourceType, localEvaluator, argument)
    {
      EndPoint = endPoint;
      formatterFactory = () => formatter;
    }

    public static TcpQactiveProvider Client(Type sourceType, IPEndPoint endPoint, Action<Socket> prepareSocket, IRemotingFormatter formatter, LocalEvaluator localEvaluator)
      => new TcpQactiveProvider(sourceType, endPoint, prepareSocket, formatter, localEvaluator);

    public static TcpQactiveProvider Client(Type sourceType, IPEndPoint endPoint, Action<Socket> prepareSocket, IRemotingFormatter formatter, LocalEvaluator localEvaluator, object argument)
      => new TcpQactiveProvider(sourceType, endPoint, prepareSocket, formatter, localEvaluator, argument);

    public static TcpQactiveProvider Server(IPEndPoint endPoint, Action<Socket> prepareSocket)
      => new TcpQactiveProvider(endPoint, prepareSocket, TcpQactiveDefaults.CreateDefaultFormatter);

    public static TcpQactiveProvider Server(IPEndPoint endPoint, Action<Socket> prepareSocket, Func<IRemotingFormatter> formatterFactory)
      => new TcpQactiveProvider(endPoint, prepareSocket, formatterFactory);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The SocketAsyncEventArgs instance is either disposed before returning or by the observable's Finally operator.")]
    public override IObservable<TResult> Connect<TResult>(Func<QbservableProtocol, Expression> prepareExpression)
    {
      SocketAsyncEventArgs e = null;
      Socket socket = null;
      try
      {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        prepareSocket(socket);

        e = new SocketAsyncEventArgs()
        {
          RemoteEndPoint = EndPoint
        };

        IConnectableObservable<SocketAsyncEventArgs> connected;
        IDisposable subscription;

        using (var completedSynchronously = new Subject<SocketAsyncEventArgs>())
        {
          connected = Observable.FromEventPattern<SocketAsyncEventArgs>(
            handler => e.Completed += handler,
            handler => e.Completed -= handler)
            .Select(e2 => e2.EventArgs)
            .Amb(completedSynchronously)
            .Take(1)
            .Finally(e.Dispose)
            .PublishLast();

          subscription = connected.Connect();

          if (!socket.ConnectAsync(e))
          {
            completedSynchronously.OnNext(e);
          }
        }

        return Observable.Using(
            () => new CompositeDisposable(subscription, socket),
            _ => (from e2 in connected
                  from result in e2.SocketError != SocketError.Success
                               ? Observable.Throw<TResult>(new SocketException((int)e2.SocketError))
                               : Observable.Create<TResult>(
                                  innerObserver =>
                                  {
                                    var cancel = new CancellationDisposable();

                                    var s = Observable.Using(
                                      () => new NetworkStream(e2.ConnectSocket, ownsSocket: false),
                                      stream => ReadObservable<TResult>(stream, prepareExpression, cancel.Token))
                                      .Subscribe(innerObserver);

                                    return new CompositeDisposable(s, cancel);
                                  })
                                  .Finally(e2.ConnectSocket.Close)
                  select result));
      }
      catch
      {
        if (socket != null)
        {
          socket.Dispose();
        }

        if (e != null)
        {
          e.Dispose();
        }

        throw;
      }
    }

    private IObservable<TResult> ReadObservable<TResult>(Stream stream, Func<QbservableProtocol, Expression> prepareExpression, CancellationToken cancel)
      => from protocol in QbservableProtocol.NegotiateClientAsync(stream, formatterFactory(), cancel).ToObservable()
         from result in protocol
          .ExecuteClient<TResult>(prepareExpression(protocol), Argument)
          .Finally(protocol.Dispose)
         select result;

    public override IObservable<ClientTermination> Listen(
      QbservableServiceOptions options,
      Func<QbservableProtocol, IParameterizedQbservableProvider> providerFactory)
      => from listener in Observable.Return(new TcpListener(EndPoint))
         .Do(listener => listener.Start())
         from result in Observable
           .FromAsync(listener.AcceptTcpClientAsync)
           .Repeat()
           .Finally(listener.Stop)
           .SelectMany(client => Observable
             .StartAsync(async cancel =>
             {
               prepareSocket(client.Client);

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
                   var provider = providerFactory(protocol);

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

               return new ClientTermination(localEndPoint, remoteEndPoint, watch.Elapsed, shutdownReason, exceptions);
             })
             .Finally(client.Close))
         select result;
  }
}

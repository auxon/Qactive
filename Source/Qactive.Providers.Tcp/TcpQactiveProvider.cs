using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
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
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Qactive
{
  internal sealed class TcpQactiveProvider : QactiveProvider
  {
    public IPEndPoint EndPoint { get; }

    protected override object Id
      => clientNumber.HasValue
       ? "C" + clientNumber.Value + " " + EndPoint
       : "S" + serverNumber.Value + " " + EndPoint;

    private static int lastServerNumber = -1;
    private static int lastClientNumber = -1;

    private readonly Func<IRemotingFormatter> formatterFactory;
    private readonly Action<Socket> prepareSocket;
    private readonly int? serverNumber, clientNumber;
    private int lastServerClientNumber = -1;

    private TcpQactiveProvider(IPEndPoint endPoint, ITcpQactiveProviderTransportInitializer transportInitializer)
    {
      Contract.Requires(endPoint != null);

      EndPoint = endPoint;
      serverNumber = Interlocked.Increment(ref lastServerNumber);

      if (transportInitializer != null)
      {
        prepareSocket = transportInitializer.Prepare;
        formatterFactory = () => transportInitializer.CreateFormatter() ?? TcpQactiveDefaults.CreateDefaultFormatter();
      }
      else
      {
        prepareSocket = Nop.Action;
        formatterFactory = TcpQactiveDefaults.CreateDefaultFormatter;
      }
    }

    private TcpQactiveProvider(Type sourceType, IPEndPoint endPoint, Action<Socket> prepareSocket, IRemotingFormatter formatter, LocalEvaluator localEvaluator)
      : base(sourceType, localEvaluator)
    {
      Contract.Requires(sourceType != null);
      Contract.Requires(endPoint != null);
      Contract.Requires(prepareSocket != null);
      Contract.Requires(formatter != null);
      Contract.Requires(localEvaluator != null);

      EndPoint = endPoint;
      clientNumber = Interlocked.Increment(ref lastClientNumber);
      this.prepareSocket = prepareSocket;
      formatterFactory = new ConstantFormatterFactory(formatter).GetFormatter;
    }

    private TcpQactiveProvider(Type sourceType, IPEndPoint endPoint, Action<Socket> prepareSocket, IRemotingFormatter formatter, LocalEvaluator localEvaluator, object argument)
      : base(sourceType, localEvaluator, argument)
    {
      Contract.Requires(sourceType != null);
      Contract.Requires(endPoint != null);
      Contract.Requires(prepareSocket != null);
      Contract.Requires(formatter != null);
      Contract.Requires(localEvaluator != null);

      EndPoint = endPoint;
      clientNumber = Interlocked.Increment(ref lastClientNumber);
      this.prepareSocket = prepareSocket;
      formatterFactory = new ConstantFormatterFactory(formatter).GetFormatter;
    }

    [ContractInvariantMethod]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
    private void ObjectInvariant()
    {
      Contract.Invariant(EndPoint != null);
      Contract.Invariant(formatterFactory != null);
      Contract.Invariant(prepareSocket != null);
    }

    public static TcpQactiveProvider Client(Type sourceType, IPEndPoint endPoint, Action<Socket> prepareSocket, IRemotingFormatter formatter, LocalEvaluator localEvaluator)
    {
      Contract.Requires(sourceType != null);
      Contract.Requires(endPoint != null);
      Contract.Requires(prepareSocket != null);
      Contract.Requires(formatter != null);
      Contract.Requires(localEvaluator != null);
      Contract.Ensures(Contract.Result<TcpQactiveProvider>() != null);

      return new TcpQactiveProvider(sourceType, endPoint, prepareSocket, formatter, localEvaluator);
    }

    public static TcpQactiveProvider Client(Type sourceType, IPEndPoint endPoint, Action<Socket> prepareSocket, IRemotingFormatter formatter, LocalEvaluator localEvaluator, object argument)
    {
      Contract.Requires(sourceType != null);
      Contract.Requires(endPoint != null);
      Contract.Requires(prepareSocket != null);
      Contract.Requires(formatter != null);
      Contract.Requires(localEvaluator != null);
      Contract.Ensures(Contract.Result<TcpQactiveProvider>() != null);

      return new TcpQactiveProvider(sourceType, endPoint, prepareSocket, formatter, localEvaluator, argument);
    }

    public static TcpQactiveProvider Server(IPEndPoint endPoint, ITcpQactiveProviderTransportInitializer transportInitializer = null)
    {
      Contract.Requires(endPoint != null);
      Contract.Ensures(Contract.Result<TcpQactiveProvider>() != null);

      return new TcpQactiveProvider(endPoint, transportInitializer);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The SocketAsyncEventArgs instance is either disposed before returning or by the observable's Finally operator.")]
    public override IObservable<TResult> Connect<TResult>(Func<IQbservableProtocol, Expression> prepareExpression)
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
                                      .SubscribeSafe(innerObserver);

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

    private IObservable<TResult> ReadObservable<TResult>(Stream stream, Func<IQbservableProtocol, Expression> prepareExpression, CancellationToken cancel)
    {
      Contract.Requires(stream != null);
      Contract.Requires(prepareExpression != null);
      Contract.Ensures(Contract.Result<IObservable<TResult>>() != null);

      return from protocol in NegotiateClientAsync(stream, formatterFactory(), cancel).ToObservable()
             from result in protocol
              .ExecuteClient<TResult>(prepareExpression(protocol), Argument)
              .Finally(protocol.Dispose)
             select result;
    }

    public override IObservable<ClientTermination> Listen(QbservableServiceOptions options, Func<IQbservableProtocol, IParameterizedQbservableProvider> providerFactory)
    {
      return from listener in Observable.Return(new TcpListener(EndPoint))
             .Do(listener => listener.Start())
             from client in Observable.FromAsync(listener.AcceptTcpClientAsync).Repeat().Finally(listener.Stop)
             let number = Interlocked.Increment(ref lastServerClientNumber)
             from result in Observable.StartAsync(async cancel =>
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
                 using (var protocol = await NegotiateServerAsync(Id + " C" + number + " " + remoteEndPoint, stream, formatterFactory(), options, cancel).ConfigureAwait(false))
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

               return new TcpClientTermination(localEndPoint, remoteEndPoint, watch.Elapsed, shutdownReason, exceptions);
             })
             .Finally(client.Close)
             select result;
    }

    private async Task<IStreamQbservableProtocol> NegotiateClientAsync(Stream stream, IRemotingFormatter formatter, CancellationToken cancel)
    {
      Contract.Requires(stream != null);
      Contract.Requires(formatter != null);

      var id = (string)Id;
      var protocol = StreamQbservableProtocolFactory.CreateClient(id, stream, formatter, cancel);

      var buffer = Encoding.ASCII.GetBytes(id);

      await protocol.SendAsync(BitConverter.GetBytes(id.Length), 0, 4).ConfigureAwait(false);
      await protocol.SendAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
      await protocol.ReceiveAsync(buffer, 0, buffer.Length).ConfigureAwait(false);

      Contract.Assume(Encoding.ASCII.GetString(buffer) == id);

      return protocol;
    }

    private static async Task<IStreamQbservableProtocol> NegotiateServerAsync(string baseId, Stream stream, IRemotingFormatter formatter, QbservableServiceOptions serviceOptions, CancellationToken cancel)
    {
      Contract.Requires(stream != null);
      Contract.Requires(formatter != null);
      Contract.Requires(serviceOptions != null);

      var protocol = StreamQbservableProtocolFactory.CreateServer(stream, formatter, serviceOptions, cancel);

      var lengthBuffer = new byte[4];

      await protocol.ReceiveAsync(lengthBuffer, 0, 4).ConfigureAwait(false);

      var length = BitConverter.ToInt32(lengthBuffer, 0);

      if (length <= 0 || length > 255)
      {
        throw new InvalidOperationException("Invalid client ID received. (" + length + " bytes)");
      }

      var clientIdBuffer = new byte[length];

      await protocol.ReceiveAsync(clientIdBuffer, 0, clientIdBuffer.Length).ConfigureAwait(false);

      var clientId = Encoding.ASCII.GetString(clientIdBuffer);

      if (clientId == null || string.IsNullOrWhiteSpace(clientId))
      {
        throw new InvalidOperationException("Invalid client ID received (empty or only whitespace).");
      }

      await protocol.SendAsync(clientIdBuffer, 0, clientIdBuffer.Length).ConfigureAwait(false);

      protocol.ClientId = baseId + " (" + clientId + ")";

      return protocol;
    }

    // This class avoids a compiler-generated closure, which was causing the Code Contract rewriter to generate invalid code.
    private sealed class ConstantFormatterFactory
    {
      private readonly IRemotingFormatter formatter;

      public ConstantFormatterFactory(IRemotingFormatter formatter)
      {
        Contract.Requires(formatter != null);

        this.formatter = formatter;
      }

      [ContractInvariantMethod]
      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
      private void ObjectInvariant()
      {
        Contract.Invariant(formatter != null);
      }

      public IRemotingFormatter GetFormatter() => formatter;
    }
  }
}

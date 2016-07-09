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
    private const int LingerTimeInSeconds = 5;

    public IPEndPoint EndPoint { get; }

    protected override object Id
      => clientNumber.HasValue
       ? "C" + clientNumber.Value + " " + EndPoint
       : "S" + serverNumber.Value + " " + EndPoint;

    private static int lastServerNumber = -1;
    private static int lastClientNumber = -1;

    private readonly ITcpQactiveProviderTransportInitializer transportInitializer;
    private readonly Func<IRemotingFormatter> formatterFactory;
    private readonly Action<Socket> prepareSocket;
    private readonly int? serverNumber, clientNumber;
    private int lastServerClientNumber = -1;

    private TcpQactiveProvider(IPEndPoint endPoint, ITcpQactiveProviderTransportInitializer transportInitializer)
    {
      Contract.Requires(endPoint != null);

      EndPoint = endPoint;
      this.transportInitializer = transportInitializer;
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

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Seems as simple as it's going to get.")]
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

          Starting();

          if (!socket.ConnectAsync(e))
          {
            completedSynchronously.OnNext(e);
          }
        }

        return Observable.Using(
            () => new CompositeDisposable(subscription, socket),
            _ => (from e2 in connected.Do(__ => Started())
                  from result in e2.SocketError != SocketError.Success
                               ? Observable.Throw<TResult>(new SocketException((int)e2.SocketError))
                               : Observable.Create<TResult>(
                                  innerObserver =>
                                  {
                                    var cancel = new CancellationDisposable();

                                    var s = Observable.Using(
                                      () => new NetworkStream(e2.ConnectSocket, ownsSocket: false),
                                      stream => GetObservable<TResult>(stream, prepareExpression, cancel.Token))
                                      .SubscribeSafe(innerObserver);

                                    return new CompositeDisposable(s, cancel);
                                  })
                                  .Finally(() => Shutdown(e2.ConnectSocket))
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

    private IObservable<TResult> GetObservable<TResult>(Stream stream, Func<IQbservableProtocol, Expression> prepareExpression, CancellationToken cancel)
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

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Seems as simple as it's going to get.")]
    public override IObservable<ClientTermination> Listen(QbservableServiceOptions options, Func<IQbservableProtocol, IParameterizedQbservableProvider> providerFactory)
    {
      return from listener in Observable.Return(new TcpListener(EndPoint))
             .Do(listener =>
               {
                 Starting();

                 listener.Start();

                 if (transportInitializer != null)
                 {
                   transportInitializer.StartedListener(serverNumber.Value, listener.LocalEndpoint);
                 }

                 Started();
               })
             from client in Observable.FromAsync(listener.AcceptTcpClientAsync).Repeat()
             .Finally(() =>
             {
               Stopping();

               var endPoint = listener.LocalEndpoint;

               listener.Stop();

               if (transportInitializer != null)
               {
                 transportInitializer.StoppedListener(serverNumber.Value, endPoint);
               }

               Stopped();
             })
             let capturedId = new CapturedId(Id + " C" + Interlocked.Increment(ref lastServerClientNumber) + " " + client.Client.RemoteEndPoint)
             from termination in Observable.StartAsync(cancel => AcceptAsync(client, capturedId, options, providerFactory, cancel))
             .Finally(() => Shutdown(client.Client, capturedId.Value))
             select termination;
    }

    private async Task<TcpClientTermination> AcceptAsync(
      TcpClient client,
      CapturedId capturedId,
      QbservableServiceOptions options,
      Func<IQbservableProtocol, IParameterizedQbservableProvider> providerFactory,
      CancellationToken cancel)
    {
      Contract.Requires(client != null);
      Contract.Requires(capturedId != null);
      Contract.Requires(options != null);
      Contract.Requires(providerFactory != null);

      ReceivingConnection(idOverride: capturedId.Value);

      // These default settings enable a proper graceful shutdown. DisconnectAsync is used instead of Close on the server-side to request 
      // that the client terminates the connection ASAP. This is important because it prevents the server-side socket from going into a 
      // TIME_WAIT state rather than the client. The linger option is meant to ensure that any outgoing data, such as an exception, is 
      // completely transmitted to the client before the socket terminates. The seconds specified is arbitrary, though chosen to be large 
      // enough to transfer any remaining data successfully and small enough to cause a timely disconnection. A custom prepareSocket 
      // implementation can always change it via SetSocketOption, if necessary.
      //
      // https://msdn.microsoft.com/en-us/library/system.net.sockets.socket.disconnect(v=vs.110).aspx
      client.LingerState.Enabled = true;
      client.LingerState.LingerTime = LingerTimeInSeconds;

      prepareSocket(client.Client);

      var watch = Stopwatch.StartNew();

      var localEndPoint = client.Client.LocalEndPoint;
      var remoteEndPoint = client.Client.RemoteEndPoint;

      var exceptions = new List<ExceptionDispatchInfo>();
      var shutdownReason = QbservableProtocolShutdownReason.None;

      try
      {
        using (var stream = new NetworkStream(client.Client, ownsSocket: false))
        using (var protocol = await NegotiateServerAsync(capturedId.Value, stream, formatterFactory(), options, cancel).ConfigureAwait(false))
        {
          capturedId.Value = protocol.ClientId;

          var provider = providerFactory(protocol);

          ReceivedConnection(idOverride: capturedId.Value);

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
          finally
          {
            shutdownReason = protocol.ShutdownReason;
          }

          var protocolExceptions = protocol.Exceptions;

          if (protocolExceptions != null)
          {
            foreach (var exception in protocolExceptions)
            {
              exceptions.Add(exception);
            }
          }
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

    private static async Task<IStreamQbservableProtocol> NegotiateServerAsync(object baseId, Stream stream, IRemotingFormatter formatter, QbservableServiceOptions serviceOptions, CancellationToken cancel)
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

    private static bool IsConnectedSafe(Socket socket)
    {
      try
      {
        // Yep, the Connected property can throw ObjectDisposedException, and it's undocumented.
        return socket != null && socket.Connected;
      }
      catch (ObjectDisposedException)
      {
        return false;
      }
    }

    private async void Shutdown(Socket socket, object id = null)
    {
      Contract.Requires(socket != null);

      try
      {
        if (IsConnectedSafe(socket))
        {
          if (IsServer)
          {
            Disconnecting(idOverride: id);
          }
          else
          {
            Stopping();
          }

          socket.Shutdown(SocketShutdown.Both);
        }
      }
      catch (ObjectDisposedException)
      {
      }
      catch (SocketException ex)
      {
        QactiveTraceSources.Qactive.TraceEvent(TraceEventType.Warning, 0, Id + " - " + ex);
      }
      finally
      {
        if (IsServer)
        {
          try
          {
            await DisconnectAsync(socket).ConfigureAwait(false);
#if TPL
            await Task.Delay(TimeSpan.FromSeconds(LingerTimeInSeconds)).ConfigureAwait(false);
#else
            await TaskEx.Delay(TimeSpan.FromSeconds(LingerTimeInSeconds)).ConfigureAwait(false);
#endif
          }
          catch (ObjectDisposedException)
          {
          }
          catch (SocketException ex)
          {
            QactiveTraceSources.Qactive.TraceEvent(TraceEventType.Warning, 0, Id + " - " + ex);
          }
          finally
          {
            socket.Close();
          }

          Disconnected(idOverride: id);
        }
        else
        {
          socket.Close();

          Stopped();
        }
      }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The SocketAsyncEventArgs instance is either disposed before returning or by the observable's Finally operator.")]
    private static async Task DisconnectAsync(Socket socket)
    {
      Contract.Requires(socket != null);

      SocketAsyncEventArgs e = null;
      try
      {
        e = new SocketAsyncEventArgs()
        {
          DisconnectReuseSocket = false
        };

        IConnectableObservable<SocketAsyncEventArgs> disconnected;
        IDisposable subscription;

        using (var completedSynchronously = new Subject<SocketAsyncEventArgs>())
        {
          disconnected = Observable.FromEventPattern<SocketAsyncEventArgs>(
            handler => e.Completed += handler,
            handler => e.Completed -= handler)
            .Select(e2 => e2.EventArgs)
            .Amb(completedSynchronously)
            .Take(1)
            .Finally(e.Dispose)
            .PublishLast();

          subscription = disconnected.Connect();

          if (!socket.DisconnectAsync(e))
          {
            completedSynchronously.OnNext(e);
          }
        }

#if ASYNCAWAIT
        await disconnected;
#else
        await disconnected.ToTask().ConfigureAwait(false);
#endif
      }
      catch
      {
        if (e != null)
        {
          e.Dispose();
        }

        throw;
      }
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

    private sealed class CapturedId
    {
      public CapturedId(object value)
      {
        Value = value;
      }

      public object Value { get; set; }
    }
  }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.WebSockets;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Runtime.ExceptionServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Qactive
{
  internal sealed class WebSocketQactiveProvider : QactiveProvider
  {
    private const string protocol = "Qactive";

    public Uri Uri { get; }

    protected override object Id
      => clientNumber.HasValue
       ? "C" + clientNumber.Value + " " + Uri
       : "S" + serverNumber.Value + " " + Uri;

    private static int lastServerNumber = -1;
    private static int lastClientNumber = -1;

    private readonly IWebSocketQactiveProviderTransportInitializer transportInitializer;
    private readonly Func<IRemotingFormatter> formatterFactory;
    private readonly Action<WebSocket> prepareSocket;
    private readonly int? serverNumber, clientNumber;
    private int lastServerClientNumber = -1;

    private WebSocketQactiveProvider(Uri uri, IWebSocketQactiveProviderTransportInitializer transportInitializer)
    {
      Contract.Requires(uri != null);

      Uri = uri;
      this.transportInitializer = transportInitializer;
      serverNumber = Interlocked.Increment(ref lastServerNumber);

      if (transportInitializer != null)
      {
        prepareSocket = transportInitializer.Prepare;
        formatterFactory = () => transportInitializer.CreateFormatter() ?? WebSocketQactiveDefaults.CreateDefaultFormatter();
      }
      else
      {
        prepareSocket = Nop.Action;
        formatterFactory = WebSocketQactiveDefaults.CreateDefaultFormatter;
      }
    }

    private WebSocketQactiveProvider(Type sourceType, Uri uri, Action<WebSocket> prepareSocket, IRemotingFormatter formatter, LocalEvaluator localEvaluator)
      : base(sourceType, localEvaluator)
    {
      Contract.Requires(sourceType != null);
      Contract.Requires(uri != null);
      Contract.Requires(prepareSocket != null);
      Contract.Requires(formatter != null);
      Contract.Requires(localEvaluator != null);

      Uri = uri;
      clientNumber = Interlocked.Increment(ref lastClientNumber);
      this.prepareSocket = prepareSocket;
      formatterFactory = new ConstantFormatterFactory(formatter).GetFormatter;
    }

    private WebSocketQactiveProvider(Type sourceType, Uri uri, Action<WebSocket> prepareSocket, IRemotingFormatter formatter, LocalEvaluator localEvaluator, object argument)
      : base(sourceType, localEvaluator, argument)
    {
      Contract.Requires(sourceType != null);
      Contract.Requires(uri != null);
      Contract.Requires(prepareSocket != null);
      Contract.Requires(formatter != null);
      Contract.Requires(localEvaluator != null);

      Uri = uri;
      clientNumber = Interlocked.Increment(ref lastClientNumber);
      this.prepareSocket = prepareSocket;
      formatterFactory = new ConstantFormatterFactory(formatter).GetFormatter;
    }

    [ContractInvariantMethod]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
    private void ObjectInvariant()
    {
      Contract.Invariant(Uri != null);
      Contract.Invariant(formatterFactory != null);
      Contract.Invariant(prepareSocket != null);
    }

    public static WebSocketQactiveProvider Client(Type sourceType, Uri uri, Action<WebSocket> prepareSocket, IRemotingFormatter formatter, LocalEvaluator localEvaluator)
    {
      Contract.Requires(sourceType != null);
      Contract.Requires(uri != null);
      Contract.Requires(prepareSocket != null);
      Contract.Requires(formatter != null);
      Contract.Requires(localEvaluator != null);
      Contract.Ensures(Contract.Result<WebSocketQactiveProvider>() != null);

      return new WebSocketQactiveProvider(sourceType, uri, prepareSocket, formatter, localEvaluator);
    }

    public static WebSocketQactiveProvider Client(Type sourceType, Uri uri, Action<WebSocket> prepareSocket, IRemotingFormatter formatter, LocalEvaluator localEvaluator, object argument)
    {
      Contract.Requires(sourceType != null);
      Contract.Requires(uri != null);
      Contract.Requires(prepareSocket != null);
      Contract.Requires(formatter != null);
      Contract.Requires(localEvaluator != null);
      Contract.Ensures(Contract.Result<WebSocketQactiveProvider>() != null);

      return new WebSocketQactiveProvider(sourceType, uri, prepareSocket, formatter, localEvaluator, argument);
    }

    public static WebSocketQactiveProvider Server(Uri uri, IWebSocketQactiveProviderTransportInitializer transportInitializer = null)
    {
      Contract.Requires(uri != null);
      Contract.Ensures(Contract.Result<WebSocketQactiveProvider>() != null);

      return new WebSocketQactiveProvider(uri, transportInitializer);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Seems as simple as it's going to get.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The SocketAsyncEventArgs instance is either disposed before returning or by the observable's Finally operator.")]
    public override IObservable<TResult> Connect<TResult>(Func<IQbservableProtocol, Expression> prepareExpression)
    {
      ClientWebSocket socket = null;
      try
      {
        socket = new ClientWebSocket();
        socket.Options.AddSubProtocol(protocol);

        prepareSocket(socket);

        Starting();

        return Observable.Using(
            () => socket,
            _ => (from __ in Observable.StartAsync(cancel => socket.ConnectAsync(Uri, cancel)).Do(__ => Started())
                  let capturedShutdownReason = new CapturedShutdownReason()
                  from result in Observable.Create<TResult>(
                                  innerObserver =>
                                  {
                                    var cancel = new CancellationDisposable();

                                    var s = Observable.Using(
                                      () => new WebSocketStream(socket),
                                      stream => GetObservable<TResult>(stream, prepareExpression, capturedShutdownReason, cancel.Token))
                                      .SubscribeSafe(innerObserver);

                                    return new CompositeDisposable(s, cancel);
                                  })
                                  .Finally(() => Shutdown(socket, capturedShutdownReason.Value))
                  select result));
      }
      catch
      {
        if (socket != null)
        {
          socket.Dispose();
        }

        throw;
      }
    }

    private IObservable<TResult> GetObservable<TResult>(Stream stream, Func<IQbservableProtocol, Expression> prepareExpression, CapturedShutdownReason capturedShutdownReason, CancellationToken cancel)
    {
      Contract.Requires(stream != null);
      Contract.Requires(prepareExpression != null);
      Contract.Ensures(Contract.Result<IObservable<TResult>>() != null);

      return from protocol in NegotiateClientAsync(stream, formatterFactory(), cancel).ToObservable()
             from result in protocol
              .ExecuteClient<TResult>(prepareExpression(protocol), Argument)
              .Finally(() =>
              {
                capturedShutdownReason.Value = protocol.ShutdownReason;
                protocol.Dispose();
              })
             select result;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Seems as simple as it's going to get.")]
    public override IObservable<ClientTermination> Listen(QbservableServiceOptions options, Func<IQbservableProtocol, IParameterizedQbservableProvider> providerFactory)
    {
      var l = new HttpListener()
      {
        Prefixes =
        {
          Uri.ToString()
        }
      };

      return from listener in Observable.Return(l)
             .Do(listener =>
             {
               Starting();

               listener.Start();

               if (transportInitializer != null)
               {
                 transportInitializer.StartedListener(serverNumber.Value, Uri);
               }

               Started();
             })
             from context in Observable.FromAsync(listener.GetContextAsync)
                                       .SelectMany(context => context.AcceptWebSocketAsync(protocol))
                                       .Repeat()
             .Finally(() =>
             {
               Stopping();

               listener.Stop();

               if (transportInitializer != null)
               {
                 transportInitializer.StoppedListener(serverNumber.Value, Uri);
               }

               Stopped();
             })
             let capturedId = new CapturedId(Id + " C" + Interlocked.Increment(ref lastServerClientNumber) + " " + context.Origin)
             let capturedShutdownReason = new CapturedShutdownReason()
             from termination in Observable.StartAsync(cancel => AcceptAsync(context, capturedId, capturedShutdownReason, options, providerFactory, cancel))
             .Finally(() => Shutdown(context.WebSocket, capturedShutdownReason.Value, capturedId.Value))
             select termination;
    }

    private async Task<WebSocketClientTermination> AcceptAsync(
      HttpListenerWebSocketContext context,
      CapturedId capturedId,
      CapturedShutdownReason capturedShutdownReason,
      QbservableServiceOptions options,
      Func<IQbservableProtocol, IParameterizedQbservableProvider> providerFactory,
      CancellationToken cancel)
    {
      Contract.Requires(context != null);
      Contract.Requires(capturedId != null);
      Contract.Requires(capturedShutdownReason != null);
      Contract.Requires(options != null);
      Contract.Requires(providerFactory != null);

      ReceivingConnection(idOverride: capturedId.Value);

      prepareSocket(context.WebSocket);

      var watch = Stopwatch.StartNew();

      var exceptions = new List<ExceptionDispatchInfo>();
      var shutdownReason = QbservableProtocolShutdownReason.None;

      try
      {
        using (var stream = new WebSocketStream(context.WebSocket))
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
      finally
      {
        capturedShutdownReason.Value = shutdownReason;
      }

      return new WebSocketClientTermination(Uri, context.Origin, watch.Elapsed, shutdownReason, exceptions);
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

    private static bool IsConnectedSafe(WebSocket socket)
    {
      try
      {
        // Socket.Connected property can throw ObjectDisposedException, and it's undocumented.
        // I don't know whether that's true for WebSocket.State, but I'm using the same code here just to be safe.
        return socket != null && socket.State == WebSocketState.Open;
      }
      catch (ObjectDisposedException)
      {
        return false;
      }
    }

    private async void Shutdown(WebSocket socket, QbservableProtocolShutdownReason reason, object id = null)
    {
      Contract.Requires(socket != null);

      try
      {
        if (IsConnectedSafe(socket))
        {
          if (IsServer)
          {
            Disconnecting(idOverride: id);

            await socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, reason.ToString(), CancellationToken.None).ConfigureAwait(false);
          }
          else
          {
            Stopping();

            await socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, reason.ToString(), CancellationToken.None).ConfigureAwait(false);
          }
        }
      }
      catch (ObjectDisposedException)
      {
      }
      catch (WebSocketException ex)
      {
        QactiveTraceSources.Qactive.TraceEvent(TraceEventType.Warning, 0, Id + " - " + ex);
      }
      finally
      {
        socket.Dispose();

        if (IsServer)
        {
          Disconnected(idOverride: id);
        }
        else
        {
          Stopped();
        }
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

    private sealed class CapturedShutdownReason
    {
      public QbservableProtocolShutdownReason Value { get; set; }
    }
  }
}

using System;
using System.IO;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Runtime.Remoting.Messaging;
using System.Threading;

namespace Qactive
{
  internal sealed class TcpClientQbservableProvider : ClientQbservableProvider
  {
    public IPEndPoint EndPoint { get; }

    public IRemotingFormatter Formatter { get; }

    public TcpClientQbservableProvider(Type sourceType, IPEndPoint endPoint, IRemotingFormatter formatter, LocalEvaluator localEvaluator)
      : base(sourceType, localEvaluator)
    {
      EndPoint = endPoint;
      Formatter = formatter;
    }

    public TcpClientQbservableProvider(Type sourceType, IPEndPoint endPoint, IRemotingFormatter formatter, LocalEvaluator localEvaluator, object argument)
      : base(sourceType, localEvaluator, argument)
    {
      EndPoint = endPoint;
      Formatter = formatter;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The SocketAsyncEventArgs instance is either disposed before returning or by the observable's Finally operator.")]
    public override IObservable<TResult> GetConnections<TResult>(Func<QbservableProtocol, Expression> prepareExpression)
    {
      SocketAsyncEventArgs e = null;
      Socket socket = null;
      try
      {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        e = new SocketAsyncEventArgs()
        {
          RemoteEndPoint = EndPoint
        };

        IConnectableObservable<Socket> connected;
        IDisposable subscription;

        using (var completedSynchronously = new Subject<SocketAsyncEventArgs>())
        {
          connected = Observable.FromEventPattern<SocketAsyncEventArgs>(
            handler => e.Completed += handler,
            handler => e.Completed -= handler)
            .Select(e2 => e2.EventArgs)
            .Amb(completedSynchronously)
            .Take(1)
            .Select(e2 => e2.ConnectSocket)
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
            _ => (from connectedSocket in connected
                  from result in
                    Observable.Create<TResult>(
                      innerObserver =>
                      {
                        var cancel = new CancellationDisposable();

                        var s = Observable.Using(
                          () => new NetworkStream(connectedSocket, ownsSocket: false),
                          stream => ReadObservable<TResult>(stream, prepareExpression, cancel.Token))
                          .Subscribe(innerObserver);

                        return new CompositeDisposable(s, cancel);
                      })
                      .Finally(connectedSocket.Close)
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

    private IObservable<TResult> ReadObservable<TResult>(Stream stream, Func<QbservableProtocol, Expression> prepareExpression, CancellationToken cancel) =>
      from protocol in QbservableProtocol.NegotiateClientAsync(stream, Formatter, cancel).ToObservable()
      from result in protocol
       .ExecuteClient<TResult>(prepareExpression(protocol), Argument)
       .Finally(protocol.Dispose)
      select result;
  }
}

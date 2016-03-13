using System;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading;

namespace Qactive
{
  internal sealed class TcpClientQuery<TResult> : QbservableBase<TResult, TcpClientQbservableProvider>
  {
    private readonly object argument;

    public TcpClientQuery(TcpClientQbservableProvider provider, object argument)
      : base(provider)
    {
      this.argument = argument;
    }

    public TcpClientQuery(TcpClientQbservableProvider provider, object argument, Expression expression)
      : base(provider, expression)
    {
      this.argument = argument;
    }

    protected override IDisposable SubscribeCore(IObserver<TResult> observer)
    {
      Socket socket = null;
      try
      {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        var e = new SocketAsyncEventArgs()
        {
          RemoteEndPoint = Provider.EndPoint
        };

        var completedSynchronously = new Subject<SocketAsyncEventArgs>();
        var connected = Observable.FromEventPattern<SocketAsyncEventArgs>(
          handler => e.Completed += handler,
          handler => e.Completed -= handler)
          .Select(e2 => e2.EventArgs)
          .Amb(completedSynchronously)
          .Take(1)
          .Select(e2 => e2.ConnectSocket)
          .PublishLast();

        var subscription = connected.Connect();

        if (!socket.ConnectAsync(e))
        {
          completedSynchronously.OnNext(e);
        }

        return new CompositeDisposable(
          subscription,
          Observable.Using(
            () => socket,
            _ => (from connectedSocket in connected
                  from result in
                    Observable.Create<TResult>(
                      innerObserver =>
                      {
                        var cancel = new CancellationDisposable();

                        var s = Observable.Using(
                          () => new NetworkStream(connectedSocket, ownsSocket: false),
                          stream => ReadObservable(stream, cancel.Token))
                          .Subscribe(innerObserver);

                        return new CompositeDisposable(s, cancel);
                      })
                      .Finally(connectedSocket.Close)
                  select result))
          .Subscribe(observer));
      }
      catch (Exception)
      {
        if (socket != null)
        {
          socket.Dispose();
        }
        throw;
      }
    }

    private IObservable<TResult> ReadObservable(NetworkStream stream, CancellationToken cancel)
    {
      return from protocol in QbservableProtocol.NegotiateClientAsync(stream, Provider.Formatter, cancel).ToObservable()
             from result in protocol
              .ExecuteClient<TResult>(PrepareExpression(protocol), argument)
              .Finally(protocol.Dispose)
             select result;
    }

    public Expression PrepareExpression(QbservableProtocol protocol)
    {
      QbservableProviderDiagnostics.DebugPrint(Expression, "TcpClientQuery Original Expression");

      if (!Expression.Type.IsGenericType
        || (Expression.Type.GetGenericTypeDefinition() != typeof(IQbservable<>)
          && Expression.Type.GetGenericTypeDefinition() != typeof(TcpClientQuery<>)))
      {
        throw new InvalidOperationException("The query must end as an IQbservable<T>.");
      }

      var visitor = ReplaceConstantsVisitor.CreateForGenericTypeByDefinition(
        typeof(TcpClientQuery<>),
        (_, actualType) => Activator.CreateInstance(typeof(QbservableSourcePlaceholder<>).MakeGenericType(actualType.GetGenericArguments()[0]), true),
        type => typeof(IQbservable<>).MakeGenericType(type.GetGenericArguments()[0]));

      var result = visitor.Visit(Expression);

      if (visitor.ReplacedConstants == 0)
      {
        throw new InvalidOperationException("A queryable observable service was not found in the query.");
      }

      var evaluator = Provider.LocalEvaluator;

      if (!evaluator.IsKnownType(Provider.SourceType))
      {
        evaluator.AddKnownType(Provider.SourceType);
      }

      var evaluationVisitor = new LocalEvaluationVisitor(evaluator, protocol);

      var preparedExpression = evaluationVisitor.Visit(result);

      QbservableProviderDiagnostics.DebugPrint(preparedExpression, "TcpClientQuery Rewritten Expression");

      return preparedExpression;
    }
  }
}
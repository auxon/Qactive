using System;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Runtime.Remoting.Messaging;

namespace Qactive
{
  public sealed class TcpQbservableClient<TSource>
  {
    private readonly IPEndPoint endPoint;
    private readonly IRemotingFormatter formatter;
    private readonly LocalEvaluator localEvaluator;

    public TcpQbservableClient(IPAddress address, int port)
      : this(new IPEndPoint(address, port))
    {
      Contract.Requires(address != null);
      Contract.Requires(port >= IPEndPoint.MinPort);
      Contract.Requires(port <= IPEndPoint.MaxPort);
    }

    public TcpQbservableClient(IPAddress address, int port, params Type[] knownTypes)
      : this(new IPEndPoint(address, port), knownTypes)
    {
      Contract.Requires(address != null);
      Contract.Requires(port >= IPEndPoint.MinPort);
      Contract.Requires(port <= IPEndPoint.MaxPort);
    }

    public TcpQbservableClient(IPAddress address, int port, LocalEvaluator localEvaluator)
      : this(new IPEndPoint(address, port), localEvaluator)
    {
      Contract.Requires(address != null);
      Contract.Requires(port >= IPEndPoint.MinPort);
      Contract.Requires(port <= IPEndPoint.MaxPort);
      Contract.Requires(localEvaluator != null);
    }

    public TcpQbservableClient(IPAddress address, int port, IRemotingFormatter formatter)
      : this(new IPEndPoint(address, port), formatter)
    {
      Contract.Requires(address != null);
      Contract.Requires(port >= IPEndPoint.MinPort);
      Contract.Requires(port <= IPEndPoint.MaxPort);
      Contract.Requires(formatter != null);
    }

    public TcpQbservableClient(IPAddress address, int port, IRemotingFormatter formatter, LocalEvaluator localEvaluator)
      : this(new IPEndPoint(address, port), formatter, localEvaluator)
    {
      Contract.Requires(address != null);
      Contract.Requires(port >= IPEndPoint.MinPort);
      Contract.Requires(port <= IPEndPoint.MaxPort);
      Contract.Requires(formatter != null);
      Contract.Requires(localEvaluator != null);
    }

    public TcpQbservableClient(IPAddress address, int port, IRemotingFormatter formatter, params Type[] knownTypes)
      : this(new IPEndPoint(address, port), formatter, knownTypes)
    {
      Contract.Requires(address != null);
      Contract.Requires(port >= IPEndPoint.MinPort);
      Contract.Requires(port <= IPEndPoint.MaxPort);
      Contract.Requires(formatter != null);
    }

    public TcpQbservableClient(IPEndPoint endPoint)
      : this(endPoint, TcpQactiveDefaults.CreateDefaultFormatter())
    {
      Contract.Requires(endPoint != null);
    }

    public TcpQbservableClient(IPEndPoint endPoint, params Type[] knownTypes)
      : this(endPoint, TcpQactiveDefaults.CreateDefaultFormatter(), knownTypes)
    {
      Contract.Requires(endPoint != null);
    }

    public TcpQbservableClient(IPEndPoint endPoint, LocalEvaluator localEvaluator)
      : this(endPoint, TcpQactiveDefaults.CreateDefaultFormatter(), localEvaluator)
    {
      Contract.Requires(endPoint != null);
      Contract.Requires(localEvaluator != null);
    }

    public TcpQbservableClient(IPEndPoint endPoint, IRemotingFormatter formatter)
      : this(endPoint, formatter, new ImmediateLocalEvaluator())
    {
      Contract.Requires(endPoint != null);
      Contract.Requires(formatter != null);
    }

    public TcpQbservableClient(IPEndPoint endPoint, IRemotingFormatter formatter, params Type[] knownTypes)
      : this(endPoint, formatter, new ImmediateLocalEvaluator(knownTypes))
    {
      Contract.Requires(endPoint != null);
      Contract.Requires(formatter != null);
    }

    public TcpQbservableClient(IPEndPoint endPoint, IRemotingFormatter formatter, LocalEvaluator localEvaluator)
    {
      Contract.Requires(endPoint != null);
      Contract.Requires(formatter != null);
      Contract.Requires(localEvaluator != null);

      this.endPoint = endPoint;
      this.formatter = formatter;
      this.localEvaluator = localEvaluator;
    }

    [ContractInvariantMethod]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
    private void ObjectInvariant()
    {
      Contract.Invariant(endPoint != null);
      Contract.Invariant(formatter != null);
      Contract.Invariant(localEvaluator != null);
    }

    public IQbservable<TSource> Query()
    {
      Contract.Ensures(Contract.Result<IQbservable<TSource>>() != null);

      return TcpQactiveProvider.Client(typeof(TSource), endPoint, Nop.Action, formatter, localEvaluator).CreateQuery<TSource>();
    }

    public IQbservable<TSource> Query(object argument)
    {
      Contract.Ensures(Contract.Result<IQbservable<TSource>>() != null);

      return TcpQactiveProvider.Client(typeof(TSource), endPoint, Nop.Action, formatter, localEvaluator, argument).CreateQuery<TSource>();
    }

    public IQbservable<TSource> Query(Action<Socket> prepareSocket)
    {
      Contract.Requires(prepareSocket != null);
      Contract.Ensures(Contract.Result<IQbservable<TSource>>() != null);

      return TcpQactiveProvider.Client(typeof(TSource), endPoint, prepareSocket, formatter, localEvaluator).CreateQuery<TSource>();
    }

    public IQbservable<TSource> Query(Action<Socket> prepareSocket, object argument)
    {
      Contract.Requires(prepareSocket != null);
      Contract.Ensures(Contract.Result<IQbservable<TSource>>() != null);

      return TcpQactiveProvider.Client(typeof(TSource), endPoint, prepareSocket, formatter, localEvaluator, argument).CreateQuery<TSource>();
    }
  }
}
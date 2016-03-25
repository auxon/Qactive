using System;
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
    }

    public TcpQbservableClient(IPAddress address, int port, params Type[] knownTypes)
      : this(new IPEndPoint(address, port), knownTypes)
    {
    }

    public TcpQbservableClient(IPAddress address, int port, LocalEvaluator localEvaluator)
      : this(new IPEndPoint(address, port), localEvaluator)
    {
    }

    public TcpQbservableClient(IPAddress address, int port, IRemotingFormatter formatter)
      : this(new IPEndPoint(address, port), formatter)
    {
    }

    public TcpQbservableClient(IPAddress address, int port, IRemotingFormatter formatter, LocalEvaluator localEvaluator)
      : this(new IPEndPoint(address, port), formatter, localEvaluator)
    {
    }

    public TcpQbservableClient(IPAddress address, int port, IRemotingFormatter formatter, params Type[] knownTypes)
      : this(new IPEndPoint(address, port), formatter, knownTypes)
    {
    }

    public TcpQbservableClient(IPEndPoint endPoint)
      : this(endPoint, TcpQactiveDefaults.CreateDefaultFormatter())
    {
    }

    public TcpQbservableClient(IPEndPoint endPoint, params Type[] knownTypes)
      : this(endPoint, TcpQactiveDefaults.CreateDefaultFormatter(), knownTypes)
    {
    }

    public TcpQbservableClient(IPEndPoint endPoint, LocalEvaluator localEvaluator)
      : this(endPoint, TcpQactiveDefaults.CreateDefaultFormatter(), localEvaluator)
    {
    }

    public TcpQbservableClient(IPEndPoint endPoint, IRemotingFormatter formatter)
      : this(endPoint, formatter, new ImmediateLocalEvaluator())
    {
    }

    public TcpQbservableClient(IPEndPoint endPoint, IRemotingFormatter formatter, params Type[] knownTypes)
      : this(endPoint, formatter, new ImmediateLocalEvaluator(knownTypes))
    {
    }

    public TcpQbservableClient(IPEndPoint endPoint, IRemotingFormatter formatter, LocalEvaluator localEvaluator)
    {
      this.endPoint = endPoint;
      this.formatter = formatter;
      this.localEvaluator = localEvaluator;
    }

    public IQbservable<TSource> Query()
    {
      return TcpQactiveProvider.Client(typeof(TSource), endPoint, Nop.Action, formatter, localEvaluator).CreateQuery<TSource>();
    }

    public IQbservable<TSource> Query(object argument)
    {
      return TcpQactiveProvider.Client(typeof(TSource), endPoint, Nop.Action, formatter, localEvaluator, argument).CreateQuery<TSource>();
    }

    public IQbservable<TSource> Query(Action<Socket> prepareSocket)
    {
      return TcpQactiveProvider.Client(typeof(TSource), endPoint, prepareSocket, formatter, localEvaluator).CreateQuery<TSource>();
    }

    public IQbservable<TSource> Query(Action<Socket> prepareSocket, object argument)
    {
      return TcpQactiveProvider.Client(typeof(TSource), endPoint, prepareSocket, formatter, localEvaluator, argument).CreateQuery<TSource>();
    }
  }
}
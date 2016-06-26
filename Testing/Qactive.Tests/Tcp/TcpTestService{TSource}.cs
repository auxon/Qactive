using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization.Formatters.Binary;

namespace Qactive.Tests.Tcp
{
  internal sealed class TcpTestService<TSource> : TestServiceBase<TSource>, ITcpQactiveProviderTransportInitializer
  {
    private static readonly IPEndPoint DefaultEndPoint = new IPEndPoint(IPAddress.Loopback, 0);

    private readonly QbservableServiceOptions options;
    private readonly Type[] knownTypes;
    private IPEndPoint endPoint;

    public TcpTestService(QbservableServiceOptions options, Type[] knownTypes, params Notification<TSource>[] notifications)
      : this(DefaultEndPoint, options, knownTypes, notifications)
    {
    }

    public TcpTestService(QbservableServiceOptions options, Type[] knownTypes, IObservable<TSource> source)
      : this(DefaultEndPoint, options, knownTypes, source)
    {
    }

    public TcpTestService(IPEndPoint endPoint, QbservableServiceOptions options, Type[] knownTypes, params Notification<TSource>[] notifications)
      : base(notifications)
    {
      this.endPoint = endPoint;
      this.options = options;
      this.knownTypes = (knownTypes ?? Enumerable.Empty<Type>()).Concat(new[] { typeof(ObservableExtensions) }).ToArray();
    }

    public TcpTestService(IPEndPoint endPoint, QbservableServiceOptions options, Type[] knownTypes, IObservable<TSource> source)
      : base(source)
    {
      this.endPoint = endPoint;
      this.options = options;
      this.knownTypes = (knownTypes ?? Enumerable.Empty<Type>()).Concat(new[] { typeof(ObservableExtensions) }).ToArray();
    }

    protected override IObservable<ClientTermination> ServeQbservable(IObservable<TSource> source)
      => source.ServeQbservableTcp(endPoint, this, options);

    protected override IQbservable<TSource> CreateQuery()
      => new TcpQbservableClient<TSource>(endPoint, knownTypes).Query(Prepare);

    public void StartedListener(int serverNumber, EndPoint endPoint)
    {
      this.endPoint = (IPEndPoint)endPoint;
    }

    public void StoppedListener(int serverNumber, EndPoint endPoint)
    {
    }

    public void Prepare(Socket socket)
      => socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);

    public IRemotingFormatter CreateFormatter()
      => new BinaryFormatter();
  }
}

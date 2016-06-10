using System;
using System.Net;
using System.Reactive;
using System.Reactive.Linq;

namespace Qactive.Tests.Tcp
{
  internal sealed class TcpTestService<TSource> : TestServiceBase<TSource>
  {
    private static readonly IPEndPoint DefaultEndPoint = new IPEndPoint(IPAddress.Loopback, 24142);

    private readonly QbservableServiceOptions options;
    private readonly Type[] knownTypes;
    private readonly IPEndPoint endPoint;

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
      this.knownTypes = knownTypes;
    }

    public TcpTestService(IPEndPoint endPoint, QbservableServiceOptions options, Type[] knownTypes, IObservable<TSource> source)
      : base(source)
    {
      this.endPoint = endPoint;
      this.options = options;
      this.knownTypes = knownTypes;
    }

    protected override IObservable<ClientTermination> ServeQbservable(IObservable<TSource> source)
      => source.ServeQbservableTcp(endPoint, options);

    protected override IQbservable<TSource> CreateQuery()
      => new TcpQbservableClient<TSource>(endPoint, knownTypes).Query();
  }
}

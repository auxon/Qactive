using System;
using System.Linq;
using System.Net.WebSockets;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization.Formatters.Binary;

namespace Qactive.Tests.WebSockets
{
  internal sealed class WebSocketTestService<TSource> : TestServiceBase<TSource>, IWebSocketQactiveProviderTransportInitializer
  {
    private static readonly Uri DefaultUri = new Uri("http://localhost:49491/test/");
    private static readonly string ClientUriPrefix = "ws://";

    private readonly QbservableServiceOptions options;
    private readonly Type[] knownTypes;
    private Uri uri;

    private Uri HostUri => new Uri(new Uri(ClientUriPrefix + uri.Host + ":" + uri.Port), uri.PathAndQuery);

    public WebSocketTestService(QbservableServiceOptions options, Type[] knownTypes, params Notification<TSource>[] notifications)
      : this(DefaultUri, options, knownTypes, notifications)
    {
    }

    public WebSocketTestService(QbservableServiceOptions options, Type[] knownTypes, IObservable<TSource> source)
      : this(DefaultUri, options, knownTypes, source)
    {
    }

    public WebSocketTestService(Uri uri, QbservableServiceOptions options, Type[] knownTypes, params Notification<TSource>[] notifications)
      : base(notifications)
    {
      this.uri = uri;
      this.options = options;
      this.knownTypes = (knownTypes ?? Enumerable.Empty<Type>()).Concat(new[] { typeof(ObservableExtensions) }).ToArray();
    }

    public WebSocketTestService(Uri uri, QbservableServiceOptions options, Type[] knownTypes, IObservable<TSource> source)
      : base(source)
    {
      this.uri = uri;
      this.options = options;
      this.knownTypes = (knownTypes ?? Enumerable.Empty<Type>()).Concat(new[] { typeof(ObservableExtensions) }).ToArray();
    }

    protected override IObservable<ClientTermination> ServeQbservable(IObservable<TSource> source)
      => source.ServeQbservableWebSocket(uri, this, options);

    protected override IQbservable<TSource> CreateQuery()
      => new WebSocketQbservableClient<TSource>(HostUri, knownTypes).Query(Prepare);

    public void StartedListener(int serverNumber, Uri uri)
    {
      this.uri = uri;
    }

    public void StoppedListener(int serverNumber, Uri uri)
    {
    }

    public void Prepare(WebSocket socket)
    {
    }

    public IRemotingFormatter CreateFormatter()
      => new BinaryFormatter();
  }
}

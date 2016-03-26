using System;
using System.Net;
using System.Reactive.Linq;

namespace Qactive
{
  public static class Qbservable2
  {
    public static IObservable<ClientTermination> ServeQbservableTcp<TSource>(
      this IObservable<TSource> source,
      IPEndPoint endPoint)
    {
      return TcpQbservableServer.CreateService<object, TSource>(endPoint, _ => source);
    }

    public static IObservable<ClientTermination> ServeQbservableTcp<TSource>(
      this IObservable<TSource> source,
      IPEndPoint endPoint,
      QbservableServiceOptions options)
    {
      return TcpQbservableServer.CreateService<object, TSource>(endPoint, options, _ => source);
    }

    public static IObservable<ClientTermination> ServeQbservableTcp<TSource>(
      this IObservable<TSource> source,
      IPEndPoint endPoint,
      ITcpQactiveProviderTransportInitializer transportInitializer)
    {
      return TcpQbservableServer.CreateService<object, TSource>(endPoint, transportInitializer, _ => source);
    }

    public static IObservable<ClientTermination> ServeQbservableTcp<TSource>(
      this IObservable<TSource> source,
      IPEndPoint endPoint,
      ITcpQactiveProviderTransportInitializer transportInitializer,
      QbservableServiceOptions options)
    {
      return TcpQbservableServer.CreateService<object, TSource>(endPoint, transportInitializer, options, _ => source);
    }

    public static IObservable<ClientTermination> ServeTcp<TSource>(
      this IQbservable<TSource> source,
      IPEndPoint endPoint)
    {
      return TcpQbservableServer.CreateService<object, TSource>(endPoint, _ => source);
    }

    public static IObservable<ClientTermination> ServeTcp<TSource>(
      this IQbservable<TSource> source,
      IPEndPoint endPoint,
      QbservableServiceOptions options)
    {
      return TcpQbservableServer.CreateService<object, TSource>(endPoint, options, _ => source);
    }

    public static IObservable<ClientTermination> ServeTcp<TSource>(
      this IQbservable<TSource> source,
      IPEndPoint endPoint,
      ITcpQactiveProviderTransportInitializer transportInitializer)
    {
      return TcpQbservableServer.CreateService<object, TSource>(endPoint, transportInitializer, _ => source);
    }

    public static IObservable<ClientTermination> ServeTcp<TSource>(
      this IQbservable<TSource> source,
      IPEndPoint endPoint,
      ITcpQactiveProviderTransportInitializer transportInitializer,
      QbservableServiceOptions options)
    {
      return TcpQbservableServer.CreateService<object, TSource>(endPoint, transportInitializer, options, _ => source);
    }
  }
}
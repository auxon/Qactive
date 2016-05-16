using System;
using System.Diagnostics.Contracts;
using System.Net;
using System.Reactive.Linq;

namespace Qactive
{
  public static class TcpQbservable
  {
    public static IObservable<ClientTermination> ServeQbservableTcp<TSource>(
      this IObservable<TSource> source,
      IPEndPoint endPoint)
    {
      Contract.Requires(source != null);
      Contract.Requires(endPoint != null);
      Contract.Ensures(Contract.Result<IObservable<ClientTermination>>() != null);

      return TcpQbservableServer.CreateService<object, TSource>(endPoint, _ => source);
    }

    public static IObservable<ClientTermination> ServeQbservableTcp<TSource>(
      this IObservable<TSource> source,
      IPEndPoint endPoint,
      QbservableServiceOptions options)
    {
      Contract.Requires(source != null);
      Contract.Requires(endPoint != null);
      Contract.Requires(options != null);
      Contract.Ensures(Contract.Result<IObservable<ClientTermination>>() != null);

      return TcpQbservableServer.CreateService<object, TSource>(endPoint, options, _ => source);
    }

    public static IObservable<ClientTermination> ServeQbservableTcp<TSource>(
      this IObservable<TSource> source,
      IPEndPoint endPoint,
      ITcpQactiveProviderTransportInitializer transportInitializer)
    {
      Contract.Requires(source != null);
      Contract.Requires(endPoint != null);
      Contract.Requires(transportInitializer != null);
      Contract.Ensures(Contract.Result<IObservable<ClientTermination>>() != null);

      return TcpQbservableServer.CreateService<object, TSource>(endPoint, transportInitializer, _ => source);
    }

    public static IObservable<ClientTermination> ServeQbservableTcp<TSource>(
      this IObservable<TSource> source,
      IPEndPoint endPoint,
      ITcpQactiveProviderTransportInitializer transportInitializer,
      QbservableServiceOptions options)
    {
      Contract.Requires(source != null);
      Contract.Requires(endPoint != null);
      Contract.Requires(transportInitializer != null);
      Contract.Requires(options != null);
      Contract.Ensures(Contract.Result<IObservable<ClientTermination>>() != null);

      return TcpQbservableServer.CreateService<object, TSource>(endPoint, transportInitializer, options, _ => source);
    }

    public static IObservable<ClientTermination> ServeTcp<TSource>(
      this IQbservable<TSource> source,
      IPEndPoint endPoint)
    {
      Contract.Requires(source != null);
      Contract.Requires(endPoint != null);
      Contract.Ensures(Contract.Result<IObservable<ClientTermination>>() != null);

      return TcpQbservableServer.CreateService<object, TSource>(endPoint, _ => source);
    }

    public static IObservable<ClientTermination> ServeTcp<TSource>(
      this IQbservable<TSource> source,
      IPEndPoint endPoint,
      QbservableServiceOptions options)
    {
      Contract.Requires(source != null);
      Contract.Requires(endPoint != null);
      Contract.Requires(options != null);
      Contract.Ensures(Contract.Result<IObservable<ClientTermination>>() != null);

      return TcpQbservableServer.CreateService<object, TSource>(endPoint, options, _ => source);
    }

    public static IObservable<ClientTermination> ServeTcp<TSource>(
      this IQbservable<TSource> source,
      IPEndPoint endPoint,
      ITcpQactiveProviderTransportInitializer transportInitializer)
    {
      Contract.Requires(source != null);
      Contract.Requires(endPoint != null);
      Contract.Requires(transportInitializer != null);
      Contract.Ensures(Contract.Result<IObservable<ClientTermination>>() != null);

      return TcpQbservableServer.CreateService<object, TSource>(endPoint, transportInitializer, _ => source);
    }

    public static IObservable<ClientTermination> ServeTcp<TSource>(
      this IQbservable<TSource> source,
      IPEndPoint endPoint,
      ITcpQactiveProviderTransportInitializer transportInitializer,
      QbservableServiceOptions options)
    {
      Contract.Requires(source != null);
      Contract.Requires(endPoint != null);
      Contract.Requires(transportInitializer != null);
      Contract.Requires(options != null);
      Contract.Ensures(Contract.Result<IObservable<ClientTermination>>() != null);

      return TcpQbservableServer.CreateService<object, TSource>(endPoint, transportInitializer, options, _ => source);
    }
  }
}
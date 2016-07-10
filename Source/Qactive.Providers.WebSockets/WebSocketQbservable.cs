using System;
using System.Diagnostics.Contracts;
using System.Reactive.Linq;

namespace Qactive
{
  public static class WebSocketQbservable
  {
    public static IObservable<ClientTermination> ServeQbservableWebSocket<TSource>(
      this IObservable<TSource> source,
      Uri uri)
    {
      Contract.Requires(source != null);
      Contract.Requires(uri != null);
      Contract.Ensures(Contract.Result<IObservable<ClientTermination>>() != null);

      return WebSocketQbservableServer.CreateService<object, TSource>(uri, _ => source);
    }

    public static IObservable<ClientTermination> ServeQbservableWebSocket<TSource>(
      this IObservable<TSource> source,
      Uri uri,
      QbservableServiceOptions options)
    {
      Contract.Requires(source != null);
      Contract.Requires(uri != null);
      Contract.Requires(options != null);
      Contract.Ensures(Contract.Result<IObservable<ClientTermination>>() != null);

      return WebSocketQbservableServer.CreateService<object, TSource>(uri, options, _ => source);
    }

    public static IObservable<ClientTermination> ServeQbservableWebSocket<TSource>(
      this IObservable<TSource> source,
      Uri uri,
      IWebSocketQactiveProviderTransportInitializer transportInitializer)
    {
      Contract.Requires(source != null);
      Contract.Requires(uri != null);
      Contract.Requires(transportInitializer != null);
      Contract.Ensures(Contract.Result<IObservable<ClientTermination>>() != null);

      return WebSocketQbservableServer.CreateService<object, TSource>(uri, transportInitializer, _ => source);
    }

    public static IObservable<ClientTermination> ServeQbservableWebSocket<TSource>(
      this IObservable<TSource> source,
      Uri uri,
      IWebSocketQactiveProviderTransportInitializer transportInitializer,
      QbservableServiceOptions options)
    {
      Contract.Requires(source != null);
      Contract.Requires(uri != null);
      Contract.Requires(transportInitializer != null);
      Contract.Requires(options != null);
      Contract.Ensures(Contract.Result<IObservable<ClientTermination>>() != null);

      return WebSocketQbservableServer.CreateService<object, TSource>(uri, transportInitializer, options, _ => source);
    }

    public static IObservable<ClientTermination> ServeWebSocket<TSource>(
      this IQbservable<TSource> source,
      Uri uri)
    {
      Contract.Requires(source != null);
      Contract.Requires(uri != null);
      Contract.Ensures(Contract.Result<IObservable<ClientTermination>>() != null);

      return WebSocketQbservableServer.CreateService<object, TSource>(uri, _ => source);
    }

    public static IObservable<ClientTermination> ServeWebSocket<TSource>(
      this IQbservable<TSource> source,
      Uri uri,
      QbservableServiceOptions options)
    {
      Contract.Requires(source != null);
      Contract.Requires(uri != null);
      Contract.Requires(options != null);
      Contract.Ensures(Contract.Result<IObservable<ClientTermination>>() != null);

      return WebSocketQbservableServer.CreateService<object, TSource>(uri, options, _ => source);
    }

    public static IObservable<ClientTermination> ServeWebSocket<TSource>(
      this IQbservable<TSource> source,
      Uri uri,
      IWebSocketQactiveProviderTransportInitializer transportInitializer)
    {
      Contract.Requires(source != null);
      Contract.Requires(uri != null);
      Contract.Requires(transportInitializer != null);
      Contract.Ensures(Contract.Result<IObservable<ClientTermination>>() != null);

      return WebSocketQbservableServer.CreateService<object, TSource>(uri, transportInitializer, _ => source);
    }

    public static IObservable<ClientTermination> ServeWebSocket<TSource>(
      this IQbservable<TSource> source,
      Uri uri,
      IWebSocketQactiveProviderTransportInitializer transportInitializer,
      QbservableServiceOptions options)
    {
      Contract.Requires(source != null);
      Contract.Requires(uri != null);
      Contract.Requires(transportInitializer != null);
      Contract.Requires(options != null);
      Contract.Ensures(Contract.Result<IObservable<ClientTermination>>() != null);

      return WebSocketQbservableServer.CreateService<object, TSource>(uri, transportInitializer, options, _ => source);
    }
  }
}
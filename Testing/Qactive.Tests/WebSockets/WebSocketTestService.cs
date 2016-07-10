using System;
using System.Reactive;

namespace Qactive.Tests.WebSockets
{
  internal static class WebSocketTestService
  {
    public static readonly QbservableServiceOptions DefaultOptions = TestService.DefaultOptions;
    public static readonly QbservableServiceOptions UnrestrictedOptions = TestService.UnrestrictedOptions;

    /// <summary>
    /// Same as <see cref="UnrestrictedOptions"/> except that <see cref="QbservableServiceOptions.EnableDuplex"/> is false.
    /// </summary>
    public static readonly QbservableServiceOptions UnrestrictedExpressionsOptions = TestService.UnrestrictedExpressionsOptions;

    public static WebSocketTestService<TSource> Create<TSource>(params Notification<TSource>[] notifications)
      => new WebSocketTestService<TSource>(DefaultOptions, (Type[])null, notifications);

    public static WebSocketTestService<TSource> Create<TSource>(Type[] knownTypes, params Notification<TSource>[] notifications)
      => new WebSocketTestService<TSource>(DefaultOptions, knownTypes, notifications);

    public static WebSocketTestService<TSource> Create<TSource>(QbservableServiceOptions options, params Notification<TSource>[] notifications)
      => new WebSocketTestService<TSource>(options, (Type[])null, notifications);

    public static WebSocketTestService<TSource> Create<TSource>(QbservableServiceOptions options, Type[] knownTypes, params Notification<TSource>[] notifications)
      => new WebSocketTestService<TSource>(options, knownTypes, notifications);

    public static WebSocketTestService<TSource> Create<TSource>(IObservable<TSource> source)
      => new WebSocketTestService<TSource>(DefaultOptions, (Type[])null, source);

    public static WebSocketTestService<TSource> Create<TSource>(Type[] knownTypes, IObservable<TSource> source)
      => new WebSocketTestService<TSource>(DefaultOptions, knownTypes, source);

    public static WebSocketTestService<TSource> Create<TSource>(QbservableServiceOptions options, IObservable<TSource> source)
      => new WebSocketTestService<TSource>(options, (Type[])null, source);

    public static WebSocketTestService<TSource> Create<TSource>(QbservableServiceOptions options, Type[] knownTypes, IObservable<TSource> source)
      => new WebSocketTestService<TSource>(options, knownTypes, source);
  }
}

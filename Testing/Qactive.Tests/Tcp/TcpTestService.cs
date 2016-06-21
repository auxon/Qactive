using System;
using System.Reactive;

namespace Qactive.Tests.Tcp
{
  internal static class TcpTestService
  {
    public static readonly QbservableServiceOptions DefaultOptions = TestService.DefaultOptions;
    public static readonly QbservableServiceOptions UnrestrictedOptions = TestService.UnrestrictedOptions;

    /// <summary>
    /// Same as <see cref="UnrestrictedOptions"/> except that <see cref="QbservableServiceOptions.EnableDuplex"/> is false.
    /// </summary>
    public static readonly QbservableServiceOptions UnrestrictedExpressionsOptions = TestService.UnrestrictedExpressionsOptions;

    public static TcpTestService<TSource> Create<TSource>(params Notification<TSource>[] notifications)
      => new TcpTestService<TSource>(DefaultOptions, (Type[])null, notifications);

    public static TcpTestService<TSource> Create<TSource>(Type[] knownTypes, params Notification<TSource>[] notifications)
      => new TcpTestService<TSource>(DefaultOptions, knownTypes, notifications);

    public static TcpTestService<TSource> Create<TSource>(QbservableServiceOptions options, params Notification<TSource>[] notifications)
      => new TcpTestService<TSource>(options, (Type[])null, notifications);

    public static TcpTestService<TSource> Create<TSource>(QbservableServiceOptions options, Type[] knownTypes, params Notification<TSource>[] notifications)
      => new TcpTestService<TSource>(options, knownTypes, notifications);

    public static TcpTestService<TSource> Create<TSource>(IObservable<TSource> source)
      => new TcpTestService<TSource>(DefaultOptions, (Type[])null, source);

    public static TcpTestService<TSource> Create<TSource>(Type[] knownTypes, IObservable<TSource> source)
      => new TcpTestService<TSource>(DefaultOptions, knownTypes, source);

    public static TcpTestService<TSource> Create<TSource>(QbservableServiceOptions options, IObservable<TSource> source)
      => new TcpTestService<TSource>(options, (Type[])null, source);

    public static TcpTestService<TSource> Create<TSource>(QbservableServiceOptions options, Type[] knownTypes, IObservable<TSource> source)
      => new TcpTestService<TSource>(options, knownTypes, source);
  }
}

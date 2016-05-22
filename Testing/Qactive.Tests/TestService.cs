using System;
using System.Reactive;

namespace Qactive.Tests
{
  internal static class TestService
  {
    public static TestService<TSource> Create<TSource>(params Notification<TSource>[] notifications)
      => new TestService<TSource>(QbservableServiceOptions.Default, (Type[])null, notifications);

    public static TestService<TSource> Create<TSource>(Type[] knownTypes, params Notification<TSource>[] notifications)
      => new TestService<TSource>(QbservableServiceOptions.Default, knownTypes, notifications);

    public static TestService<TSource> Create<TSource>(TestQactiveProvider provider, params Notification<TSource>[] notifications)
      => new TestService<TSource>(QbservableServiceOptions.Default, provider, notifications);

    public static TestService<TSource> Create<TSource>(QbservableServiceOptions options, params Notification<TSource>[] notifications)
      => new TestService<TSource>(options, (Type[])null, notifications);

    public static TestService<TSource> Create<TSource>(QbservableServiceOptions options, Type[] knownTypes, params Notification<TSource>[] notifications)
      => new TestService<TSource>(options, knownTypes, notifications);

    public static TestService<TSource> Create<TSource>(QbservableServiceOptions options, TestQactiveProvider provider, params Notification<TSource>[] notifications)
      => new TestService<TSource>(options, provider, notifications);

    public static TestService<TSource> Create<TSource>(IObservable<TSource> source)
      => new TestService<TSource>(QbservableServiceOptions.Default, (Type[])null, source);

    public static TestService<TSource> Create<TSource>(Type[] knownTypes, IObservable<TSource> source)
      => new TestService<TSource>(QbservableServiceOptions.Default, knownTypes, source);

    public static TestService<TSource> Create<TSource>(TestQactiveProvider provider, IObservable<TSource> source)
      => new TestService<TSource>(QbservableServiceOptions.Default, provider, source);

    public static TestService<TSource> Create<TSource>(QbservableServiceOptions options, IObservable<TSource> source)
      => new TestService<TSource>(options, (Type[])null, source);

    public static TestService<TSource> Create<TSource>(QbservableServiceOptions options, Type[] knownTypes, IObservable<TSource> source)
      => new TestService<TSource>(options, knownTypes, source);

    public static TestService<TSource> Create<TSource>(QbservableServiceOptions options, TestQactiveProvider provider, IObservable<TSource> source)
      => new TestService<TSource>(options, provider, source);
  }
}

using System;
using System.Reactive;

namespace Qactive.Tests
{
  internal static class TestService
  {
    public static TestService<TSource> Create<TSource>(params Notification<TSource>[] notifications)
      => new TestService<TSource>((Type[])null, notifications);

    public static TestService<TSource> Create<TSource>(Type[] knownTypes, params Notification<TSource>[] notifications)
      => new TestService<TSource>(knownTypes, notifications);

    public static TestService<TSource> Create<TSource>(TestQactiveProvider provider, params Notification<TSource>[] notifications)
      => new TestService<TSource>(provider, notifications);
  }
}

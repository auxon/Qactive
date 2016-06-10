using System;
using System.Reactive;
using System.Reactive.Linq;

namespace Qactive.Tests
{
  internal sealed class TestService<TSource> : TestServiceBase<TSource>
  {
    private readonly QbservableServiceOptions options;
    private readonly TestQactiveProvider provider;

    public TestService(QbservableServiceOptions options, Type[] knownTypes, params Notification<TSource>[] notifications)
      : base(notifications)
    {
      this.options = options;
      provider = TestQactiveProvider.Create<TSource>(knownTypes);
    }

    public TestService(QbservableServiceOptions options, TestQactiveProvider provider, params Notification<TSource>[] notifications)
      : base(notifications)
    {
      this.options = options;
      provider = TestQactiveProvider.Create<TSource>();
    }

    public TestService(QbservableServiceOptions options, Type[] knownTypes, IObservable<TSource> source)
      : base(source)
    {
      this.options = options;
      provider = TestQactiveProvider.Create<TSource>(knownTypes);
    }

    public TestService(QbservableServiceOptions options, TestQactiveProvider provider, IObservable<TSource> source)
      : base(source)
    {
      this.options = options;
      provider = TestQactiveProvider.Create<TSource>();
    }

    protected override IObservable<ClientTermination> ServeQbservable(IObservable<TSource> source)
      => source.ServeQbservable(provider, options);

    protected override IQbservable<TSource> CreateQuery()
      => provider.CreateQuery<TSource>();
  }
}

using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Reactive.Testing;

namespace Qactive.Tests
{
  internal sealed class TestService
  {
    private readonly Type[] knownTypes;
    private readonly TestQactiveProvider provider;

    public TestScheduler Scheduler { get; }

    public TestService(params Type[] knownTypes)
      : this(new TestScheduler(), knownTypes)
    {
    }

    public TestService(TestQactiveProvider provider)
      : this(new TestScheduler(), provider)
    {
    }

    public TestService(TestScheduler scheduler, params Type[] knownTypes)
    {
      this.Scheduler = scheduler;
      this.knownTypes = knownTypes;
    }

    public TestService(TestScheduler scheduler, TestQactiveProvider provider)
    {
      this.Scheduler = scheduler;
      this.provider = provider;
    }

    public async Task<ITestableObserver<TResult>> StartAsync<TSource, TResult>(
      Func<IQbservable<TSource>, IQbservable<TResult>> query,
      params Recorded<Notification<TSource>>[] notifications)
    {
      var p = provider ?? TestQactiveProvider.Create<TSource>(knownTypes);
      var server = notifications.OrderBy(n => n.Time).Select(n => n.Value).ToObservable().Dematerialize().ServeQbservable(p);
      var client = query(p.CreateQuery<TSource>());
      var observer = Scheduler.CreateObserver<TResult>();

      var xs = server.IgnoreElements().Cast<TResult>().Merge(client.AsObservable()).PublishLast();

      xs.Subscribe(observer);
      xs.Connect();

      await xs;

      return observer;
    }
  }
}

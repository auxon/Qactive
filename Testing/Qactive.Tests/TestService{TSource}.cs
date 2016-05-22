using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace Qactive.Tests
{
  internal sealed class TestService<TSource>
  {
    private readonly Type[] knownTypes;
    private readonly TestQactiveProvider provider;
    private readonly IObservable<TSource> source;

    public IReadOnlyCollection<Notification<TSource>> Notifications { get; }

    public QbservableServiceOptions Options { get; }

    public TestService(QbservableServiceOptions options, Type[] knownTypes, params Notification<TSource>[] notifications)
      : this(options, knownTypes, notifications.ToObservable().Dematerialize())
    {
      Notifications = notifications;
    }

    public TestService(QbservableServiceOptions options, TestQactiveProvider provider, params Notification<TSource>[] notifications)
      : this(options, provider, notifications.ToObservable().Dematerialize())
    {
      Notifications = notifications;
    }

    public TestService(QbservableServiceOptions options, Type[] knownTypes, IObservable<TSource> source)
    {
      Options = options ?? TestService.DefaultOptions;
      this.knownTypes = knownTypes;
      this.source = source;
    }

    public TestService(QbservableServiceOptions options, TestQactiveProvider provider, IObservable<TSource> source)
    {
      Options = options ?? TestService.DefaultOptions;
      this.provider = provider;
      this.source = source;
    }

    public async Task<IReadOnlyCollection<Notification<TResult>>> QueryAsync<TResult>(
      Func<IQbservable<TSource>, IQbservable<TResult>> query)
    {
      var p = provider ?? TestQactiveProvider.Create<TSource>(knownTypes);
      var server = source.ServeQbservable(p, Options);
      var client = query(p.CreateQuery<TSource>());

      var both = server.Take(1).IgnoreElements().Cast<Notification<TResult>>().Merge(client.AsObservable().Materialize()).Publish();
      var results = new List<Notification<TResult>>();

      using (both.Subscribe(results.Add))
      {
        var completed = both.Dematerialize().IgnoreElements().OnErrorResumeNext(Observable.Return(default(TResult))).ToTask();

        using (both.Connect())
        {
          await completed.ConfigureAwait(false);
        }
      }

      return results.AsReadOnly();
    }
  }
}

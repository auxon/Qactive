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

    public Task<IReadOnlyCollection<Notification<TResult>>> QueryAsync<TResult>(
      Func<IQbservable<TSource>, IQbservable<TResult>> query)
    {
      var p = provider ?? TestQactiveProvider.Create<TSource>(knownTypes);

      return RunQueryAsync(Observable.Merge(
        source.ServeQbservable(p, Options).Take(1).IgnoreElements().Cast<TResult>(),
        query(p.CreateQuery<TSource>()).AsObservable()));
    }

    public Task<IReadOnlyCollection<Notification<TResult>>> InMemoryQueryAsync<TResult>(
      Func<IObservable<TSource>, IObservable<TResult>> query)
      => RunQueryAsync(query(source));

    private async Task<IReadOnlyCollection<Notification<TResult>>> RunQueryAsync<TResult>(
      IObservable<TResult> query)
    {
      var results = new List<Notification<TResult>>();
      var materialized = query.Materialize().Publish();

      using (materialized.Subscribe(results.Add))
      {
        var completed = materialized.Dematerialize().IgnoreElements().OnErrorResumeNext(Observable.Return(default(TResult))).ToTask();

        using (materialized.Connect())
        {
          await completed.ConfigureAwait(false);
        }
      }

      return results.AsReadOnly();
    }
  }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace Qactive.Tests
{
  internal abstract class TestServiceBase<TSource>
  {
    private readonly IObservable<TSource> source;

    public IReadOnlyCollection<Notification<TSource>> Notifications { get; }

    public TestServiceBase(params Notification<TSource>[] notifications)
      : this(notifications.ToObservable().Dematerialize())
    {
      Notifications = notifications;
    }

    public TestServiceBase(IObservable<TSource> source)
    {
      this.source = source;
    }

    protected abstract IObservable<ClientTermination> ServeQbservable(IObservable<TSource> source);

    protected abstract IQbservable<TSource> CreateQuery();

    public Task<IReadOnlyCollection<Notification<TResult>>> QueryAsync<TResult>(
      Func<IQbservable<TSource>, IQbservable<TResult>> query)
      => RunQueryAsync(Observable.Merge(
          ServeQbservable(source).Take(1).IgnoreElements().Cast<TResult>(),
          query(CreateQuery()).AsObservable()));

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

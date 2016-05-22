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

    public IReadOnlyCollection<Notification<TSource>> Notifications { get; private set; }

    public TestService(Type[] knownTypes, params Notification<TSource>[] notifications)
    {
      this.knownTypes = knownTypes;
      Notifications = notifications;
    }

    public TestService(TestQactiveProvider provider, params Notification<TSource>[] notifications)
    {
      this.provider = provider;
      Notifications = notifications;
    }

    public async Task<IReadOnlyCollection<Notification<TResult>>> QueryAsync<TResult>(
      Func<IQbservable<TSource>, IQbservable<TResult>> query)
    {
      var p = provider ?? TestQactiveProvider.Create<TSource>(knownTypes);
      var source = Notifications.ToObservable().Dematerialize();
      var server = source.ServeQbservable(p);
      var client = query(p.CreateQuery<TSource>());

      var both = server.Take(1).IgnoreElements().Cast<Notification<TResult>>().Merge(client.AsObservable().Materialize()).Publish();
      var results = new List<Notification<TResult>>();

      using (both.Subscribe(results.Add))
      {
        var completed = both.ToTask();

        using (both.Connect())
        {
          await completed.ConfigureAwait(false);
        }
      }

      return results.AsReadOnly();
    }
  }
}

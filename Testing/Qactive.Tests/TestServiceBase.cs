using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Qactive.Tests
{
  internal abstract class TestServiceBase<TSource>
  {
    private readonly IObservable<TSource> source;

    public IReadOnlyCollection<Notification<TSource>> Notifications { get; }

    public TestServiceBase(params Notification<TSource>[] notifications)
      : this(notifications.ToObservable().Dematerialize())
    {
#if READONLYCOLLECTIONS
      Notifications = notifications;
#else
      Notifications = notifications.AsReadOnly();
#endif
    }

    public TestServiceBase(IObservable<TSource> source)
    {
      this.source = source;
    }

    protected abstract IObservable<ClientTermination> ServeQbservable(IObservable<TSource> source);

    protected abstract IQbservable<TSource> CreateQuery();

    public Task<IReadOnlyCollection<Notification<TResult>>> QueryAsync<TResult>(
      Func<IQbservable<TSource>, IQbservable<TResult>> query,
      IEnumerable<Exception> expectedClientTerminationErrors = null,
      Exception expectedServerError = null,
      TimeSpan? timeout = null)
      => RunQueryAsync(Observable.Merge(
          ServeQbservable(source).Take(1)
                                 .Do(termination => QactiveAssert.AreEqual(termination.Exceptions.Select(ex => ex.SourceException), expectedClientTerminationErrors),
                                     ex => QactiveAssert.AreEqual(ex, expectedServerError, "Unexpected server error: " + ex.ToString()),
                                     () => Assert.IsNull(expectedServerError, "A server error was expected, but the sequence completed instead."))
                                 .IgnoreElements()
                                 .Cast<TResult>(),
          // Defer is required to ensure that the server is subscribed to first so that in case the port is 0, the TCP listener is started before the client query is 
          // created, thus the port selected by TCP will be known at this point in time and can be passed to the TcpQbservableClient. See TcpTestService<TSource>.
          Observable.Defer(() => query(CreateQuery()).AsObservable()))
          .Timeout(Debugger.IsAttached ? TimeSpan.FromDays(5) : timeout ?? TimeSpan.FromSeconds(5)));

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

#if READONLYCOLLECTIONS
      return results.AsReadOnly();
#else
      return results.AsReadOnly().AsInterface();
#endif
    }
  }
}

using System;
using System.Diagnostics.Contracts;
using System.Reactive.Linq;

namespace Qactive
{
  public static class Qbservable2
  {
    public static IObservable<ClientTermination> ServeQbservable<TSource>(this IObservable<TSource> source, IQactiveProvider provider)
    {
      Contract.Requires(source != null);
      Contract.Requires(provider != null);
      Contract.Ensures(Contract.Result<IObservable<ClientTermination>>() != null);

      return QbservableServer.CreateService<object, TSource>(provider, _ => source);
    }

    public static IObservable<ClientTermination> ServeQbservable<TSource>(this IObservable<TSource> source, IQactiveProvider provider, QbservableServiceOptions options)
    {
      Contract.Requires(source != null);
      Contract.Requires(provider != null);
      Contract.Requires(options != null);
      Contract.Ensures(Contract.Result<IObservable<ClientTermination>>() != null);

      return QbservableServer.CreateService<object, TSource>(provider, options, _ => source);
    }

    public static IObservable<ClientTermination> Serve<TSource>(this IQbservable<TSource> source, IQactiveProvider provider)
    {
      Contract.Requires(source != null);
      Contract.Requires(provider != null);
      Contract.Ensures(Contract.Result<IObservable<ClientTermination>>() != null);

      return QbservableServer.CreateService<object, TSource>(provider, _ => source);
    }

    public static IObservable<ClientTermination> Serve<TSource>(this IQbservable<TSource> source, IQactiveProvider provider, QbservableServiceOptions options)
    {
      Contract.Requires(source != null);
      Contract.Requires(provider != null);
      Contract.Requires(options != null);
      Contract.Ensures(Contract.Result<IObservable<ClientTermination>>() != null);

      return QbservableServer.CreateService<object, TSource>(provider, options, _ => source);
    }
  }
}

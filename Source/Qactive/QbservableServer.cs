using System;
using System.Diagnostics.Contracts;
using System.Reactive.Linq;

namespace Qactive
{
  public static partial class QbservableServer
  {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<ClientTermination> CreateService<TSource, TResult>(
      IQactiveProvider provider,
      Func<IObservable<TSource>, IObservable<TResult>> service)
    {
      Contract.Requires(provider != null);
      Contract.Requires(service != null);
      Contract.Ensures(Contract.Result<IObservable<ClientTermination>>() != null);

      return CreateService<TSource, TResult>(provider, request => service(request).AsQbservable());
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<ClientTermination> CreateService<TSource, TResult>(
      IQactiveProvider provider,
      QbservableServiceOptions options,
      Func<IObservable<TSource>, IObservable<TResult>> service)
    {
      Contract.Requires(provider != null);
      Contract.Requires(options != null);
      Contract.Requires(service != null);
      Contract.Ensures(Contract.Result<IObservable<ClientTermination>>() != null);

      return CreateService<TSource, TResult>(provider, options, request => service(request).AsQbservable());
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<ClientTermination> CreateService<TSource, TResult>(
      IQactiveProvider provider,
      Func<IObservable<TSource>, IQbservable<TResult>> service)
    {
      Contract.Requires(provider != null);
      Contract.Requires(service != null);
      Contract.Ensures(Contract.Result<IObservable<ClientTermination>>() != null);

      return CreateService(provider, QbservableServiceOptions.Default, service);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<ClientTermination> CreateService<TSource, TResult>(
      IQactiveProvider provider,
      QbservableServiceOptions options,
      Func<IObservable<TSource>, IQbservable<TResult>> service)
    {
      Contract.Requires(provider != null);
      Contract.Requires(options != null);
      Contract.Requires(service != null);
      Contract.Ensures(Contract.Result<IObservable<ClientTermination>>() != null);

      return provider.Listen(
        options,
        protocol => new ServerQbservableProvider<TResult>(
                      protocol,
                      options,
                      argument => argument == null && typeof(TSource).IsValueType
                                ? service(Observable.Return(default(TSource)))
                                : service(Observable.Return((TSource)argument))));
    }
  }
}
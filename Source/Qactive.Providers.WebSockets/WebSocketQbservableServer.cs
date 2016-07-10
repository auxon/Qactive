using System;
using System.Diagnostics.Contracts;
using System.Reactive.Linq;
using System.Runtime.Remoting.Messaging;

namespace Qactive
{
  public static partial class WebSocketQbservableServer
  {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<ClientTermination> CreateService<TSource, TResult>(
      Uri uri,
      Func<IObservable<TSource>, IObservable<TResult>> service)
    {
      Contract.Requires(uri != null);
      Contract.Requires(service != null);
      Contract.Ensures(Contract.Result<IObservable<ClientTermination>>() != null);

      return QbservableServer.CreateService(WebSocketQactiveProvider.Server(uri), service);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<ClientTermination> CreateService<TSource, TResult>(
      Uri uri,
      QbservableServiceOptions options,
      Func<IObservable<TSource>, IObservable<TResult>> service)
    {
      Contract.Requires(uri != null);
      Contract.Requires(options != null);
      Contract.Requires(service != null);
      Contract.Ensures(Contract.Result<IObservable<ClientTermination>>() != null);

      return QbservableServer.CreateService(WebSocketQactiveProvider.Server(uri), options, service);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<ClientTermination> CreateService<TSource, TResult>(
      Uri uri,
      IWebSocketQactiveProviderTransportInitializer transportInitializer,
      Func<IObservable<TSource>, IObservable<TResult>> service)
    {
      Contract.Requires(uri != null);
      Contract.Requires(transportInitializer != null);
      Contract.Requires(service != null);
      Contract.Ensures(Contract.Result<IObservable<ClientTermination>>() != null);

      return QbservableServer.CreateService(WebSocketQactiveProvider.Server(uri, transportInitializer), service);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<ClientTermination> CreateService<TSource, TResult>(
      Uri uri,
      IWebSocketQactiveProviderTransportInitializer transportInitializer,
      QbservableServiceOptions options,
      Func<IObservable<TSource>, IObservable<TResult>> service)
    {
      Contract.Requires(uri != null);
      Contract.Requires(transportInitializer != null);
      Contract.Requires(options != null);
      Contract.Requires(service != null);
      Contract.Ensures(Contract.Result<IObservable<ClientTermination>>() != null);

      return QbservableServer.CreateService(WebSocketQactiveProvider.Server(uri, transportInitializer), options, service);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<ClientTermination> CreateService<TSource, TResult>(
      Uri uri,
      Func<IObservable<TSource>, IQbservable<TResult>> service)
    {
      Contract.Requires(uri != null);
      Contract.Requires(service != null);
      Contract.Ensures(Contract.Result<IObservable<ClientTermination>>() != null);

      return QbservableServer.CreateService(WebSocketQactiveProvider.Server(uri), service);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<ClientTermination> CreateService<TSource, TResult>(
      Uri uri,
      QbservableServiceOptions options,
      Func<IObservable<TSource>, IQbservable<TResult>> service)
    {
      Contract.Requires(uri != null);
      Contract.Requires(options != null);
      Contract.Requires(service != null);
      Contract.Ensures(Contract.Result<IObservable<ClientTermination>>() != null);

      return QbservableServer.CreateService(WebSocketQactiveProvider.Server(uri), options, service);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<ClientTermination> CreateService<TSource, TResult>(
      Uri uri,
      IRemotingFormatter formatter,
      Func<IObservable<TSource>, IQbservable<TResult>> service)
    {
      Contract.Requires(uri != null);
      Contract.Requires(formatter != null);
      Contract.Requires(service != null);
      Contract.Ensures(Contract.Result<IObservable<ClientTermination>>() != null);

      return QbservableServer.CreateService(WebSocketQactiveProvider.Server(uri), service);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<ClientTermination> CreateService<TSource, TResult>(
      Uri uri,
      IWebSocketQactiveProviderTransportInitializer transportInitializer,
      QbservableServiceOptions options,
      Func<IObservable<TSource>, IQbservable<TResult>> service)
    {
      Contract.Requires(uri != null);
      Contract.Requires(transportInitializer != null);
      Contract.Requires(options != null);
      Contract.Requires(service != null);
      Contract.Ensures(Contract.Result<IObservable<ClientTermination>>() != null);

      return QbservableServer.CreateService(WebSocketQactiveProvider.Server(uri, transportInitializer), options, service);
    }
  }
}
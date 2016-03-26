using System;
using System.Net;
using System.Reactive.Linq;
using System.Runtime.Remoting.Messaging;

namespace Qactive
{
  public static partial class TcpQbservableServer
  {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<ClientTermination> CreateService<TSource, TResult>(
      IPEndPoint endPoint,
      Func<IObservable<TSource>, IObservable<TResult>> service)
      => QbservableServer.CreateService(TcpQactiveProvider.Server(endPoint), service);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<ClientTermination> CreateService<TSource, TResult>(
      IPEndPoint endPoint,
      QbservableServiceOptions options,
      Func<IObservable<TSource>, IObservable<TResult>> service)
      => QbservableServer.CreateService(TcpQactiveProvider.Server(endPoint), options, service);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<ClientTermination> CreateService<TSource, TResult>(
      IPEndPoint endPoint,
      ITcpQactiveProviderTransportInitializer transportInitializer,
      Func<IObservable<TSource>, IObservable<TResult>> service)
      => QbservableServer.CreateService(TcpQactiveProvider.Server(endPoint, transportInitializer), service);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<ClientTermination> CreateService<TSource, TResult>(
      IPEndPoint endPoint,
      ITcpQactiveProviderTransportInitializer transportInitializer,
      QbservableServiceOptions options,
      Func<IObservable<TSource>, IObservable<TResult>> service)
      => QbservableServer.CreateService(TcpQactiveProvider.Server(endPoint, transportInitializer), options, service);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<ClientTermination> CreateService<TSource, TResult>(
      IPEndPoint endPoint,
      Func<IObservable<TSource>, IQbservable<TResult>> service)
      => QbservableServer.CreateService(TcpQactiveProvider.Server(endPoint), service);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<ClientTermination> CreateService<TSource, TResult>(
      IPEndPoint endPoint,
      QbservableServiceOptions options,
      Func<IObservable<TSource>, IQbservable<TResult>> service)
      => QbservableServer.CreateService(TcpQactiveProvider.Server(endPoint), options, service);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<ClientTermination> CreateService<TSource, TResult>(
      IPEndPoint endPoint,
      IRemotingFormatter formatter,
      Func<IObservable<TSource>, IQbservable<TResult>> service)
      => QbservableServer.CreateService(TcpQactiveProvider.Server(endPoint), service);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<ClientTermination> CreateService<TSource, TResult>(
      IPEndPoint endPoint,
      ITcpQactiveProviderTransportInitializer transportInitializer,
      QbservableServiceOptions options,
      Func<IObservable<TSource>, IQbservable<TResult>> service)
      => QbservableServer.CreateService(TcpQactiveProvider.Server(endPoint, transportInitializer), options, service);
  }
}
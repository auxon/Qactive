using System;
using System.Net;
using System.Net.Sockets;
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
      => QbservableServer.CreateService(TcpQactiveProvider.Server(endPoint, Nop.Action), service);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<ClientTermination> CreateService<TSource, TResult>(
      IPEndPoint endPoint,
      QbservableServiceOptions options,
      Func<IObservable<TSource>, IObservable<TResult>> service)
      => QbservableServer.CreateService(TcpQactiveProvider.Server(endPoint, Nop.Action), options, service);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<ClientTermination> CreateService<TSource, TResult>(
      IPEndPoint endPoint,
      IRemotingFormatter formatter,
      Func<IObservable<TSource>, IObservable<TResult>> service)
      => QbservableServer.CreateService(TcpQactiveProvider.Server(endPoint, Nop.Action, () => formatter), service);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<ClientTermination> CreateService<TSource, TResult>(
      IPEndPoint endPoint,
      IRemotingFormatter formatter,
      QbservableServiceOptions options,
      Func<IObservable<TSource>, IObservable<TResult>> service)
      => QbservableServer.CreateService(TcpQactiveProvider.Server(endPoint, Nop.Action, () => formatter), options, service);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<ClientTermination> CreateService<TSource, TResult>(
      IPEndPoint endPoint,
      Action<Socket> prepareSocket,
      IRemotingFormatter formatter,
      QbservableServiceOptions options,
      Func<IObservable<TSource>, IObservable<TResult>> service)
      => QbservableServer.CreateService(TcpQactiveProvider.Server(endPoint, prepareSocket, () => formatter), options, service);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<ClientTermination> CreateService<TSource, TResult>(
      IPEndPoint endPoint,
      Func<IObservable<TSource>, IQbservable<TResult>> service)
      => QbservableServer.CreateService(TcpQactiveProvider.Server(endPoint, Nop.Action), service);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<ClientTermination> CreateService<TSource, TResult>(
      IPEndPoint endPoint,
      QbservableServiceOptions options,
      Func<IObservable<TSource>, IQbservable<TResult>> service)
      => QbservableServer.CreateService(TcpQactiveProvider.Server(endPoint, Nop.Action), options, service);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<ClientTermination> CreateService<TSource, TResult>(
      IPEndPoint endPoint,
      IRemotingFormatter formatter,
      Func<IObservable<TSource>, IQbservable<TResult>> service)
      => QbservableServer.CreateService(TcpQactiveProvider.Server(endPoint, Nop.Action, () => formatter), service);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<ClientTermination> CreateService<TSource, TResult>(
      IPEndPoint endPoint,
      IRemotingFormatter formatter,
      QbservableServiceOptions options,
      Func<IObservable<TSource>, IQbservable<TResult>> service)
      => QbservableServer.CreateService(TcpQactiveProvider.Server(endPoint, Nop.Action, () => formatter), options, service);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<ClientTermination> CreateService<TSource, TResult>(
      IPEndPoint endPoint,
      Action<Socket> prepareSocket,
      IRemotingFormatter formatter,
      QbservableServiceOptions options,
      Func<IObservable<TSource>, IQbservable<TResult>> service)
      => QbservableServer.CreateService(TcpQactiveProvider.Server(endPoint, prepareSocket, () => formatter), options, service);
  }
}
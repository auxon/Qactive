using System;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Security;

namespace Qactive
{
  public static partial class TcpQbservableServer
  {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Too many overloads.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<ClientTermination> CreateService<TSource, TResult>(
      AppDomainSetup appDomainSetup,
      IPEndPoint endPoint,
      Func<IObservable<TSource>, IObservable<TResult>> service,
      [CallerMemberName] string appDomainBaseName = null,
      params Assembly[] fullTrustAssemblies)
      => QbservableServer.CreateService<TSource, TResult>(appDomainSetup, new TcpQactiveProviderFactory(endPoint, Nop.Action), new QbservableServiceConverter<TSource, TResult>(service).Convert, appDomainBaseName, fullTrustAssemblies);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Too many overloads.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<ClientTermination> CreateService<TSource, TResult>(
      AppDomainSetup appDomainSetup,
      IPEndPoint endPoint,
      QbservableServiceOptions options,
      Func<IObservable<TSource>, IObservable<TResult>> service,
      [CallerMemberName] string appDomainBaseName = null,
      params Assembly[] fullTrustAssemblies)
      => QbservableServer.CreateService<TSource, TResult>(appDomainSetup, new TcpQactiveProviderFactory(endPoint, Nop.Action), options, new QbservableServiceConverter<TSource, TResult>(service).Convert, appDomainBaseName, fullTrustAssemblies);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Too many overloads.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<ClientTermination> CreateService<TSource, TResult>(
      AppDomainSetup appDomainSetup,
      IPEndPoint endPoint,
      Func<IRemotingFormatter> formatterFactory,
      Func<IObservable<TSource>, IObservable<TResult>> service,
      [CallerMemberName] string appDomainBaseName = null,
      params Assembly[] fullTrustAssemblies)
      => QbservableServer.CreateService<TSource, TResult>(appDomainSetup, new TcpQactiveProviderFactory(endPoint, Nop.Action, formatterFactory), new QbservableServiceConverter<TSource, TResult>(service).Convert, appDomainBaseName, fullTrustAssemblies);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Too many overloads.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<ClientTermination> CreateService<TSource, TResult>(
      AppDomainSetup appDomainSetup,
      IPEndPoint endPoint,
      Func<IRemotingFormatter> formatterFactory,
      QbservableServiceOptions options,
      Func<IObservable<TSource>, IObservable<TResult>> service,
      [CallerMemberName] string appDomainBaseName = null,
      params Assembly[] fullTrustAssemblies)
      => QbservableServer.CreateService<TSource, TResult>(appDomainSetup, new TcpQactiveProviderFactory(endPoint, Nop.Action, formatterFactory), options, new QbservableServiceConverter<TSource, TResult>(service).Convert, appDomainBaseName, fullTrustAssemblies);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Too many overloads.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<ClientTermination> CreateService<TSource, TResult>(
      AppDomainSetup appDomainSetup,
      IPEndPoint endPoint,
      Func<IObservable<TSource>, IQbservable<TResult>> service,
      [CallerMemberName] string appDomainBaseName = null,
      params Assembly[] fullTrustAssemblies)
      => QbservableServer.CreateService(appDomainSetup, new TcpQactiveProviderFactory(endPoint, Nop.Action), service, appDomainBaseName, fullTrustAssemblies);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Too many overloads.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<ClientTermination> CreateService<TSource, TResult>(
      AppDomainSetup appDomainSetup,
      IPEndPoint endPoint,
      QbservableServiceOptions options,
      Func<IObservable<TSource>, IQbservable<TResult>> service,
      [CallerMemberName] string appDomainBaseName = null,
      params Assembly[] fullTrustAssemblies)
      => QbservableServer.CreateService(appDomainSetup, new TcpQactiveProviderFactory(endPoint, Nop.Action), options, service, appDomainBaseName, fullTrustAssemblies);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Too many overloads.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<ClientTermination> CreateService<TSource, TResult>(
      AppDomainSetup appDomainSetup,
      IPEndPoint endPoint,
      Func<IRemotingFormatter> formatterFactory,
      Func<IObservable<TSource>, IQbservable<TResult>> service,
      [CallerMemberName] string appDomainBaseName = null,
      params Assembly[] fullTrustAssemblies)
      => QbservableServer.CreateService(appDomainSetup, new TcpQactiveProviderFactory(endPoint, Nop.Action, formatterFactory), service, appDomainBaseName, fullTrustAssemblies);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Too many overloads.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<ClientTermination> CreateService<TSource, TResult>(
      AppDomainSetup appDomainSetup,
      IPEndPoint endPoint,
      Func<IRemotingFormatter> formatterFactory,
      QbservableServiceOptions options,
      Func<IObservable<TSource>, IQbservable<TResult>> service,
      [CallerMemberName] string appDomainBaseName = null,
      params Assembly[] fullTrustAssemblies)
      => QbservableServer.CreateService(appDomainSetup, new TcpQactiveProviderFactory(endPoint, Nop.Action, formatterFactory), options, service, appDomainBaseName, fullTrustAssemblies);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2103:ReviewImperativeSecurity", Justification = "False positive; The Address is converted to a string before being passed to the permission, so it cannot be mutated later.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "AppDomain setup is very finicky and requires lots of concrete type references.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Too many overloads.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<ClientTermination> CreateService<TSource, TResult>(
      AppDomainSetup appDomainSetup,
      PermissionSet permissions,
      IPEndPoint endPoint,
      Func<IRemotingFormatter> formatterFactory,
      QbservableServiceOptions options,
      Func<IObservable<TSource>, IQbservable<TResult>> service,
      [CallerMemberName] string appDomainBaseName = null,
      params Assembly[] fullTrustAssemblies)
      => QbservableServer.CreateService(appDomainSetup, permissions, new TcpQactiveProviderFactory(endPoint, Nop.Action, formatterFactory), options, service, appDomainBaseName, fullTrustAssemblies);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Too many overloads.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<ClientTermination> CreateService<TSource, TResult>(
      AppDomainSetup appDomainSetup,
      IPEndPoint endPoint,
      Action<Socket> prepareSocket,
      Func<IRemotingFormatter> formatterFactory,
      QbservableServiceOptions options,
      Func<IObservable<TSource>, IQbservable<TResult>> service,
      [CallerMemberName] string appDomainBaseName = null,
      params Assembly[] fullTrustAssemblies)
      => QbservableServer.CreateService(appDomainSetup, new TcpQactiveProviderFactory(endPoint, prepareSocket, formatterFactory), options, service, appDomainBaseName, fullTrustAssemblies);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2103:ReviewImperativeSecurity", Justification = "False positive; The Address is converted to a string before being passed to the permission, so it cannot be mutated later.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "AppDomain setup is very finicky and requires lots of concrete type references.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Too many overloads.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<ClientTermination> CreateService<TSource, TResult>(
      AppDomainSetup appDomainSetup,
      PermissionSet permissions,
      IPEndPoint endPoint,
      Action<Socket> prepareSocket,
      Func<IRemotingFormatter> formatterFactory,
      QbservableServiceOptions options,
      Func<IObservable<TSource>, IQbservable<TResult>> service,
      [CallerMemberName] string appDomainBaseName = null,
      params Assembly[] fullTrustAssemblies)
      => QbservableServer.CreateService(appDomainSetup, permissions, new TcpQactiveProviderFactory(endPoint, prepareSocket, formatterFactory), options, service, appDomainBaseName, fullTrustAssemblies);
  }
}
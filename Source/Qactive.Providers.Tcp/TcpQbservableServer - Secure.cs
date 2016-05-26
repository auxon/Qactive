using System;
using System.Diagnostics.Contracts;
using System.Net;
using System.Reactive.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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
    {
      Contract.Requires(appDomainSetup != null);
      Contract.Requires(endPoint != null);
      Contract.Requires(service != null);
      Contract.Requires(fullTrustAssemblies != null);
      Contract.Ensures(Contract.Result<IObservable<ClientTermination>>() != null);

#if CAS_REF
      return QbservableServer.CreateService<TSource, TResult>(
#else
      return QbservableServerSecure.CreateService<TSource, TResult>(
#endif
        appDomainSetup,
        new TcpQactiveProviderFactory(endPoint),
        new QbservableServiceConverter<TSource, TResult>(service).Convert,
        appDomainBaseName,
        fullTrustAssemblies);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Too many overloads.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<ClientTermination> CreateService<TSource, TResult>(
      AppDomainSetup appDomainSetup,
      IPEndPoint endPoint,
      QbservableServiceOptions options,
      Func<IObservable<TSource>, IObservable<TResult>> service,
      [CallerMemberName] string appDomainBaseName = null,
      params Assembly[] fullTrustAssemblies)
    {
      Contract.Requires(appDomainSetup != null);
      Contract.Requires(endPoint != null);
      Contract.Requires(options != null);
      Contract.Requires(service != null);
      Contract.Requires(fullTrustAssemblies != null);
      Contract.Ensures(Contract.Result<IObservable<ClientTermination>>() != null);

#if CAS_REF
      return QbservableServer.CreateService<TSource, TResult>(
#else
      return QbservableServerSecure.CreateService<TSource, TResult>(
#endif
        appDomainSetup,
        new TcpQactiveProviderFactory(endPoint),
        options,
        new QbservableServiceConverter<TSource, TResult>(service).Convert,
        appDomainBaseName,
        fullTrustAssemblies);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Type is constrained by new() and instances are created as needed.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Too many overloads.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<ClientTermination> CreateService<TSource, TResult, TTransportInitializer>(
      AppDomainSetup appDomainSetup,
      IPEndPoint endPoint,
      Func<IObservable<TSource>, IObservable<TResult>> service,
      [CallerMemberName] string appDomainBaseName = null,
      params Assembly[] fullTrustAssemblies)
      where TTransportInitializer : ITcpQactiveProviderTransportInitializer, new()
    {
      Contract.Requires(appDomainSetup != null);
      Contract.Requires(endPoint != null);
      Contract.Requires(service != null);
      Contract.Requires(fullTrustAssemblies != null);
      Contract.Ensures(Contract.Result<IObservable<ClientTermination>>() != null);

#if CAS_REF
      return QbservableServer.CreateService<TSource, TResult>(
#else
      return QbservableServerSecure.CreateService<TSource, TResult>(
#endif
        appDomainSetup,
        new TcpQactiveProviderFactory<TTransportInitializer>(endPoint),
        new QbservableServiceConverter<TSource, TResult>(service).Convert,
        appDomainBaseName,
        fullTrustAssemblies);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Type is constrained by new() and instances are created as needed.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Too many overloads.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<ClientTermination> CreateService<TSource, TResult, TTransportInitializer>(
      AppDomainSetup appDomainSetup,
      IPEndPoint endPoint,
      QbservableServiceOptions options,
      Func<IObservable<TSource>, IObservable<TResult>> service,
      [CallerMemberName] string appDomainBaseName = null,
      params Assembly[] fullTrustAssemblies)
      where TTransportInitializer : ITcpQactiveProviderTransportInitializer, new()
    {
      Contract.Requires(appDomainSetup != null);
      Contract.Requires(endPoint != null);
      Contract.Requires(options != null);
      Contract.Requires(service != null);
      Contract.Requires(fullTrustAssemblies != null);
      Contract.Ensures(Contract.Result<IObservable<ClientTermination>>() != null);

#if CAS_REF
      return QbservableServer.CreateService<TSource, TResult>(
#else
      return QbservableServerSecure.CreateService<TSource, TResult>(
#endif
        appDomainSetup,
        new TcpQactiveProviderFactory<TTransportInitializer>(endPoint),
        options,
        new QbservableServiceConverter<TSource, TResult>(service).Convert,
        appDomainBaseName,
        fullTrustAssemblies);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Too many overloads.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<ClientTermination> CreateService<TSource, TResult>(
      AppDomainSetup appDomainSetup,
      IPEndPoint endPoint,
      Func<IObservable<TSource>, IQbservable<TResult>> service,
      [CallerMemberName] string appDomainBaseName = null,
      params Assembly[] fullTrustAssemblies)
    {
      Contract.Requires(appDomainSetup != null);
      Contract.Requires(endPoint != null);
      Contract.Requires(service != null);
      Contract.Requires(fullTrustAssemblies != null);
      Contract.Ensures(Contract.Result<IObservable<ClientTermination>>() != null);

#if CAS_REF
      return QbservableServer.CreateService(
#else
      return QbservableServerSecure.CreateService(
#endif
        appDomainSetup,
        new TcpQactiveProviderFactory(endPoint),
        service,
        appDomainBaseName,
        fullTrustAssemblies);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Too many overloads.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<ClientTermination> CreateService<TSource, TResult>(
      AppDomainSetup appDomainSetup,
      IPEndPoint endPoint,
      QbservableServiceOptions options,
      Func<IObservable<TSource>, IQbservable<TResult>> service,
      [CallerMemberName] string appDomainBaseName = null,
      params Assembly[] fullTrustAssemblies)
    {
      Contract.Requires(appDomainSetup != null);
      Contract.Requires(endPoint != null);
      Contract.Requires(options != null);
      Contract.Requires(service != null);
      Contract.Requires(fullTrustAssemblies != null);
      Contract.Ensures(Contract.Result<IObservable<ClientTermination>>() != null);

#if CAS_REF
      return QbservableServer.CreateService(
#else
      return QbservableServerSecure.CreateService(
#endif
        appDomainSetup,
        new TcpQactiveProviderFactory(endPoint),
        options,
        service,
        appDomainBaseName,
        fullTrustAssemblies);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Type is constrained by new() and instances are created as needed.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Too many overloads.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<ClientTermination> CreateService<TSource, TResult, TTransportInitializer>(
      AppDomainSetup appDomainSetup,
      IPEndPoint endPoint,
      Func<IObservable<TSource>, IQbservable<TResult>> service,
      [CallerMemberName] string appDomainBaseName = null,
      params Assembly[] fullTrustAssemblies)
      where TTransportInitializer : ITcpQactiveProviderTransportInitializer, new()
    {
      Contract.Requires(appDomainSetup != null);
      Contract.Requires(endPoint != null);
      Contract.Requires(service != null);
      Contract.Requires(fullTrustAssemblies != null);
      Contract.Ensures(Contract.Result<IObservable<ClientTermination>>() != null);

#if CAS_REF
      return QbservableServer.CreateService(
#else
      return QbservableServerSecure.CreateService(
#endif
        appDomainSetup,
        new TcpQactiveProviderFactory<TTransportInitializer>(endPoint),
        service,
        appDomainBaseName,
        fullTrustAssemblies);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Type is constrained by new() and instances are created as needed.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Too many overloads.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<ClientTermination> CreateService<TSource, TResult, TTransportInitializer>(
      AppDomainSetup appDomainSetup,
      IPEndPoint endPoint,
      QbservableServiceOptions options,
      Func<IObservable<TSource>, IQbservable<TResult>> service,
      [CallerMemberName] string appDomainBaseName = null,
      params Assembly[] fullTrustAssemblies)
      where TTransportInitializer : ITcpQactiveProviderTransportInitializer, new()
    {
      Contract.Requires(appDomainSetup != null);
      Contract.Requires(endPoint != null);
      Contract.Requires(options != null);
      Contract.Requires(service != null);
      Contract.Requires(fullTrustAssemblies != null);
      Contract.Ensures(Contract.Result<IObservable<ClientTermination>>() != null);

#if CAS_REF
      return QbservableServer.CreateService(
#else
      return QbservableServerSecure.CreateService(
#endif
        appDomainSetup,
        new TcpQactiveProviderFactory<TTransportInitializer>(endPoint),
        options,
        service,
        appDomainBaseName,
        fullTrustAssemblies);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Type is constrained by new() and instances are created as needed.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2103:ReviewImperativeSecurity", Justification = "False positive; The Address is converted to a string before being passed to the permission, so it cannot be mutated later.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "AppDomain setup is very finicky and requires lots of concrete type references.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Too many overloads.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<ClientTermination> CreateService<TSource, TResult, TTransportInitializer>(
      AppDomainSetup appDomainSetup,
      PermissionSet permissions,
      IPEndPoint endPoint,
      QbservableServiceOptions options,
      Func<IObservable<TSource>, IQbservable<TResult>> service,
      [CallerMemberName] string appDomainBaseName = null,
      params Assembly[] fullTrustAssemblies)
      where TTransportInitializer : ITcpQactiveProviderTransportInitializer, new()
    {
      Contract.Requires(appDomainSetup != null);
      Contract.Requires(permissions != null);
      Contract.Requires(endPoint != null);
      Contract.Requires(options != null);
      Contract.Requires(service != null);
      Contract.Requires(fullTrustAssemblies != null);
      Contract.Ensures(Contract.Result<IObservable<ClientTermination>>() != null);

#if CAS_REF
      return QbservableServer.CreateService(
#else
      return QbservableServerSecure.CreateService(
#endif
        appDomainSetup,
        permissions,
        new TcpQactiveProviderFactory<TTransportInitializer>(endPoint),
        options,
        service,
        appDomainBaseName,
        fullTrustAssemblies);
    }
  }
}
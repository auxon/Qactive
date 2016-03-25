using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using System.Threading;
using Qactive.Properties;

namespace Qactive
{
  public static partial class QbservableServer
  {
    private static int appDomainNumber;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Too many overloads.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<ClientTermination> CreateService<TSource, TResult>(
      AppDomainSetup appDomainSetup,
      QactiveProviderFactory providerFactory,
      Func<IObservable<TSource>, IObservable<TResult>> service,
      [CallerMemberName] string appDomainBaseName = null,
      params Assembly[] fullTrustAssemblies)
    {
      return CreateService<TSource, TResult>(appDomainSetup, providerFactory, new QbservableServiceConverter<TSource, TResult>(service).Convert, appDomainBaseName, fullTrustAssemblies);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Too many overloads.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<ClientTermination> CreateService<TSource, TResult>(
      AppDomainSetup appDomainSetup,
      QactiveProviderFactory providerFactory,
      QbservableServiceOptions options,
      Func<IObservable<TSource>, IObservable<TResult>> service,
      [CallerMemberName] string appDomainBaseName = null,
      params Assembly[] fullTrustAssemblies)
    {
      return CreateService<TSource, TResult>(appDomainSetup, providerFactory, options, new QbservableServiceConverter<TSource, TResult>(service).Convert, appDomainBaseName, fullTrustAssemblies);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Too many overloads.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<ClientTermination> CreateService<TSource, TResult>(
      AppDomainSetup appDomainSetup,
      QactiveProviderFactory providerFactory,
      Func<IObservable<TSource>, IQbservable<TResult>> service,
      [CallerMemberName] string appDomainBaseName = null,
      params Assembly[] fullTrustAssemblies)
    {
      return CreateService(appDomainSetup, providerFactory, QbservableServiceOptions.Default, service, appDomainBaseName, fullTrustAssemblies);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Too many overloads.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<ClientTermination> CreateService<TSource, TResult>(
      AppDomainSetup appDomainSetup,
      QactiveProviderFactory providerFactory,
      QbservableServiceOptions options,
      Func<IObservable<TSource>, IQbservable<TResult>> service,
      [CallerMemberName] string appDomainBaseName = null,
      params Assembly[] fullTrustAssemblies)
    {
      var permissions = new PermissionSet(PermissionState.None);

      return CreateService(appDomainSetup, permissions, providerFactory, options, service, appDomainBaseName, fullTrustAssemblies);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2103:ReviewImperativeSecurity", Justification = "False positive; The Address is converted to a string before being passed to the permission, so it cannot be mutated later.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "AppDomain setup is very finicky and requires lots of concrete type references.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Too many overloads.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<ClientTermination> CreateService<TSource, TResult>(
      AppDomainSetup appDomainSetup,
      PermissionSet permissions,
      QactiveProviderFactory providerFactory,
      QbservableServiceOptions options,
      Func<IObservable<TSource>, IQbservable<TResult>> service,
      [CallerMemberName] string appDomainBaseName = null,
      params Assembly[] fullTrustAssemblies)
    {
      var minimumPermissions = new PermissionSet(PermissionState.None);

      minimumPermissions.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));

      foreach (var permission in providerFactory.MinimumServerPermissions)
      {
        minimumPermissions.AddPermission(permission);
      }

      var entryAssembly = Assembly.GetEntryAssembly();

      var domain = AppDomain.CreateDomain(
        Interlocked.Increment(ref appDomainNumber) + ':' + appDomainBaseName,
        null,
        appDomainSetup,
        minimumPermissions.Union(permissions),
        new[]
        {
          typeof(QbservableServer).Assembly.Evidence.GetHostEvidence<StrongName>(),
          typeof(System.Reactive.Linq.Observable).Assembly.Evidence.GetHostEvidence<StrongName>(),
          typeof(System.Reactive.Linq.Qbservable).Assembly.Evidence.GetHostEvidence<StrongName>(),
          typeof(System.Reactive.Notification).Assembly.Evidence.GetHostEvidence<StrongName>(),
          typeof(System.Reactive.IEventPattern<,>).Assembly.Evidence.GetHostEvidence<StrongName>(),
          typeof(System.Reactive.Concurrency.TaskPoolScheduler).Assembly.Evidence.GetHostEvidence<StrongName>()
        }
        .Concat(entryAssembly == null ? new StrongName[0] : new[] { entryAssembly.Evidence.GetHostEvidence<StrongName>() })
        .Concat(providerFactory.FullTrustAssemblies)
        .Concat(fullTrustAssemblies.Select(assembly => assembly.Evidence.GetHostEvidence<StrongName>()))
        .Distinct()
        .ToArray());

      try
      {
        var handle = Activator.CreateInstanceFrom(
          domain,
          typeof(CreateServiceProxy<TSource, TResult>).Assembly.ManifestModule.FullyQualifiedName,
          typeof(CreateServiceProxy<TSource, TResult>).FullName);

        var proxy = (CreateServiceProxy<TSource, TResult>)handle.Unwrap();

        return proxy.CreateService(providerFactory, options, new CreateServiceProxyDelegates<TSource, TResult>(service))
          .Finally(() => AppDomain.Unload(domain));
      }
      catch
      {
        AppDomain.Unload(domain);
        throw;
      }
    }

    private sealed class CreateServiceProxyDelegates<TSource, TResult> : MarshalByRefObject
    {
      public Func<IObservable<TSource>, IQbservable<TResult>> Service { get; }

      public CreateServiceProxyDelegates(Func<IObservable<TSource>, IQbservable<TResult>> service)
      {
        Service = service;
      }

      public override object InitializeLifetimeService()
      {
        return null;
      }
    }

    private sealed class CreateServiceProxy<TSource, TResult> : MarshalByRefObject
    {
      public IObservable<ClientTermination> CreateService(
        QactiveProviderFactory providerFactory,
        QbservableServiceOptions options,
        CreateServiceProxyDelegates<TSource, TResult> delegates)
      {
        new PermissionSet(PermissionState.Unrestricted).Assert();

        QactiveProvider provider;
        try
        {
          provider = providerFactory.Create();

          InitializeInFullTrust(provider);
        }
        finally
        {
          PermissionSet.RevertAssert();
        }

        Func<IObservable<TSource>, IQbservable<TResult>> service;

        /* Retrieving a cross-domain delegate always fails with a SecurityException due to 
         * a Demand for ReflectionPermission, regardless of whether that permission is asserted
         * here or even whether full trust is asserted (commented line).  It is unclear why the
         * assertions don't work.  The only solution appears to be that the delegates must 
         * point to public members.
         * 
         * (Alternatively, adding the ReflectionPermission to the minimum permission set of the 
         * AppDomain works as well, but it's more secure to disallow it entirely to prevent 
         * reflection from executing within clients' expression trees, just in case the host 
         * relaxes the service options to allow unrestricted expressions constrained only by 
         * the minimum AppDomain permissions; i.e., The Principle of Least Surprise.)
         * 
         * new PermissionSet(PermissionState.Unrestricted).Assert();
         */

        try
        {
          service = delegates.Service;
        }
        catch (SecurityException ex)
        {
          throw new ArgumentException(Errors.CreateServiceDelegatesNotPublic, ex);
        }
        finally
        {
          /* This line is unnecessary - see comments above.
           * PermissionSet.RevertAssert();
           */
        }

        return QbservableServer.CreateService(provider, options, service).RemotableWithoutConfiguration();
      }

      [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
      private static void InitializeInFullTrust(IQactiveProvider provider)
      {
        // Rx demands full trust for the static initializer of the Observable class (as of v2.2.5)
        Observable.ToObservable(new string[0]);

        provider.InitializeSecureServer();
      }

      public override object InitializeLifetimeService()
      {
        return null;
      }
    }
  }
}
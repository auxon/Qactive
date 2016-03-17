using System;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using System.Threading;
using Qactive.Properties;

namespace Qactive
{
  public static partial class QbservableTcpServer
  {
    private static int appDomainNumber;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Too many overloads.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<TcpClientTermination> CreateService<TSource, TResult>(
      AppDomainSetup appDomainSetup,
      IPEndPoint endPoint,
      Func<IObservable<TSource>, IObservable<TResult>> service,
      [CallerMemberName] string appDomainBaseName = null,
      params Assembly[] fullTrustAssemblies)
    {
      return CreateService<TSource, TResult>(appDomainSetup, endPoint, new QbservableServiceConverter<TSource, TResult>(service).Convert, appDomainBaseName, fullTrustAssemblies);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Too many overloads.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<TcpClientTermination> CreateService<TSource, TResult>(
      AppDomainSetup appDomainSetup,
      IPEndPoint endPoint,
      QbservableServiceOptions options,
      Func<IObservable<TSource>, IObservable<TResult>> service,
      [CallerMemberName] string appDomainBaseName = null,
      params Assembly[] fullTrustAssemblies)
    {
      return CreateService<TSource, TResult>(appDomainSetup, endPoint, options, new QbservableServiceConverter<TSource, TResult>(service).Convert, appDomainBaseName, fullTrustAssemblies);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Too many overloads.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<TcpClientTermination> CreateService<TSource, TResult>(
      AppDomainSetup appDomainSetup,
      IPEndPoint endPoint,
      Func<IRemotingFormatter> formatterFactory,
      Func<IObservable<TSource>, IObservable<TResult>> service,
      [CallerMemberName] string appDomainBaseName = null,
      params Assembly[] fullTrustAssemblies)
    {
      return CreateService<TSource, TResult>(appDomainSetup, endPoint, formatterFactory, new QbservableServiceConverter<TSource, TResult>(service).Convert, appDomainBaseName, fullTrustAssemblies);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Too many overloads.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<TcpClientTermination> CreateService<TSource, TResult>(
      AppDomainSetup appDomainSetup,
      IPEndPoint endPoint,
      Func<IRemotingFormatter> formatterFactory,
      QbservableServiceOptions options,
      Func<IObservable<TSource>, IObservable<TResult>> service,
      [CallerMemberName] string appDomainBaseName = null,
      params Assembly[] fullTrustAssemblies)
    {
      return CreateService<TSource, TResult>(appDomainSetup, endPoint, formatterFactory, options, new QbservableServiceConverter<TSource, TResult>(service).Convert, appDomainBaseName, fullTrustAssemblies);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Too many overloads.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<TcpClientTermination> CreateService<TSource, TResult>(
      AppDomainSetup appDomainSetup,
      IPEndPoint endPoint,
      Func<IObservable<TSource>, IQbservable<TResult>> service,
      [CallerMemberName] string appDomainBaseName = null,
      params Assembly[] fullTrustAssemblies)
    {
      return CreateService<TSource, TResult>(appDomainSetup, endPoint, CreateDefaultFormatter, QbservableServiceOptions.Default, service, appDomainBaseName, fullTrustAssemblies);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Too many overloads.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<TcpClientTermination> CreateService<TSource, TResult>(
      AppDomainSetup appDomainSetup,
      IPEndPoint endPoint,
      QbservableServiceOptions options,
      Func<IObservable<TSource>, IQbservable<TResult>> service,
      [CallerMemberName] string appDomainBaseName = null,
      params Assembly[] fullTrustAssemblies)
    {
      return CreateService<TSource, TResult>(appDomainSetup, endPoint, CreateDefaultFormatter, options, service, appDomainBaseName, fullTrustAssemblies);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Too many overloads.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<TcpClientTermination> CreateService<TSource, TResult>(
      AppDomainSetup appDomainSetup,
      IPEndPoint endPoint,
      Func<IRemotingFormatter> formatterFactory,
      Func<IObservable<TSource>, IQbservable<TResult>> service,
      [CallerMemberName] string appDomainBaseName = null,
      params Assembly[] fullTrustAssemblies)
    {
      return CreateService<TSource, TResult>(appDomainSetup, endPoint, formatterFactory, QbservableServiceOptions.Default, service, appDomainBaseName, fullTrustAssemblies);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Too many overloads.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<TcpClientTermination> CreateService<TSource, TResult>(
      AppDomainSetup appDomainSetup,
      IPEndPoint endPoint,
      Func<IRemotingFormatter> formatterFactory,
      QbservableServiceOptions options,
      Func<IObservable<TSource>, IQbservable<TResult>> service,
      [CallerMemberName] string appDomainBaseName = null,
      params Assembly[] fullTrustAssemblies)
    {
      var permissions = new PermissionSet(PermissionState.None);

      return CreateService<TSource, TResult>(appDomainSetup, permissions, endPoint, formatterFactory, options, service, appDomainBaseName, fullTrustAssemblies);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2103:ReviewImperativeSecurity", Justification = "False positive; The Address is converted to a string before being passed to the permission, so it cannot be mutated later.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "AppDomain setup is very finicky and requires lots of concrete type references.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Too many overloads.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public static IObservable<TcpClientTermination> CreateService<TSource, TResult>(
      AppDomainSetup appDomainSetup,
      PermissionSet permissions,
      IPEndPoint endPoint,
      Func<IRemotingFormatter> formatterFactory,
      QbservableServiceOptions options,
      Func<IObservable<TSource>, IQbservable<TResult>> service,
      [CallerMemberName] string appDomainBaseName = null,
      params Assembly[] fullTrustAssemblies)
    {
      var minimumPermissions = new PermissionSet(PermissionState.None);

      minimumPermissions.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
      minimumPermissions.AddPermission(new SocketPermission(NetworkAccess.Accept, TransportType.Tcp, endPoint.Address.ToString(), endPoint.Port));

      var entryAssembly = Assembly.GetEntryAssembly();

      var domain = AppDomain.CreateDomain(
        Interlocked.Increment(ref appDomainNumber) + ':' + appDomainBaseName,
        null,
        appDomainSetup, minimumPermissions.Union(permissions),
        new[]
        {
          typeof(QbservableTcpServer).Assembly.Evidence.GetHostEvidence<StrongName>(),
          typeof(System.Reactive.Linq.Observable).Assembly.Evidence.GetHostEvidence<StrongName>(),
          typeof(System.Reactive.Linq.Qbservable).Assembly.Evidence.GetHostEvidence<StrongName>(),
          typeof(System.Reactive.Notification).Assembly.Evidence.GetHostEvidence<StrongName>(),
          typeof(System.Reactive.IEventPattern<,>).Assembly.Evidence.GetHostEvidence<StrongName>(),
          typeof(System.Reactive.Concurrency.TaskPoolScheduler).Assembly.Evidence.GetHostEvidence<StrongName>()
        }
        .Concat(entryAssembly == null ? new StrongName[0] : new[] { entryAssembly.Evidence.GetHostEvidence<StrongName>() })
        .Concat(fullTrustAssemblies.Select(assembly => assembly.Evidence.GetHostEvidence<StrongName>()))
        .ToArray());

      try
      {
        var handle = Activator.CreateInstanceFrom(
          domain,
          typeof(CreateServiceProxy<TSource, TResult>).Assembly.ManifestModule.FullyQualifiedName,
          typeof(CreateServiceProxy<TSource, TResult>).FullName);

        var proxy = (CreateServiceProxy<TSource, TResult>)handle.Unwrap();

        return proxy.CreateService(endPoint, options, new CreateServiceProxyDelegates<TSource, TResult>(formatterFactory, service))
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
      public Func<IRemotingFormatter> FormatterFactory
      {
        get
        {
          return formatterFactory;
        }
      }

      public Func<IObservable<TSource>, IQbservable<TResult>> Service
      {
        get
        {
          return service;
        }
      }

      private readonly Func<IRemotingFormatter> formatterFactory;
      private readonly Func<IObservable<TSource>, IQbservable<TResult>> service;

      public CreateServiceProxyDelegates(
        Func<IRemotingFormatter> formatterFactory,
        Func<IObservable<TSource>, IQbservable<TResult>> service)
      {
        this.formatterFactory = formatterFactory;
        this.service = service;
      }

      public override object InitializeLifetimeService()
      {
        return null;
      }
    }

    private sealed class CreateServiceProxy<TSource, TResult> : MarshalByRefObject
    {
      public IObservable<TcpClientTermination> CreateService(
        IPEndPoint endPoint,
        QbservableServiceOptions options,
        CreateServiceProxyDelegates<TSource, TResult> delegates)
      {
        new PermissionSet(PermissionState.Unrestricted).Assert();

        try
        {
          InitializeInFullTrust();
        }
        finally
        {
          PermissionSet.RevertAssert();
        }

        Func<IRemotingFormatter> formatterFactory;
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
          formatterFactory = delegates.FormatterFactory;
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

        return QbservableTcpServer
          .CreateService(endPoint, formatterFactory, options, service)
          .RemotableWithoutConfiguration();
      }

      [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
      private static void InitializeInFullTrust()
      {
        // Rx demands full trust for the static initializer of the Observable class (as of v2.2.5)
        Observable.ToObservable(new string[0]);
      }

      public override object InitializeLifetimeService()
      {
        return null;
      }
    }
  }
}
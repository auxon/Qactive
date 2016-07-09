using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;

namespace Qactive
{
  [ContractClass(typeof(QactiveProviderContract))]
  public abstract class QactiveProvider :
#if REMOTING
    MarshalByRefObject,
#endif
    IQactiveProvider
  {
    public Type SourceType { get; }

    public LocalEvaluator ClientEvaluator { get; }

    public bool IsServer => ClientEvaluator == null;

    public object Argument { get; }

    /// <summary>
    /// This is purely for diagnostic purposes only. The value returned may be used to identify clients in logs. It's called immediately before the provider's <c>CreateQuery</c> method is invoked.
    /// </summary>
    protected abstract object Id { get; }

    /// <summary>
    /// Constructs an instance of a server provider.
    /// </summary>
    protected QactiveProvider()
    {
    }

    /// <summary>
    /// Constructs an instance of a client provider.
    /// </summary>
    protected QactiveProvider(Type sourceType, LocalEvaluator clientEvaluator)
    {
      Contract.Requires(sourceType != null);
      Contract.Requires(clientEvaluator != null);

      SourceType = sourceType;
      ClientEvaluator = clientEvaluator;
    }

    /// <summary>
    /// Constructs an instance of a client provider.
    /// </summary>
    protected QactiveProvider(Type sourceType, LocalEvaluator clientEvaluator, object argument)
      : this(sourceType, clientEvaluator)
    {
      Contract.Requires(sourceType != null);
      Contract.Requires(clientEvaluator != null);

      Argument = argument;
    }

    public IQbservable<TResult> CreateQuery<TResult>()
      => new ClientQuery<TResult>(Id, this);

    public IQbservable<TResult> CreateQuery<TResult>(Expression expression)
      => new ClientQuery<TResult>(Id, this, expression);

    public abstract IObservable<TResult> Connect<TResult>(Func<IQbservableProtocol, Expression> prepareExpression);

    public abstract IObservable<ClientTermination> Listen(QbservableServiceOptions options, Func<IQbservableProtocol, IParameterizedQbservableProvider> providerFactory);

    [Conditional("TRACE")]
    protected void Starting(object data = null, [CallerMemberName]string label = null)
      => Log.Starting(IsServer, isServerReceiving: false, sourceId: Id, label: label, data: data);

    [Conditional("TRACE")]
    protected void Started(object data = null, [CallerMemberName]string label = null)
      => Log.Started(IsServer, isServerReceiving: false, sourceId: Id, label: label, data: data);

    [Conditional("TRACE")]
    protected void ReceivingConnection(object idOverride = null, object data = null, [CallerMemberName]string label = null)
      => Log.Starting(isServer: true, isServerReceiving: true, sourceId: idOverride ?? Id, label: label, data: data);

    [Conditional("TRACE")]
    protected void ReceivedConnection(object idOverride = null, object data = null, [CallerMemberName]string label = null)
      => Log.Started(isServer: true, isServerReceiving: true, sourceId: idOverride ?? Id, label: label, data: data);

    [Conditional("TRACE")]
    protected void Stopping(object data = null, [CallerMemberName]string label = null)
      => Log.Stopping(IsServer, isServerReceiving: false, sourceId: Id, label: label, data: data);

    [Conditional("TRACE")]
    protected void Stopped(object data = null, [CallerMemberName]string label = null)
      => Log.Stopped(IsServer, isServerReceiving: false, sourceId: Id, label: label, data: data);

    [Conditional("TRACE")]
    protected void Disconnecting(object idOverride = null, object data = null, [CallerMemberName]string label = null)
      => Log.DuplexDisconnecting(IsServer, isClientReceiving: false, sourceId: idOverride ?? Id, label: label, data: data);

    [Conditional("TRACE")]
    protected void Disconnected(object idOverride = null, object data = null, [CallerMemberName]string label = null)
      => Log.DuplexDisconnected(IsServer, isClientReceiving: false, sourceId: idOverride ?? Id, label: label, data: data);

    [Conditional("TRACE")]
    protected void ReceivingDisconnection(object data = null, [CallerMemberName]string label = null)
      => Log.DuplexDisconnecting(IsServer, isClientReceiving: true, sourceId: Id, label: label, data: data);

    [Conditional("TRACE")]
    protected void ReceivedDisconnection(object data = null, [CallerMemberName]string label = null)
      => Log.DuplexDisconnected(IsServer, isClientReceiving: true, sourceId: Id, label: label, data: data);

    void IQactiveProvider.InitializeSecureServer()
      => InitializeSecureServer();

    protected virtual void InitializeSecureServer()
    {
    }
  }

  [ContractClassFor(typeof(QactiveProvider))]
  internal abstract class QactiveProviderContract : QactiveProvider
  {
    protected override object Id
    {
      get
      {
        Contract.Ensures(Contract.Result<object>() != null);
        return null;
      }
    }

    protected QactiveProviderContract(Type sourceType, LocalEvaluator clientEvaluator)
      : base(sourceType, clientEvaluator)
    {
    }
  }
}
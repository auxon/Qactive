using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reactive;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Threading.Tasks;

namespace Qactive
{
  /// <summary>
  /// Provides the basic algorithm for a single observable communication channel between a client and server.
  /// </summary>
  [ContractClass(typeof(QbservableProtocolContract<>))]
  public abstract class QbservableProtocol<TSource> : QbservableProtocol
  {
    protected TSource Source { get; }

    protected IRemotingFormatter Formatter { get; }

    internal QbservableProtocol(TSource source, IRemotingFormatter formatter, CancellationToken cancel)
      : base(cancel)
    {
      Contract.Requires(source != null);
      Contract.Requires(formatter != null);
      Contract.Ensures(IsClient);

      Source = source;
      Formatter = formatter;
    }

    internal QbservableProtocol(TSource source, IRemotingFormatter formatter, QbservableServiceOptions serviceOptions, CancellationToken cancel)
      : base(serviceOptions, cancel)
    {
      Contract.Requires(source != null);
      Contract.Requires(formatter != null);
      Contract.Requires(serviceOptions != null);
      Contract.Ensures(!IsClient);

      Source = source;
      Formatter = formatter;
    }

    [ContractInvariantMethod]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
    private void ObjectInvariant()
    {
      Contract.Invariant(Source != null);
      Contract.Invariant(Formatter != null);
    }
  }

  [ContractClassFor(typeof(QbservableProtocol<>))]
  internal abstract class QbservableProtocolContract<TSource> : QbservableProtocol<TSource>
  {
    protected QbservableProtocolContract()
      : base(default(TSource), null, CancellationToken.None)
    {
    }

    public override TSink FindSink<TSink>()
    {
      throw new NotImplementedException();
    }

    public override TSink GetOrAddSink<TSink>(Func<TSink> createSink)
    {
      Contract.Requires(createSink != null);
      return default(TSink);
    }

    protected override IObservable<TResult> ClientReceive<TResult>() => null;

    protected override Task ClientSendQueryAsync(Expression expression, object argument)
    {
      // expression can be null
      return null;
    }

    protected override Task<Tuple<Expression, object>> ServerReceiveQueryAsync()
    {
      throw new NotImplementedException();
    }

    protected override Task ServerSendAsync(NotificationKind kind, object data)
    {
      throw new NotImplementedException();
    }

    protected override Task ShutdownCoreAsync()
    {
      throw new NotImplementedException();
    }

    internal override IClientDuplexQbservableProtocolSink CreateClientDuplexSinkInternal()
    {
      throw new NotImplementedException();
    }

    internal override IServerDuplexQbservableProtocolSink CreateServerDuplexSinkInternal()
    {
      throw new NotImplementedException();
    }

    internal override Task InitializeSinksAsync()
    {
      throw new NotImplementedException();
    }

    internal override Task ServerReceiveAsync() => null;
  }
}
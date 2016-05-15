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
    protected IRemotingFormatter Formatter { get; }

    protected TSource Source { get; }

    internal QbservableProtocol(TSource source, IRemotingFormatter formatter, CancellationToken cancel)
      : base(cancel)
    {
      Contract.Ensures(IsClient);

      Source = source;
      Formatter = formatter;
    }

    internal QbservableProtocol(TSource source, IRemotingFormatter formatter, QbservableServiceOptions serviceOptions, CancellationToken cancel)
      : base(serviceOptions, cancel)
    {
      Contract.Ensures(!IsClient);

      Source = source;
      Formatter = formatter;
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

    protected override IObservable<TResult> ClientReceive<TResult>()
    {
      Contract.Ensures(Contract.Result<IObservable<TResult>>() != null);
      return null;
    }

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

    internal override Task ServerReceiveAsync()
    {
      Contract.Requires(!IsClient);
      return null;
    }
  }
}
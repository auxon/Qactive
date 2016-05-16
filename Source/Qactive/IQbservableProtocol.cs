using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace Qactive
{
  [ContractClass(typeof(IQbservableProtocolContract))]
  public interface IQbservableProtocol : IDisposable
  {
    bool IsClient { get; }

    object CurrentClientId { get; }

    IReadOnlyCollection<ExceptionDispatchInfo> Exceptions { get; }

    QbservableProtocolShutdownReason ShutdownReason { get; }

    TSink FindSink<TSink>();

    TSink GetOrAddSink<TSink>(Func<TSink> createSink);

    IClientDuplexQbservableProtocolSink CreateClientDuplexSink();

    IServerDuplexQbservableProtocolSink CreateServerDuplexSink();

    IObservable<TResult> ExecuteClient<TResult>(Expression expression, object argument);

    Task ExecuteServerAsync(object clientId, IQbservableProvider provider);

    void CancelAllCommunication();

    void CancelAllCommunication(ExceptionDispatchInfo exception);
  }

  [ContractClassFor(typeof(IQbservableProtocol))]
  internal abstract class IQbservableProtocolContract : IQbservableProtocol
  {
    public bool IsClient { get; }

    public object CurrentClientId { get; }

    public IReadOnlyCollection<ExceptionDispatchInfo> Exceptions
    {
      get
      {
        Contract.Ensures(Contract.Result<IReadOnlyCollection<ExceptionDispatchInfo>>() != null);
        return null;
      }
    }

    public QbservableProtocolShutdownReason ShutdownReason { get; }

    public TSink FindSink<TSink>()
    {
      return default(TSink);
    }

    public TSink GetOrAddSink<TSink>(Func<TSink> createSink)
    {
      Contract.Requires(createSink != null);
      return default(TSink);
    }

    public IClientDuplexQbservableProtocolSink CreateClientDuplexSink()
    {
      Contract.Ensures(Contract.Result<IClientDuplexQbservableProtocolSink>() != null);
      return null;
    }

    public IServerDuplexQbservableProtocolSink CreateServerDuplexSink()
    {
      Contract.Ensures(Contract.Result<IServerDuplexQbservableProtocolSink>() != null);
      return null;
    }

    public IObservable<TResult> ExecuteClient<TResult>(Expression expression, object argument)
    {
      Contract.Requires(IsClient);
      Contract.Requires(expression != null);
      Contract.Ensures(Contract.Result<IObservable<TResult>>() != null);
      return null;
    }

    public Task ExecuteServerAsync(object clientId, IQbservableProvider provider)
    {
      Contract.Requires(!IsClient);
      Contract.Requires(clientId != null);
      Contract.Requires(provider != null);
      Contract.Ensures(Contract.Result<Task>() != null);
      return null;
    }

    public void CancelAllCommunication()
    {
    }

    public void CancelAllCommunication(ExceptionDispatchInfo exception)
    {
      Contract.Requires(exception != null);
    }

    public void Dispose()
    {
    }
  }
}
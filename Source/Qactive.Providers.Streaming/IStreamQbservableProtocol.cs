using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace Qactive
{
  [ContractClass(typeof(IStreamQbservableProtocolContract))]
  public interface IStreamQbservableProtocol : IQbservableProtocol
  {
    Task SendAsync(byte[] buffer, int offset, int count);

    Task ReceiveAsync(byte[] buffer, int offset, int count);
  }

  [ContractClassFor(typeof(IStreamQbservableProtocol))]
  internal abstract class IStreamQbservableProtocolContract : IStreamQbservableProtocol
  {
    public Task ReceiveAsync(byte[] buffer, int offset, int count)
    {
      Contract.Requires(buffer != null);
      Contract.Requires(offset >= 0);
      Contract.Requires(count >= 0);
      Contract.Requires(offset + count <= buffer.Length);
      return null;
    }

    public Task SendAsync(byte[] buffer, int offset, int count)
    {
      Contract.Requires(buffer != null);
      Contract.Requires(offset >= 0);
      Contract.Requires(count >= 0);
      Contract.Requires(offset + count <= buffer.Length);
      return null;
    }

    public bool IsClient { get; }

    public object ClientId { get; set; }

    public IReadOnlyCollection<ExceptionDispatchInfo> Exceptions { get; }

    public QbservableProtocolShutdownReason ShutdownReason { get; }

    public void CancelAllCommunication()
    {
    }

    public void CancelAllCommunication(ExceptionDispatchInfo exception)
    {
    }

    public IClientDuplexQbservableProtocolSink CreateClientDuplexSink()
    {
      return null;
    }

    public IServerDuplexQbservableProtocolSink CreateServerDuplexSink()
    {
      return null;
    }

    public void Dispose()
    {
    }

    public IObservable<TResult> ExecuteClient<TResult>(Expression expression, object argument)
    {
      return null;
    }

    public Task ExecuteServerAsync(IQbservableProvider provider)
    {
      return null;
    }

    public TSink FindSink<TSink>()
    {
      return default(TSink);
    }

    public TSink GetOrAddSink<TSink>(Func<TSink> createSink)
    {
      return default(TSink);
    }
  }
}

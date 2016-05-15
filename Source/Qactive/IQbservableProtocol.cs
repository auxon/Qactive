using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace Qactive
{
  public interface IQbservableProtocol : IDisposable
  {
    IReadOnlyCollection<ExceptionDispatchInfo> Exceptions { get; }

    QbservableProtocolShutdownReason ShutdownReason { get; }

    object CurrentClientId { get; }

    TSink FindSink<TSink>();

    TSink GetOrAddSink<TSink>(Func<TSink> createSink);

    IClientDuplexQbservableProtocolSink CreateClientDuplexSink();

    IServerDuplexQbservableProtocolSink CreateServerDuplexSink();

    IObservable<TResult> ExecuteClient<TResult>(Expression expression, object argument);

    Task ExecuteServerAsync(object clientId, IQbservableProvider provider);

    void CancelAllCommunication();

    void CancelAllCommunication(ExceptionDispatchInfo exception);
  }
}
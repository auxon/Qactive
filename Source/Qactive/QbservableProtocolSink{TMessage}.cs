using System;
using System.Diagnostics.Contracts;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace Qactive
{
  [ContractClass(typeof(QbservableProtocolSinkContract<,>))]
  public abstract class QbservableProtocolSink<TSource, TMessage>
    where TMessage : IProtocolMessage
  {
    private readonly ISubject<ExceptionDispatchInfo, ExceptionDispatchInfo> exceptions = Subject.Synchronize(new Subject<ExceptionDispatchInfo>());

    public IObservable<ExceptionDispatchInfo> Exceptions => exceptions.AsObservable();

    public abstract Task InitializeAsync(QbservableProtocol<TSource, TMessage> protocol, CancellationToken cancel);

    public abstract Task<TMessage> SendingAsync(TMessage message, CancellationToken cancel);

    public abstract Task<TMessage> ReceivingAsync(TMessage message, CancellationToken cancel);

    protected void Fail(ExceptionDispatchInfo error)
    {
      Contract.Requires(error != null);

      exceptions.OnNext(error);
    }
  }

  [ContractClassFor(typeof(QbservableProtocolSink<,>))]
  internal abstract class QbservableProtocolSinkContract<TSource, TMessage> : QbservableProtocolSink<TSource, TMessage>
    where TMessage : IProtocolMessage
  {
    public override Task InitializeAsync(QbservableProtocol<TSource, TMessage> protocol, CancellationToken cancel)
    {
      Contract.Requires(protocol != null);
      return null;
    }

    public override Task<TMessage> SendingAsync(TMessage message, CancellationToken cancel)
    {
      Contract.Requires(message != null);
      return null;
    }

    public override Task<TMessage> ReceivingAsync(TMessage message, CancellationToken cancel)
    {
      Contract.Requires(message != null);
      return null;
    }
  }
}
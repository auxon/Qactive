using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace Qactive
{
  [ContractClass(typeof(QbservableProtocolSinkContract<,>))]
  public abstract class QbservableProtocolSink<TSource, TMessage>
    where TMessage : IProtocolMessage
  {
    public abstract Task InitializeAsync(QbservableProtocol<TSource, TMessage> protocol, CancellationToken cancel);

    public abstract Task<TMessage> SendingAsync(TMessage message, CancellationToken cancel);

    public abstract Task<TMessage> ReceivingAsync(TMessage message, CancellationToken cancel);
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
using System.Threading;
using System.Threading.Tasks;

namespace Qactive
{
  public abstract class QbservableProtocolSink<TSource, TMessage>
    where TMessage : IProtocolMessage
  {
    public abstract Task InitializeAsync(QbservableProtocol<TSource, TMessage> protocol, CancellationToken cancel);

    public abstract Task<TMessage> SendingAsync(TMessage message, CancellationToken cancel);

    public abstract Task<TMessage> ReceivingAsync(TMessage message, CancellationToken cancel);
  }
}
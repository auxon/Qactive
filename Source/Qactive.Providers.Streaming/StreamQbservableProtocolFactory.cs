using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Threading;

namespace Qactive
{
  public static class StreamQbservableProtocolFactory
  {
    public static IStreamQbservableProtocol CreateClient(Stream stream, IRemotingFormatter formatter, CancellationToken cancel)
    {
      Contract.Requires(stream != null);
      Contract.Requires(formatter != null);
      Contract.Ensures(Contract.Result<IStreamQbservableProtocol>() != null);

      return new StreamQbservableProtocol(stream, formatter, cancel);
    }

    public static IStreamQbservableProtocol CreateServer(Stream stream, IRemotingFormatter formatter, QbservableServiceOptions serviceOptions, CancellationToken cancel)
    {
      Contract.Requires(stream != null);
      Contract.Requires(formatter != null);
      Contract.Requires(serviceOptions != null);
      Contract.Ensures(Contract.Result<IStreamQbservableProtocol>() != null);

      return new StreamQbservableProtocol(stream, formatter, serviceOptions, cancel);
    }
  }
}
using System.Threading.Tasks;

namespace Qactive
{
  public interface IStreamQbservableProtocol : IQbservableProtocol
  {
    Task SendAsync(byte[] buffer, int offset, int count);

    Task ReceiveAsync(byte[] buffer, int offset, int count);
  }
}

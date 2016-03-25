using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization.Formatters.Binary;

namespace Qactive
{
  public static class TcpQactiveDefaults
  {
    // This method must be public otherwise CreateService fails inside of a new AppDomain - see CreateServiceProxy comments
    public static IRemotingFormatter CreateDefaultFormatter()
    {
      return new BinaryFormatter();
    }
  }
}

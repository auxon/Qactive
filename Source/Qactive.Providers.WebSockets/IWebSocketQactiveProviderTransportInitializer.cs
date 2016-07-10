using System;
using System.Diagnostics.Contracts;
using System.Net.WebSockets;
using System.Runtime.Remoting.Messaging;

namespace Qactive
{
  /// <remarks>
  /// A type is required to define the methods that this interface contains rather than simply having delegate parameters in the provider factory methods
  /// because the secure overloads require the functions to be called within the sandbox AppDomain, thus either remoting permission is required to serialize 
  /// the delegates across the AppDomain, which is undesireable, or a type must be instantiated from within the AppDomain itself. The latter choice is used.
  /// </remarks>
  [ContractClass(typeof(IWebSocketQactiveProviderTransportInitializerContract))]
  public interface IWebSocketQactiveProviderTransportInitializer
  {
    void StartedListener(int serverNumber, Uri uri);

    void StoppedListener(int serverNumber, Uri uri);

    /// <summary>
    /// Prepares the socket for transport; e.g., you could call <see cref="Socket.SetSocketOption(SocketOptionLevel, SocketOptionName, bool)"/>
    /// to disable the Nagel algorithm and to enable keep-alives.
    /// </summary>
    /// <remarks>
    /// This method is called once for the client socket when used for the client provider.
    /// This method is called once for each connected socket when used for the server provider.
    /// </remarks>
    void Prepare(WebSocket socket);

    /// <summary>
    /// Returns the transport formatter to be used for a socket, or returns null to use the default formatter.
    /// </summary>
    /// <remarks>
    /// This method is called once for the client socket when used for the client provider.
    /// This method is called once for each connected socket when used for the server provider.
    /// </remarks>
    IRemotingFormatter CreateFormatter();
  }

  [ContractClassFor(typeof(IWebSocketQactiveProviderTransportInitializer))]
  internal abstract class IWebSocketQactiveProviderTransportInitializerContract : IWebSocketQactiveProviderTransportInitializer
  {
    public void StartedListener(int serverNumber, Uri uri)
    {
      Contract.Requires(uri != null);
    }

    public void StoppedListener(int serverNumber, Uri uri)
    {
      Contract.Requires(uri != null);
    }

    public void Prepare(WebSocket socket)
    {
      Contract.Requires(socket != null);
    }

    public IRemotingFormatter CreateFormatter() => null;
  }
}

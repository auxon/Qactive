using System;
using System.Net;
using SharedLibrary;

namespace QbservableClient
{
  class Program
  {
    public static readonly IPEndPoint TimerServiceEndPoint = new IPEndPoint(IPAddress.Loopback, port: 49593);
    public static readonly IPEndPoint BasicServiceEndPoint = new IPEndPoint(IPAddress.Loopback, port: 38245);
    public static readonly IPEndPoint AdvancedServiceEndPoint = new IPEndPoint(IPAddress.Loopback, port: 38246);
    public static readonly IPEndPoint ChatServiceEndPoint = new IPEndPoint(IPAddress.Loopback, port: 19841);
    public static readonly IPEndPoint SandboxedServiceEndPoint = new IPEndPoint(IPAddress.Loopback, port: 7223);
    public static readonly IPEndPoint LimitedServiceEndPoint = new IPEndPoint(IPAddress.Loopback, port: 9105);

    static void Main()
    {
      ConsoleTrace.PrintCurrentMethod();

      Console.WriteLine();
      Console.WriteLine("Press any key to connect to the server.  Make sure that it's running first!");

      Console.ReadKey(intercept: true);

      //new BasicClient().Run();
      //new TimerClient().Run();
      //new BasicFeedAggregationClient().Run();
      //new AdvancedClient().Run();
      //new DuplexClient().Run();
      //new ChatClient().Run();
      new MaliciousClient().Run();
    }
  }
}
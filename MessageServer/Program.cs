using System.Net;
using CoreServer;

namespace MessageServer
{
    class Program
    {
        static void Main(string[] args)
        {
            IPAddress addr = IPAddress.Any;
            CoreServer.MessageServer server = new CoreServer.MessageServer(addr, 3807);
            server.Start();
        }
    }
}

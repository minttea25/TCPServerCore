using System;
using System.Net;

using ServerCoreTCP;

namespace TCPServer
{
    class Program
    {
        static void Main(string[] args)
        {
            string host = Dns.GetHostName(); // local host name of my pc
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new(address: ipAddr, port: 8888);

            Listener listener = new(endPoint, () => { return SessionManager.Instance.CreateNewSession(); });
            listener.Listen(register: 10, backLog: 100);

            while (true)
            {
            }
        }
    }
}

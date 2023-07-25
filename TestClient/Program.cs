using System;
using System.Net;
using System.Threading;

using ServerCoreTCP;

namespace TestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread.Sleep(1000);

            string host = Dns.GetHostName(); // local host name of my pc
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new(address: ipAddr, port: 8888);

            Connector connector = new();
            connector.Connect(endPoint, () => { return SessionManager.Instance.CreateNewSession(); }, 10);

            while (true)
            {
            }
        }
    }
}

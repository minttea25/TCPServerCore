using System;
using System.Net;

using ServerCoreTCP;
using ServerCoreTCP.Utils;

namespace TCPServer
{
    class Program
    {
        public readonly static Room Room = new();
        static void FlushRoom()
        {
            Room.AddJob(() => Room.Flush());
            JobTimer.Instance.Push(FlushRoom, 500);
        }

        static void Main(string[] args)
        {
            string host = Dns.GetHostName(); // local host name of my pc
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new(address: ipAddr, port: 8888);

            Listener listener = new(endPoint, () => { return SessionManager.Instance.CreateNewSession(); });
            listener.Listen(register: 10, backLog: 100);

            // exec right now
            JobTimer.Instance.Push(FlushRoom, 0);

            while (true)
            {
                JobTimer.Instance.Flush();
            }
        }
    }
}

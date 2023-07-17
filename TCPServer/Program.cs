//#define MEMORY_BUFFER

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using ServerCoreTCP;

namespace TCPServer
{
    class ClientSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine("OnConnected: {0}", endPoint);
        }

        public override void OnDisconnected(EndPoint endPoint, object error = null)
        {
            Console.WriteLine("OnDisconnected: {0}", endPoint);
        }

        public override void OnRecv(ArraySegment<byte> buffer)
        {
            TClass t = new();
            t.Deserialize(buffer);
            Console.WriteLine("ArraySegment - Received: {0}", t);
        }

        public override void OnRecv(Memory<byte> buffer)
        {
            TClass t = new();
            t.MDeserialize(buffer);
            Console.WriteLine("Memory - Received: {0}", t);
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine("Sent: {0} bytes", numOfBytes);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            string host = Dns.GetHostName(); // local host name of my pc
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new(address: ipAddr, port: 8888);

            var session = new ClientSession();
            Listener listener = new(endPoint, () => { return new ClientSession(); });
            listener.Listen(register: 10, backLog: 100);

            while (true)
            {
            }
        }
    }
}

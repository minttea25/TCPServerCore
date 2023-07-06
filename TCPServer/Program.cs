using System;
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

            // TEMP
            {
                Thread.Sleep(1000);
                string msg = "This is Server!";
                byte[] msgBuffer = Encoding.UTF8.GetBytes(msg);
                var segment = SendBufferTLS.Reserve(1024);

                Array.Copy(msgBuffer, 0, segment.Array, segment.Offset, msgBuffer.Length);
                var buffer = SendBufferTLS.Return(msgBuffer.Length);

                Send(buffer);
            }
        }

        public override void OnDisconnected(EndPoint endPoint, object error = null)
        {
            Console.WriteLine("OnDisconnected: {0}", endPoint);
        }

        public override int OnRecv(ArraySegment<byte> buffer)
        {
            // TEMP
            Console.WriteLine("Received: {0}", buffer.Count);
            Console.WriteLine("Contents: {0}", Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count));
            return buffer.Count;
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
            listener.Listen(register: 10);

            while (true)
            {
                ;
            }
        }
    }
}

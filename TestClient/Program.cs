using System;
using System.Net;
using System.Text;
using System.Threading;

using ServerCoreTCP;

namespace TestClient
{
    public class ServerSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine("OnConnected: {0}", endPoint);

            for (int i=0; i<5; i++)
            {
                byte[] buffer1 = Encoding.UTF8.GetBytes($"This is Client! {i}");
                Send(buffer1);
            }
            string msg = "Message";
            byte[] msgBuffer = Encoding.UTF8.GetBytes(msg);
            var segment = SendBufferTLS.Reserve(1024);

            Array.Copy(msgBuffer, 0, segment.Array, segment.Offset, msgBuffer.Length);
            var buffer = SendBufferTLS.Return(msgBuffer.Length);
            Send(buffer);
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
            Thread.Sleep(1000);

            string host = Dns.GetHostName(); // local host name of my pc
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new(address: ipAddr, port: 8888);

            Connector connector = new();
            ServerSession session = new();
            connector.Connect(endPoint, () => { return new ServerSession(); }, 5);

            while (true)
            {
                Thread.Sleep(3000);
            }
        }
    }
}

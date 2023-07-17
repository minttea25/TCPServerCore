//#define MEMORY_BUFFER

using System;
using System.Net;
using System.Text;
using System.Threading;

using ServerCoreTCP;

namespace TestClient
{
    public class ServerSession : Session
    {
        Random random = new();
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine("OnConnected: {0}", endPoint);

            for (int i = 1; i < 10; i++)
            {
                TClass t = new();
                t.id = (ushort)i;
                t.value = (float)random.NextDouble();
                t.msg = $"It is client : {i}";
                t.list.Add(new TClass.NestedT((uint)i, $"This is item {i}"));
                t.list.Add(new TClass.NestedT((uint)i + 1, $"This is item {i * 10}"));
                t.list.Add(new TClass.NestedT((uint)i + 2, $"This is item {i * 100}"));
#if MEMORY_BUFFER
                Send(t.MSerialize());
#else
                Send(t.Serialize());
#endif

                Thread.Sleep(200);
            }
        }

        public override void OnDisconnected(EndPoint endPoint, object error = null)
        {
            Console.WriteLine("OnDisconnected: {0}", endPoint);
        }

        public override void OnRecv(ArraySegment<byte> buffer)
        {
        }

        public override void OnRecv(Memory<byte> buffer)
        {
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
            connector.Connect(endPoint, () => { return new ServerSession(); }, 1000);

            while (true)
            {
            }
        }
    }
}

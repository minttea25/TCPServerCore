//#define MEMORY_BUFFER

using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

using ServerCoreTCP;

using TestNamespace;

namespace TestClient
{
    public class ServerSession : Session
    {
        Random random = new();
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine("OnConnected: {0}", endPoint);

            for(int i=0; i<5; i++)
            {
                TestPacket p = new();
                p.itemId = (ushort)random.Next(0, 1000);
                p.titles.Add($"{i} - title");
                p.titles.Add($"{i * 10} - title");
                p.items = new()
                {
                    playerId = random.Next(),
                    playerName = $"name - {i}"
                };


#if MEMORY_BUFFER
                Send(p.MSerialize());
#else
                Send(p.Serialize());
#endif
                Console.WriteLine(p);
                Thread.Sleep(100);
            }

            

            for (int i = 0; i < 5; i++)
            {
                TestPacket2 p = new();
                p.itemId = random.NextDouble();
                p.numbers.Add(i);
                p.numbers.Add(i*10);
                p.numbers.Add(i*100);
                p.weapons.Add(new()
                {
                    date = 10f / i,
                    weaponId = new() { (ushort)random.Next(0, 10), (ushort)random.Next(0, 10) }
                });
                p.weapons.Add(new()
                {
                    date = 10f / i,
                    weaponId = new() { (ushort)random.Next(0, 10), (ushort)random.Next(0, 10) }
                });
#if MEMORY_BUFFER
                Send(p.MSerialize());
#else
                Send(p.Serialize());
#endif
                Console.WriteLine(p);
                Thread.Sleep(100);
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
            connector.Connect(endPoint, () => { return new ServerSession(); }, 1);

            while (true)
            {
            }
        }
    }
}

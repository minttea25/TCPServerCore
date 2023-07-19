//#define MEMORY_BUFFER

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using ServerCoreTCP;

using TestNamespace;

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
            int offset = sizeof(ushort);

            Packets pkt = (Packets)BitConverter.ToUInt16(buffer.Slice(offset, sizeof(ushort)));
            offset += sizeof(ushort);

            switch (pkt)
            {
                case Packets.TestPacket:
                    TestPacket t = new();
                    t.Deserialize(buffer);
                    Console.WriteLine(t);
                    break;
                case Packets.TestPacket2:
                    TestPacket2 t2 = new();
                    t2.Deserialize(buffer);
                    Console.WriteLine(t2);
                    break;
            }
        }

        public override void OnRecv(Memory<byte> buffer)
        {
            int offset = sizeof(ushort);

            Packets pkt = (Packets)BitConverter.ToUInt16(buffer.Span.Slice(offset, sizeof(ushort)));
            offset += sizeof(ushort);

            switch (pkt)
            {
                case Packets.TestPacket:
                    TestPacket t = new();
                    t.MDeserialize(buffer);
                    Console.WriteLine(t);
                    break;
                case Packets.TestPacket2:
                    TestPacket2 t2 = new();
                    t2.MDeserialize(buffer);
                    Console.WriteLine(t2);
                    break;
            }
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

using System;
using System.Net;
using System.Threading;
using ServerCoreTCP.ProtobufWrapper;

namespace TCPServer
{
    public class ClientSession : PacketSession
    {
        public readonly uint SessionId;

        public Room Room { get; set; }

        static Random rand = new();

        public ClientSession(uint sessionId)
        {
            SessionId = sessionId;
        }

        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine("OnConnected: {0}", endPoint);

            while (true)
            {
                Program.Room.AddJob(() => Program.Room.Enter(this));
                Thread.Sleep(rand.Next(1000, 5000));
            }
        }

        public override void OnDisconnected(EndPoint endPoint, object error = null)
        {
            SessionManager.Instance.Remove(this);

            Console.WriteLine("OnDisconnected: {0}", endPoint);
        }

        public override void OnRecv(ReadOnlySpan<byte> buffer)
        {
            PacketManager.Instance.OnRecvPacket(this, buffer);
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine("Sent: {0} bytes", numOfBytes);
        }
    }
}

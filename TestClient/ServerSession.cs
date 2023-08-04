using System;
using System.Net;
using System.Threading;
using ServerCoreTCP;

using ServerCoreTCP.ProtobufWrapper;

namespace TestClient
{
    public class ServerSession : PacketSession
    {
        readonly Random random = new();
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine("OnConnected: {0}", endPoint);
        }

        public override void OnDisconnected(EndPoint endPoint, object error = null)
        {
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

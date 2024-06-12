#if PROTOBUF
using Chat;

using ServerCoreTCP;

using System;
using System.Net;
using ServerCoreTCP.Protobuf;
using Google.Protobuf.WellKnownTypes;

namespace Chat.Protobuf
{
    public class ClientSession : PacketSession
    {
        public override void InitSession()
        {
        }

        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"Connected to {endPoint}");

            
        }

        public override void OnDisconnected(EndPoint endPoint, object error = null)
        {
            Console.WriteLine($"OnDisconnected");
        }

        public override void OnRecv(ReadOnlySpan<byte> buffer)
        {
            MessageManager.Instance.OnRecvPacket(this, buffer);
        }

        public override void OnSend(int numOfBytes)
        {
        }

        public override void ClearSession()
        {
        }
    }
}
#endif
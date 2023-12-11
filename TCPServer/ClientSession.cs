using Chat;
using Google.Protobuf.WellKnownTypes;
using ServerCoreTCP.MessageWrapper;
using System;
using System.Net;

namespace TCPServer
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

        public override void PreSessionCleanup()
        {
        }
    }
}
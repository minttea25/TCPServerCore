#if FLATBUFFERS

using NetCore;
using System;
using System.Net;

namespace Test.Flatbuffers
{
    public class ClientSession : PacketSession
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"Connected to {endPoint}");

        }

        public override void OnDisconnected(EndPoint endPoint, object error = null)
        {
            Console.WriteLine($"OnDisconnected");

        }

        public override void OnRecv(ArraySegment<byte> buffer, int offset, int count)
        {
            PacketManager.Instance.OnRecvPacket(this, buffer, offset, count);
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine("Sent: " + numOfBytes + " bytes.");
        }
    }
}

#endif
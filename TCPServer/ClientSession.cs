using System;
using System.Net;
using System.Threading;
using ServerCoreTCP.Protobuf;

namespace TCPServer
{
    public class ClientSession : PacketSession
    {
        public readonly uint SessionId; // = UserId
        public string UserName => _userName;
        string _userName;

        public Room Room { get; set; }

        static Random rand = new();

        public ClientSession(uint sessionId)
        {
            SessionId = sessionId;
        }

        public void SetUserName(string userName)
        {
            _userName = userName;
        }

        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine("OnConnected: {0}", endPoint);
        }

        public override void OnDisconnected(EndPoint endPoint, object error = null)
        {
            SessionManager.Instance.Remove(this);

            Console.WriteLine("OnDisconnected: {0}", endPoint);
        }

        public override void OnRecv(ReadOnlySpan<byte> buffer)
        {
            ChatTest.PacketManager.Instance.OnRecvPacket(this, buffer);
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine("Sent: {0} bytes", numOfBytes);
        }
    }
}

using System;
using System.Net;
using System.Threading;
using ServerCoreTCP;

using ServerCoreTCP.Protobuf;
using ChatTest;


namespace TestClient
{
    public class ServerSession : PacketSession
    {
        readonly Random random = new();
        public uint UserId => _userId;
        uint _userId;

        public void SendChat(string msg)
        {
            S_Chat chat = new()
            {
                Msg = msg,
                UserId = UserId
            };
            Send(chat);
        }

        public void LeaveRoom()
        {
            S_LeaveRoom leave = new();
            Send(leave);
        }

        public void EnterCompleted(uint id, uint roomNo)
        {
            _userId = id;
            Console.WriteLine($"Entered the room: {roomNo}");
        }

        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine("OnConnected: {0}", endPoint);

            S_ReqEnterRoom req = new()
            {
                UserName = Program.UserName,
                RoomNo = Program.ReqRoomNo,
            };

            Send(req);
        }

        public override void OnDisconnected(EndPoint endPoint, object error = null)
        {
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

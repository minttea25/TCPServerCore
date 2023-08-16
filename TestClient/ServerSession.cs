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
        public uint UserId => _userId;
        uint _userId;

        public void SendChat(string msg)
        {
            S_Chat chat = new S_Chat()
            {
                Msg = msg,
                UserId = UserId
            };
            Program.Logger.Information("Send Chat: {chat}", chat);
            Send(chat);
        }

        public void LeaveRoom()
        {
            S_LeaveRoom leave = new S_LeaveRoom();
            Program.Logger.Information("Send LeaveRoom: {leave}", leave);
            Send(leave);
        }

        public void EnterCompleted(uint id, uint roomNo)
        {
            _userId = id;
            Console.WriteLine($"Entered the room: {roomNo}");
        }

        public override void OnConnected(EndPoint endPoint)
        {
            Program.Logger.Information("OnConnected: {endPoint}", endPoint);

            S_ReqEnterRoom req = new S_ReqEnterRoom()
            {
                UserName = Program.UserName,
                RoomNo = Program.ReqRoomNo,
            };

            Program.Logger.Information("Send ReqEnterRoom: {req}", req);
            Send(req);
        }

        public override void OnDisconnected(EndPoint endPoint, object error = null)
        {
            Program.Logger.Information("OnDisconnected: {endpoint}", endPoint);
        }

        public override void OnRecv(ReadOnlySpan<byte> buffer)
        {
            ChatTest.PacketManager.Instance.OnRecvPacket(this, buffer);
        }

        public override void OnSend(int numOfBytes)
        {
            Program.Logger.Information("Sent: {numOfBytes} bytes to {ConnectedEndPoint}", numOfBytes, ConnectedEndPoint);
        }
    }
}

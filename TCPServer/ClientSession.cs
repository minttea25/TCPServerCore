using System;
using System.Net;
using ServerCoreTCP.Message;

namespace TCPServer
{
    public class ClientSession : PacketSession
    {
        public readonly uint SessionId; // = UserId
        public string UserName => _userName;
        string _userName;

        public Room Room { get; set; }

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
            Program.Logger.Information("OnConnected: {endPoint}", endPoint);
        }

        public override void OnDisconnected(EndPoint endPoint, object error = null)
        {
            SessionManager.Instance.Remove(this);

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

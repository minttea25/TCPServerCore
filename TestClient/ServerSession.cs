using System;
using System.Net;
using System.Threading;
using ServerCoreTCP;

using ServerCoreTCP.MessageWrapper;
using Google.Protobuf;
using Chat;
using Google.Protobuf.WellKnownTypes;

namespace TestClient
{
    public class ServerSession : PacketSession
    {
        public UserInfo userInfo;
        uint _userId;

        public uint enteredRoomNo;

        public void Send_<T>(T message) where T : IMessage
        {
            //Program.Logger.Information("Send Message: {message}", message);

            Send(message);
        }


        public void EnterCompleted(uint id, uint roomNo)
        {
            _userId = id;
            Console.WriteLine($"Entered the room: {roomNo}");
        }

        public void TrafficTestAuto()
        {
            while (true)
            {
                SChatText chat = new SChatText()
                {
                    RoomId = enteredRoomNo,
                    ChatMsgId = 0,
                    SenderInfo = userInfo,
                    ChatBase = new() { ChatType = ChatType.Text, Timestamp = Timestamp.FromDateTime(DateTime.UtcNow.AddHours(9)) },
                    Msg = $"Hello World! Hello World! Hello World! Hello World! Hello World! Hello World! Hello World! Hello World! Hello World! Hello World! Hello World! Hello World! Hello World! Hello World!  {enteredRoomNo} - {Program.UserName}",
                };

                for (int i = 0; i < 10; ++i)
                {
                    Send_(chat);
                }

                int delay = Program.rand.Next(1000, 3000);
                Thread.Sleep(delay);
            }
        }

        public override void OnConnected(EndPoint endPoint)
        {
            Program.Logger.Information("OnConnected: {endPoint}", endPoint);

            enteredRoomNo = (uint)(Program.rand.Next(1, 6));

            SUserAuthReq req = new()
            {
                UserName = "Test" + Program.rand.Next(1, 10000),
            };
            Send_(req);
        }

        public override void OnDisconnected(EndPoint endPoint, object error = null)
        {
            Program.Logger.Information("OnDisconnected: {endpoint}", endPoint);
        }

        public override void OnRecv(ReadOnlySpan<byte> buffer)
        {
            MessageManager.Instance.OnRecvPacket(this, buffer);
        }

        public override void OnSend(int numOfBytes)
        {
            Program.Logger.Information("Sent: {numOfBytes} bytes to {ConnectedEndPoint}", numOfBytes, ConnectedEndPoint);
        }

        public override void InitSession()
        {
            ;
        }
    }
}
using System;
using System.Net;
using System.Threading;
using ServerCoreTCP;

using ServerCoreTCP.MessageWrapper;
using Google.Protobuf;
using Chat;
using Google.Protobuf.WellKnownTypes;
using ServerCoreTCP.CLogger;
using System.Threading.Tasks;

namespace TestClient
{
    public class ServerSession : PacketSession
    {
        public UserInfo userInfo;
        uint _userId;

        public uint enteredRoomNo;

        public void Send_<T>(T message) where T : IMessage
        {
            CoreLogger.LogSend(message);
            Send(message);
        }


        public void EnterCompleted(uint id, uint roomNo)
        {
            _userId = id;
            Console.WriteLine($"Entered the room: {roomNo}");
        }

        public async void TrafficTestAuto()
        {
            while (true)
            {
                SChatText chat = new SChatText()
                {
                    RoomId = enteredRoomNo,
                    ChatMsgId = 0,
                    SenderInfo = userInfo,
                    ChatBase = new() { ChatType = ChatType.Text, Timestamp = Timestamp.FromDateTime(DateTime.UtcNow.AddHours(9)) },
                    Msg = $"Hello World! Hello World! Hello World! Hello World! Hello World! Hello World! Hello World! Hello World! Hello World! Hello World! Hello World! Hello World!  {enteredRoomNo} - {Program.UserName}",
                };

                for (int i = 0; i < 10; ++i)
                {
                    Send_(chat);
                }

                int delay = Program.rand.Next(1000, 3000);
                await Task.Delay(delay);
            }
        }

        public override void OnConnected(EndPoint endPoint)
        {
            CoreLogger.LogInfo("ServerSession", "OnConnected: {0}", endPoint);

            enteredRoomNo = (uint)(Program.rand.Next(1, 3));

            SUserAuthReq req = new()
            {
                UserName = "Test" + Program.rand.Next(1, 10000),
            };
            Send_(req);
        }

        public override void OnDisconnected(EndPoint endPoint, object error = null)
        {
            CoreLogger.LogInfo("ServerSession", "OnDisconnected: {0}", endPoint);
        }

        public override void OnRecv(ReadOnlySpan<byte> buffer)
        {
            MessageManager.Instance.OnRecvPacket(this, buffer);
        }

        public override void OnSend(int numOfBytes)
        {
        }

        public override void InitSession()
        {
            ;
        }

        public override void PreSessionCleanup()
        {
            ;
        }
    }
}
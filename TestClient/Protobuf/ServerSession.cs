#if PROTOBUF

using Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using ServerCoreTCP;
using Google.Protobuf.WellKnownTypes;
using ServerCoreTCP.Protobuf;

namespace Chat.Protobuf
{
    public class ServerSession : PacketSession
    {
        public override void InitSession()
        {
        }

        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"Connected to {endPoint}");

            Timer timer = new(500);
            timer.Elapsed += (_, _) =>
            {
                for (int i = 0; i < 5; ++i)
                {
                    SSendChatText chatTextPacket = new()
                    {
                        ChatBase = new() { Timestamp = Timestamp.FromDateTime(DateTime.UtcNow) },
                        Msg = $"It is {i} th message! Genshin is sooooo fun!!!",
                        SenderInfo = null,
                    };
                    Send(chatTextPacket);
                }
            };
            timer.Start();
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

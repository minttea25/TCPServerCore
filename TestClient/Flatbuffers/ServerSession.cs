#if FLATBUFFERS

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Google.FlatBuffers;
using ServerCoreTCP;
using ServerCoreTCP.Flatbuffers;

namespace Test.Flatbuffers
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
                    FlatBufferBuilder fb = new(128);
                    var msg = fb.CreateString("This is a message!");
                    var offset = TestPacket.CreateTestPacket(fb, msg, 10000);
                    fb.Finish(offset.Value);
                    var buf = PacketWrapper.Serialize(fb, ushort.MaxValue - 1);
                    Send(buf);
                }
            };
            timer.Start();
        }

        public override void OnDisconnected(EndPoint endPoint, object error = null)
        {
            Console.WriteLine($"OnDisconnected");
        }

        public override void OnSend(int numOfBytes)
        {
        }

        public override void ClearSession()
        {
        }

        public override void OnRecv(ArraySegment<byte> buffer, int offset, int count)
        {
            PacketManager.Instance.OnRecvPacket(this, buffer, offset, count);
        }
    }
}

#endif

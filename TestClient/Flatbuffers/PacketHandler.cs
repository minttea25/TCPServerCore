#if FLATBUFFERS
using Google.FlatBuffers;
using NetCore;
using System;

namespace Test.Flatbuffers
{
    internal class PacketHandler
    {
        public static void TestPacketHandler(ByteBuffer bb, Session session)
        {
            TestPacket pkt = TestPacket.GetRootAsTestPacket(bb);

            Console.WriteLine(pkt.Msg + ", " + pkt.Number);
        }
    }
}
#endif

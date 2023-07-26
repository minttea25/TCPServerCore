using System;

using ServerCoreTCP;

namespace ServerCoreTCP.CustomBuffer
{
    public class PacketHandler
    {
        public static void TestPacketHandler(IPacket packet, Session session)
        {
            TestPacket pkt = packet as TestPacket;

            // TODO
            Console.WriteLine(IPacket.ToString(pkt));
        }

        public static void TestPacket2Handler(IPacket packet, Session session)
        {
            TestPacket2 pkt = packet as TestPacket2;

            // TODO
            Console.WriteLine(IPacket.ToString(pkt));
        }


    }
}

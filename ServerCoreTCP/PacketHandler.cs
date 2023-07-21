using System;

using ServerCoreTCP;

namespace TestNamespace
{
    public class PacketHandler
    {
        public static void TestPacketHandler(IPacket packet, Session session)
        {
            TestPacket pkt = packet as TestPacket;

            // TODO
            Console.WriteLine(session.EndPoint);
            Console.WriteLine(pkt);
        }
        
        public static void TestPacket2Handler(IPacket packet, Session session)
        {
            TestPacket2 pkt = packet as TestPacket2;

            // TODO
            Console.WriteLine(session.EndPoint);
            Console.WriteLine(pkt);
        }
        
        
    }
}

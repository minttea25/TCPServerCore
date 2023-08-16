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
        }
        
        
    }
}

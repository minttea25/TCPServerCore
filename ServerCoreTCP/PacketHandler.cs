using System;

using ServerCoreTCP;

namespace TestNamespace
{
    public class PacketHandler
    {
        public static void TestPacketHandler(IPacket packet, Session session)
        {
            // TODO
            Console.WriteLine(session.EndPoint);
            Console.WriteLine(packet);
        }

        public static void TestPacket2Handler(IPacket packet, Session session)
        {
            // TODO
            Console.WriteLine(session.EndPoint);
            Console.WriteLine(packet);
        }
    }
}

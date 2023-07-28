using System;

using Google.Protobuf;

namespace ServerCoreTCP.ProtobufWrapper
{
    public class PacketHandler
    {
        public static void Test1PacketHandler(IMessage message, Session session)
        {
            Test1 pkt = message as Test1;

            //TODO
            Console.WriteLine(pkt);
        }

        public static void Test2PacketHandler(IMessage message, Session session)
        {
            Test2 pkt = message as Test2;

            //TODO
            Console.WriteLine(pkt);
        }

        public static void Vector3PacketHandler(IMessage message, Session session)
        {
            Vector3 pkt = message as Vector3;

            //TODO
            Console.WriteLine(pkt);
        }
    }
}
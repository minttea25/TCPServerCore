
using System;
using System.Collections.Generic;

namespace ServerCoreTCP.ProtobufWrapper
{
    public class PacketBase
    {
        public readonly static Dictionary<Type, PacketType> PacketMap = new()
        {
            { typeof(Test1), PacketType.Ptest1 },
            { typeof(Test2), PacketType.Ptest2 },
            { typeof(Vector3), PacketType.Pvector3 },
        };
    }

    public enum PacketType
    {
        Pinvalid = 0,
        Ptest1 = 1,
        Ptest2 = 2,
        Pvector3 = 3,
    }
}
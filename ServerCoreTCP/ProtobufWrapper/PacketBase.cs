
using System;
using System.Collections.Generic;

namespace ServerCoreTCP.ProtobufWrapper
{
    public class PacketBase
    {
        public readonly static Dictionary<Type, PacketType> PacketMap = new Dictionary<Type, PacketType>()
        {
            //{ typeof(Vector3), PacketType.Pvector3 },
        };
    }

    public enum PacketType
    {
        Pinvalid = 0,
        Pvector3 = 1,
    }
}
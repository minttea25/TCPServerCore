namespace ServerCoreTCP.Protobuf
{
    /*
     * The first tag of the message must be fixed32 type and its name is 'size'.
     * The second tag of the message must be enumeration type and its name is 'packetType'.
     * 
     * PacketType is regarded as int type, but Google Protobuf serializes it as Varint.
     * => WE SET THE PACKETTYPE VALUE FROM 0 TO 127(2^7) FOR USING ONLY 1 BYTE.
     * 
     * If the count of the PacketType enums seems to run out, I will change the type, enum to fixed32(4 bytes).
     * 
     * --------------------------------------------------------------------------------------------------------
     * 
     * Serialized Data
     * [tag, 1][size, 4][tag, 1][packetType, 1][tag, 1][data]
     * 
     * The size is the full size of the serialized buffer. It must be called right before being serialized and sent.
     * Send() in Session will call CalcSize. You don't need to care of it.
     * However, if you want to send the message as ArraySegment or Memory, you need to call CalcSize manually.
     * 
     * The size_ must be not 0.
     * If the value of a field is 0, Google Protobuf Parser will not include the field on seiralizing.
     * 
    */

    public interface IPacket
    {
        public uint CalcSize();
    }

    public partial class Test1 : IPacket
    {
        public uint CalcSize()
        {
            return size_ = (uint)CalculateSize();
        }

        partial void OnConstruction()
        {
            packetType_ = PacketType.Ptest1;
            size_ = 1 + sizeof(uint) + 1 + 1;
        }
    }

    public partial class Test2 : IPacket
    {
        public uint CalcSize()
        {
            return size_ = (uint)CalculateSize();
        }

        partial void OnConstruction()
        {
            packetType_ = PacketType.Ptest2;
            size_ = 1 + sizeof(uint) + 1 + 1;
        }
    }

    public partial class Vector3 : IPacket
    {
        public uint CalcSize()
        {
            return size_ = (uint)CalculateSize();
        }

        partial void OnConstruction()
        {
            packetType_ = PacketType.Pvector3;
            size_ = 1 + sizeof(uint) + 1 + 1;
        }
    }
}

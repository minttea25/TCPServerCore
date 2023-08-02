namespace ServerCoreTCP.Protobuf
{
    /*
     * The first tag of the message must be PacketType(enum) type
     * 
     * PacketType is regarded as int type, and Google Protobuf serializes it as Varint.
     * 
     * --------------------------------------------------------------------------------------------------------
     * 
     * Serialized Data
     * [size, varint][tag, 1][packetType, varint][data]
     * 
     * The size is the full size of the serialized buffer. It must be called right before being serialized and sent.
     * In this library, when contents send a message through a session, 
     * the session calls WriteDelimitedTo() that calculates the size of the message and encoding that size and the message.
     * (You don't need to calculate the size of the message manually.)
     * Send() in Session will call CalcSize. You don't need to care of it.
     * However, if you want to send the message as ArraySegment or Memory, you need to call CalcSize manually.
     * 
    */

    public partial class Test1
    {
        partial void OnConstruction()
        {
            packetType_ = PacketType.Ptest1;
        }
    }

    public partial class Test2
    {
        partial void OnConstruction()
        {
            packetType_ = PacketType.Ptest2;
        }
    }

    public partial class Vector3
    {
        partial void OnConstruction()
        {
            packetType_ = PacketType.Pvector3;
        }
    }
}

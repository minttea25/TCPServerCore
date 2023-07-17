using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCoreTCP
{
    // Note: The first data is always the whole size of the data. (ushort)

    public interface IPacket
    {
        public PacketType Packet { get; }
        public Memory<byte> MSerialize();
        public ArraySegment<byte> Serialize();
        public void MDeserialize(Memory<byte> data);
        public void Deserialize(ReadOnlySpan<byte> span);
    }

    public interface IItemPacket
    {
        public bool MSerialize(Memory<byte> buffer, ref int offset);
        public bool Serialize(Span<byte> span, ref int offset);
        public void MDeserialize(Memory<byte> buffer, ref int offset);
        public void Deserialize(ReadOnlySpan<byte> span, ref int offset);
    }

    public enum PacketType : ushort
    {
        TClass = 1,
    }
}

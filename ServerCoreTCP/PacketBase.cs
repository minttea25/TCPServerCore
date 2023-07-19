using System;

using ServerCoreTCP;

namespace TestNamespace
{
    public enum Packets : ushort
    {
        TestPacket = 1,
        TestPacket2 = 2,
        
    }

    // Note: The first data is always the whole size of the data. (ushort)
    public interface IPacket
    {
        public ushort PacketType { get; }
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
}

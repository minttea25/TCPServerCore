using System;
using System.Collections.Generic;
using System.Text;

using ServerCoreTCP;

namespace ServerCoreTCP.CustomBuffer
{
    public class TestPacket2 : IPacket
    {
        public ushort PacketType { get; private set; } = (ushort)Packets.TestPacket2; // 1st
        public double itemId;
        public List<int> numbers = new();
        public List<Weapon> weapons = new();
        
        public Memory<byte> MSerialize()
        {
            Memory<byte> buffer = MSendBufferTLS.Reserve(2048);
            bool suc = true;
            int offset = 0;

            offset += sizeof(ushort); // the header size

            suc &= BitConverter.TryWriteBytes(buffer.Span.Slice(offset, sizeof(ushort)), PacketType);
            offset += sizeof(ushort);

            suc &= BitConverter.TryWriteBytes(buffer.Span.Slice(offset, sizeof(double)), itemId);
            offset += sizeof(double);
            
            suc &= BitConverter.TryWriteBytes(buffer.Span.Slice(offset, sizeof(ushort)), (ushort)numbers.Count);
            offset += sizeof(ushort);
            foreach (int _t in numbers)
            {
                suc &= BitConverter.TryWriteBytes(buffer.Span.Slice(offset, sizeof(int)), _t);
                offset += sizeof(int);
                
            }
            
            suc &= BitConverter.TryWriteBytes(buffer.Span.Slice(offset, sizeof(ushort)), (ushort)weapons.Count);
            offset += sizeof(ushort);
            foreach (Weapon _t in weapons)
            {
                suc &= _t.MSerialize(buffer, ref offset);
                
            }
            
            
            suc &= BitConverter.TryWriteBytes(buffer.Span.Slice(0, sizeof(ushort)), (ushort)offset);

            return suc == true ? MSendBufferTLS.Return(offset) : null;
        }
        
        public void MDeserialize(Memory<byte> buffer)
        {
            int offset = 0;

            offset += sizeof(ushort);

            PacketType = BitConverter.ToUInt16(buffer.Span.Slice(offset, sizeof(ushort)));
            offset += sizeof(ushort);

            itemId = BitConverter.ToDouble(buffer.Span.Slice(offset, sizeof(double)));
            offset += sizeof(double);
            
            numbers.Clear();
            ushort numbersCnt = BitConverter.ToUInt16(buffer.Span.Slice(offset, sizeof(ushort)));
            offset += sizeof(ushort);
            for (ushort i = 0; i < numbersCnt; ++i)
            {
                numbers.Add(BitConverter.ToInt32(buffer.Span.Slice(offset, sizeof(int))));
                offset += sizeof(int);
            }
            
            weapons.Clear();
            ushort weaponsCnt = BitConverter.ToUInt16(buffer.Span.Slice(offset, sizeof(ushort)));
            offset += sizeof(ushort);
            for (ushort i = 0; i < weaponsCnt; ++i)
            {
                Weapon _t = new();
                _t.MDeserialize(buffer, ref offset);
                weapons.Add(_t);
            }
            
            
        }

        public ArraySegment<byte> Serialize()
        {
            ArraySegment<byte> buffer = SendBufferTLS.Reserve(2048);
            bool suc = true;
            int offset = 0;

            Span<byte> span = new(buffer.Array, buffer.Offset, buffer.Count);

            offset += sizeof(ushort);

            suc &= BitConverter.TryWriteBytes(span.Slice(offset, sizeof(ushort)), PacketType);
            offset += sizeof(ushort);

            suc &= BitConverter.TryWriteBytes(span.Slice(offset, sizeof(double)), itemId);
            offset += sizeof(double);
            
            suc &= BitConverter.TryWriteBytes(span.Slice(offset, sizeof(ushort)), (ushort)numbers.Count);
            offset += sizeof(ushort);
            foreach (int _t in numbers)
            {
                suc &= BitConverter.TryWriteBytes(span.Slice(offset, sizeof(int)), _t);
                offset += sizeof(int);
                
            }
            
            suc &= BitConverter.TryWriteBytes(span.Slice(offset, sizeof(ushort)), (ushort)weapons.Count);
            offset += sizeof(ushort);
            foreach (Weapon _t in weapons)
            {
                suc &= _t.Serialize(buffer, ref offset);
            }
            
            
            suc &= BitConverter.TryWriteBytes(span.Slice(0, sizeof(ushort)), (ushort)offset);

            return suc == true ? SendBufferTLS.Return(offset) : null;
        }

        public void Deserialize(ReadOnlySpan<byte> span)
        {
            int offset = 0;

            offset += sizeof(ushort);

            PacketType = BitConverter.ToUInt16(span.Slice(offset, sizeof(ushort)));
            offset += sizeof(ushort);

            itemId = BitConverter.ToDouble(span.Slice(offset, sizeof(double)));
            offset += sizeof(double);
            numbers.Clear();
            ushort numbersCnt = BitConverter.ToUInt16(span.Slice(offset, sizeof(ushort)));
            offset += sizeof(ushort);
            for (ushort i = 0; i < numbersCnt; ++i)
            {
                numbers.Add(BitConverter.ToInt32(span.Slice(offset, sizeof(int))));
                offset += sizeof(int);
            }
            
            weapons.Clear();
            ushort weaponsCnt = BitConverter.ToUInt16(span.Slice(offset, sizeof(ushort)));
            offset += sizeof(ushort);
            for (ushort i = 0; i < weaponsCnt; ++i)
            {
                Weapon _t = new();
                _t.Deserialize(span, ref offset);
                weapons.Add(_t);
            }
            
            
        }
    }
}

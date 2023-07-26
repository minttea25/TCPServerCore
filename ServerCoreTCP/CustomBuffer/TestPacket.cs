using System;
using System.Collections.Generic;
using System.Text;

using ServerCoreTCP;

namespace ServerCoreTCP.CustomBuffer
{
    public class TestPacket : IPacket
    {
        public ushort PacketType { get; private set; } = (ushort)Packets.TestPacket; // 1st
        public ushort itemId;
        public List<string> titles = new();
        public Item items;
        
        public Memory<byte> MSerialize()
        {
            Memory<byte> buffer = MSendBufferTLS.Reserve(2048);
            bool suc = true;
            int offset = 0;

            offset += sizeof(ushort); // the header size

            suc &= BitConverter.TryWriteBytes(buffer.Span.Slice(offset, sizeof(ushort)), PacketType);
            offset += sizeof(ushort);

            suc &= BitConverter.TryWriteBytes(buffer.Span.Slice(offset, sizeof(ushort)), itemId);
            offset += sizeof(ushort);
            
            suc &= BitConverter.TryWriteBytes(buffer.Span.Slice(offset, sizeof(ushort)), (ushort)titles.Count);
            offset += sizeof(ushort);
            foreach (string _t in titles)
            {
                ushort _tLen = (ushort)Encoding.Unicode.GetBytes(_t, buffer.Span.Slice(offset + sizeof(ushort)));
                suc &= BitConverter.TryWriteBytes(buffer.Span.Slice(offset, sizeof(ushort)), _tLen);
                offset += sizeof(ushort);
                offset += _tLen;
                
            }
            
            suc &= items.MSerialize(buffer, ref offset);
            
            
            suc &= BitConverter.TryWriteBytes(buffer.Span.Slice(0, sizeof(ushort)), (ushort)offset);

            return suc == true ? MSendBufferTLS.Return(offset) : null;
        }
        
        public void MDeserialize(Memory<byte> buffer)
        {
            int offset = 0;

            offset += sizeof(ushort);

            PacketType = BitConverter.ToUInt16(buffer.Span.Slice(offset, sizeof(ushort)));
            offset += sizeof(ushort);

            itemId = BitConverter.ToUInt16(buffer.Span.Slice(offset, sizeof(ushort)));
            offset += sizeof(ushort);
            
            titles.Clear();
            ushort titlesCnt = BitConverter.ToUInt16(buffer.Span.Slice(offset, sizeof(ushort)));
            offset += sizeof(ushort);
            for (ushort i = 0; i < titlesCnt; ++i)
            {
                int _len = BitConverter.ToUInt16(buffer.Span.Slice(offset, sizeof(ushort)));
                offset += sizeof(ushort);
                titles.Add(Encoding.Unicode.GetString(buffer.Span.Slice(offset, _len)));
                offset += _len;
            }
            
            items = new();
            items.MDeserialize(buffer, ref offset);
            
            
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

            suc &= BitConverter.TryWriteBytes(span.Slice(offset, sizeof(ushort)), itemId);
            offset += sizeof(ushort);
            
            suc &= BitConverter.TryWriteBytes(span.Slice(offset, sizeof(ushort)), (ushort)titles.Count);
            offset += sizeof(ushort);
            foreach (string _t in titles)
            {
                ushort _tLen = (ushort)Encoding.Unicode.GetBytes(_t, span.Slice(offset + sizeof(ushort)));
                suc &= BitConverter.TryWriteBytes(span.Slice(offset, sizeof(ushort)), _tLen);
                offset += sizeof(ushort);
                offset += _tLen;
                
            }
            
            suc &= items.Serialize(buffer, ref offset);
            
            suc &= BitConverter.TryWriteBytes(span.Slice(0, sizeof(ushort)), (ushort)offset);

            return suc == true ? SendBufferTLS.Return(offset) : null;
        }

        public void Deserialize(ReadOnlySpan<byte> span)
        {
            int offset = 0;

            offset += sizeof(ushort);

            PacketType = BitConverter.ToUInt16(span.Slice(offset, sizeof(ushort)));
            offset += sizeof(ushort);

            itemId = BitConverter.ToUInt16(span.Slice(offset, sizeof(ushort)));
            offset += sizeof(ushort);
            titles.Clear();
            ushort titlesCnt = BitConverter.ToUInt16(span.Slice(offset, sizeof(ushort)));
            offset += sizeof(ushort);
            for (ushort i = 0; i < titlesCnt; ++i)
            {
                int _len = BitConverter.ToUInt16(span.Slice(offset, sizeof(ushort)));
                offset += sizeof(ushort);
                titles.Add(Encoding.Unicode.GetString(span.Slice(offset, _len)));
                offset += _len;
            }
            
            items = new();
            items.Deserialize(span, ref offset);
            
            
        }
    }
}

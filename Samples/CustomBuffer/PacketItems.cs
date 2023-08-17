using System;
using System.Collections.Generic;
using System.Text;

using ServerCoreTCP;

namespace ServerCoreTCP.CustomBuffer
{
    public class Item : PacketItem
    {
        public long playerId;
        public string playerName;
#if MEMORY_BUFFER
        public override bool MSerialize(Memory<byte> buffer, ref int offset)
        {
            bool suc = true;
    
            suc &= BitConverter.TryWriteBytes(buffer.Span.Slice(offset, sizeof(long)), playerId);
            offset += sizeof(long);
            
            ushort playerNameLen = (ushort)Encoding.Unicode.GetBytes(playerName, buffer.Span.Slice(offset + sizeof(ushort)));
            suc &= BitConverter.TryWriteBytes(buffer.Span.Slice(offset, sizeof(ushort)), playerNameLen);
            offset += sizeof(ushort);
            offset += playerNameLen;
            
            
            return suc;
        }
    
        public override void MDeserialize(Memory<byte> buffer, ref int offset)
        {
            playerId = BitConverter.ToInt64(buffer.Span.Slice(offset, sizeof(long)));
            offset += sizeof(long);
            
            int playerNameLen = BitConverter.ToUInt16(buffer.Span.Slice(offset, sizeof(ushort)));
            offset += sizeof(ushort);
            playerName = Encoding.Unicode.GetString(buffer.Span.Slice(offset, playerNameLen));
            offset += playerNameLen;
            
            
        }
#else   
        public override bool Serialize(Span<byte> span, ref int offset)
        {
            bool suc = true;
    
            suc &= BitConverter.TryWriteBytes(span.Slice(offset, sizeof(long)), playerId);
            offset += sizeof(long);
            
            ushort playerNameLen = (ushort)Encoding.Unicode.GetBytes(playerName, span.Slice(offset + sizeof(ushort)));
            suc &= BitConverter.TryWriteBytes(span.Slice(offset, sizeof(ushort)), playerNameLen);
            offset += sizeof(ushort);
            offset += playerNameLen;
            
            
            return suc;
        }
    
        public override void Deserialize(ReadOnlySpan<byte> span, ref int offset)
        {
            playerId = BitConverter.ToInt64(span.Slice(offset, sizeof(long)));
            offset += sizeof(long);
            int playerNameLen = BitConverter.ToUInt16(span.Slice(offset, sizeof(ushort)));
            offset += sizeof(ushort);
            playerName = Encoding.Unicode.GetString(span.Slice(offset, playerNameLen));
            offset += playerNameLen;
            
            
        }
#endif
    }
}

#if FLATBUFFERS

using Google.FlatBuffers;
using System;
using System.Runtime.InteropServices;

namespace ServerCoreTCP.Flatbuffers
{
    public class PacketWrapper
    {
        public static ArraySegment<byte> Serialize(FlatBufferBuilder fb, ushort id)
        {
            int dataSize = fb.Offset;
            int dataPosition = fb.DataBuffer.Position;

            byte[] buffer = new byte[Defines.PACKET_HEADER_SIZE + dataSize];
            WritePacketHeader(id, (ushort)(dataSize + Defines.PACKET_HEADER_SIZE), buffer);

            ArraySegment<byte> flatBufferSegment = fb.DataBuffer.ToArraySegment(dataPosition, dataSize);
            Buffer.BlockCopy(flatBufferSegment.Array!, flatBufferSegment.Offset, buffer, Defines.PACKET_HEADER_SIZE, dataSize);

            return new ArraySegment<byte>(buffer);
        }

        public static ArraySegment<byte> Serialize2(FlatBufferBuilder fb, ushort id)
        {
            int dataSize = fb.Offset;
            int dataPosition = fb.DataBuffer.Position;

            var header = new PacketHeader(id, (ushort)(dataSize + Defines.PACKET_HEADER_SIZE));
            byte[] buffer = new byte[Defines.PACKET_HEADER_SIZE + dataSize];

            header.WriteTo(buffer);

            ArraySegment<byte> flatBufferSegment = fb.DataBuffer.ToArraySegment(dataPosition, dataSize);
            Buffer.BlockCopy(flatBufferSegment.Array!, flatBufferSegment.Offset, buffer, Defines.PACKET_HEADER_SIZE, dataSize);

            return new ArraySegment<byte>(buffer);
        }

        public static void WritePacketHeader(ushort id, ushort size, byte[] buffer, int ofs = 0)
        {
#if DEBUG
            if (buffer.Length < 4) throw new Exception("The length of buffer must be at least 4.");
#endif
            buffer[ofs] = (byte)(size);
            buffer[ofs + 1] = (byte)(size >> 8);

            buffer[ofs + 2] = (byte)id;
            buffer[ofs + 3] = (byte)(id >> 8);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal readonly struct PacketHeader
    {
        //public static readonly int SizeOf = Marshal.SizeOf<PacketHeader>();

        public readonly ushort Size { get; }
        public readonly ushort Id { get; }

        public PacketHeader(ushort id, ushort size)
        {
            Id = id;
            Size = size;
        }

        // Little-Endian
        public readonly void WriteTo(byte[] buffer)
        {
            buffer[0] = (byte)Size;
            buffer[1] = (byte)(Size >> 8);

            buffer[2] = (byte)Id;
            buffer[3] = (byte)(Id >> 8);
        }

        public readonly void WrtieToBitConverter(byte[] buffer, int offset = 0)
        {
            BitConverter.GetBytes(Size).CopyTo(buffer, offset);
            BitConverter.GetBytes(Id).CopyTo(buffer, offset + sizeof(ushort));
        }
    }
}
#endif
#if FLATBUFFERS

using Google.FlatBuffers;
using System;
using System.Runtime.InteropServices;

namespace NetCore.Flatbuffers
{
    public class PacketWrapper
    {
        /// <summary>
        /// Wrap the data with id
        /// </summary>
        /// <param name="fb">the finished FlatBufferBuilder</param>
        /// <param name="id">packet id</param>
        /// <returns>The wrapped buffer</returns>
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

        /// <summary>
        /// Write id (2 bytes) and size (2 bytes) to buffer of offset with Little Endian.
        /// </summary>
        /// <param name="id">packet id</param>
        /// <param name="size">size of packet</param>
        /// <param name="buffer">buffer to write</param>
        /// <param name="ofs">offset to write</param>
        public static void WritePacketHeader(ushort id, ushort size, byte[] buffer, int ofs = 0)
        {
            if (buffer.Length - ofs < 4) throw new Exception("Not enough space to write id.");
            buffer[ofs] = (byte)(size);
            buffer[ofs + 1] = (byte)(size >> 8);

            buffer[ofs + 2] = (byte)id;
            buffer[ofs + 3] = (byte)(id >> 8);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal readonly struct PacketHeader
    {

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

        public readonly void WriteToBitConverter(byte[] buffer, int offset = 0)
        {
            BitConverter.GetBytes(Size).CopyTo(buffer, offset);
            BitConverter.GetBytes(Id).CopyTo(buffer, offset + sizeof(ushort));
        }
    }
}
#endif
using System;

namespace NetCore.Utils
{
    /// <summary>
    /// Static class for encoding of variant length with uint value.<br/>(Same as the way if encoding in the Protobuf about variant encoding)
    /// </summary>
    public static class Base128Encoding
    {
        public static int WriteUInt32(uint value, Span<byte> buffer, int offset = 0)
        {
            int index = offset;

            while (value >= 0x80)
            {
                buffer[index++] = (byte)((value & 0x7F) | 0x80);
                value >>= 7;
            }

            buffer[index++] = (byte)value;

            return index;
        }

        public static uint ReadUInt32(ReadOnlySpan<byte> buffer, out int bytesRead, int offset = 0)
        {
            uint result = 0;
            int shift = 0;
            bytesRead = 0;

            for (int i = 0 + offset; i < buffer.Length + offset; i++)
            {
                byte b = buffer[i];
                result |= (uint)(b & 0x7F) << shift;
                shift += 7;
                bytesRead++;

                if ((b & 0x80) == 0) return result;
            }

            throw new ArgumentException("Malformed varint.");
        }
    }
}

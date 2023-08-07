using System;

namespace ServerCoreTCP.Utils
{
    public static class Base128Encoding
    {
        public static int WriteUInt32(uint value, Span<byte> buffer)
        {
            int index = 0;

            while (value >= 0x80)
            {
                buffer[index++] = (byte)((value & 0x7F) | 0x80);
                value >>= 7;
            }

            buffer[index++] = (byte)value;

            return index;
        }

        public static uint ReadUInt32(ReadOnlySpan<byte> buffer, out int bytesRead)
        {
            uint result = 0;
            int shift = 0;
            bytesRead = 0;

            for (int i = 0; i < buffer.Length; i++)
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

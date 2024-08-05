using System;

namespace NetCore.Utils
{
    public static class Serialization
    {
        public static void FromUInt16(ushort value, Span<byte> buffer, int offset = 0)
        {

            if (buffer == null) throw new ArgumentNullException("FromUInt16 - The buffer was NULL.");
            if (buffer.Length - offset < sizeof(ushort)) throw new ArgumentException("FromUInt16 - There is not enough space in the buffer starting from offset.");

            buffer[offset] = (byte)value;
            buffer[offset + 1] = (byte)(value >> 8);
        }

        public static void FromUInt16(ushort value, Span<byte> buffer, ref int offset)
        {

            if (buffer == null) throw new ArgumentNullException("FromUInt16 - The buffer was NULL.");
            if (buffer.Length - offset < sizeof(ushort)) throw new ArgumentException("FromUInt16 - There is not enough space in the buffer starting from offset.");

            buffer[offset++] = (byte)value;
            buffer[offset++] = (byte)(value >> 8);
        }

        public static void FromInt16(short value, Span<byte> buffer, int offset = 0)
        {
            FromUInt16((ushort)value, buffer, offset);
        }

        public static void FromInt16(short value, Span<byte> buffer, ref int offset)
        {
            FromUInt16((ushort)value, buffer, ref offset);
        }

        public static void FromUInt32(uint value, Span<byte> buffer, int offset = 0)
        {
#if DEBUG
            if (buffer == null) throw new ArgumentNullException("FromUInt16 - The parameter, buffer was NULL.");
            if (buffer.Length - offset < sizeof(ushort)) throw new ArgumentException("FromUInt16 - There is not enough space in the buffer starting from offset.");
#endif
            buffer[offset] = (byte)value;
            buffer[offset + 1] = (byte)(value >> 8);
            buffer[offset + 2] = (byte)(value >> 16);
            buffer[offset + 3] = (byte)(value >> 24);
        }

        public static void FromUInt32(uint value, Span<byte> buffer, ref int offset)
        {

            if (buffer == null) throw new ArgumentNullException("FromUInt16 - The parameter, buffer was NULL.");
            if (buffer.Length - offset < sizeof(ushort)) throw new ArgumentException("FromUInt16 - There is not enough space in the buffer starting from offset.");

            buffer[offset++] = (byte)value;
            buffer[offset++] = (byte)(value >> 8);
            buffer[offset++] = (byte)(value >> 16);
            buffer[offset++] = (byte)(value >> 24);
        }

        public static void FromInt32(int value, Span<byte> buffer, int offset = 0)
        {
            FromUInt32((uint)value, buffer, offset);
        }

        public static void FromInt32(int value, Span<byte> buffer, ref int offset)
        {
            FromUInt32((uint)value, buffer, ref offset);
        }
    }

    public static class Deserialization
    {
        public static ushort ToUInt16(ReadOnlySpan<byte> buffer, int offset = 0)
        {

            if (buffer == null) throw new ArgumentNullException("ToUInt16 - The buffer was NULL.");
            if (buffer.Length - offset < sizeof(ushort)) throw new ArgumentException("ToUInt16 - There are not enough bytes in the buffer starting from offset for deserializing.");

            return (ushort)(buffer[offset] | (buffer[offset+1] << 8));
        }

        public static ushort ToUInt16(ReadOnlySpan<byte> buffer, ref int offset)
        {

            if (buffer == null) throw new ArgumentNullException("ToUInt16 - The buffer was NULL.");
            if (buffer.Length - offset < sizeof(ushort)) throw new ArgumentException("ToUInt16 - There are not enough bytes in the buffer starting from offset for deserializing.");

            return (ushort)(buffer[offset++] | (buffer[offset++] << 8));
        }

        public static short ToInt16(ReadOnlySpan<byte> buffer, int offset = 0)
        {
            return (short)ToUInt16(buffer, offset);
        }

        public static short ToInt16(ReadOnlySpan<byte> buffer, ref int offset)
        {
            return (short)ToUInt16(buffer, ref offset);
        }

        public static uint ToUInt32(ReadOnlySpan<byte> buffer, int offset = 0)
        {

            if (buffer == null) throw new ArgumentNullException("ToUInt32 - The buffer was NULL.");
            if (buffer.Length - offset < sizeof(ushort)) throw new ArgumentException("ToUInt32 - There are not enough bytes in the buffer starting from offset for deserializing.");

            return (uint)(buffer[offset] | (buffer[offset + 1] << 8) | (buffer[offset + 2] << 16) | (buffer[offset + 3] << 24));
        }

        public static uint ToUInt32(ReadOnlySpan<byte> buffer, ref int offset)
        {

            if (buffer == null) throw new ArgumentNullException("ToUInt32 - The buffer was NULL.");
            if (buffer.Length - offset < sizeof(ushort)) throw new ArgumentException("ToUInt32 - There are not enough bytes in the buffer starting from offset for deserializing.");

            return (uint)(buffer[offset++] | (buffer[offset++] << 8) | (buffer[offset++] << 16) | (buffer[offset++] << 24));
        }

        public static int ToInt32(ReadOnlySpan<byte> buffer, int offset = 0)
        {
            return (int)ToUInt32(buffer, offset);
        }

        public static int ToInt32(ReadOnlySpan<byte> buffer, ref int offset)
        {
            return (int)ToUInt32(buffer, ref offset);
        }
    }

    
}

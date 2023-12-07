using System;
using System.Net.Sockets;

namespace ServerCoreTCP.Utils
{
    public static class Extensions
    {
        #region Socket Util Extends
        public static void SetLinger(this Socket socket, int lingerSeconds)
        {
            SocketUtils.SetLinger(socket, lingerSeconds);
        }

        public static void SetNoDelay(this Socket socket, bool noDelay)
        {
            SocketUtils.SetNoDelay(socket, noDelay);
        }

        public static void SetKeepAlive(this Socket socket, bool keepAlive)
        {
            SocketUtils.SetKeepAlive(socket, keepAlive);
        }

        public static void SetReuseAddress(this Socket socket, bool reuseAddress)
        {
            SocketUtils.SetReuseAddress(socket, reuseAddress);
        }
        #endregion

        #region Serialization
        public static void FromUInt16(this ushort value, Span<byte> buffer, int offset = 0)
        {
            Serialization.FromUInt16(value, buffer, offset);
        }

        public static void FromUInt16(this ushort value, Span<byte> buffer, ref int offset)
        {
            Serialization.FromUInt16(value, buffer, ref offset);
        }

        public static void FromInt16(this short value, Span<byte> buffer, int offset = 0)
        {
            Serialization.FromInt16(value, buffer, offset);
        }

        public static void FromInt16(this short value, Span<byte> buffer, ref int offset)
        {
            Serialization.FromInt16(value, buffer, ref offset);
        }

        public static void FromUInt32(this uint value, Span<byte> buffer, int offset = 0)
        {
            Serialization.FromUInt32(value, buffer, offset);
        }

        public static void FromUInt32(this uint value, Span<byte> buffer, ref int offset)
        {
            Serialization.FromUInt32(value, buffer, ref offset);
        }

        public static void FromInt32(this int value, Span<byte> buffer, int offset = 0)
        {
            Serialization.FromInt32(value, buffer, offset);
        }

        public static void FromInt32(this int value, Span<byte> buffer, ref int offset)
        {
            Serialization.FromInt32(value, buffer, ref offset);
        }
        #endregion

        #region Deserialization
        public static ushort ToUInt16(this ReadOnlySpan<byte> buffer, int offset = 0)
        {
            return Deserialization.ToUInt16(buffer, offset);
        }

        public static ushort ToUInt16(this ReadOnlySpan<byte> buffer, ref int offset)
        {
            return Deserialization.ToUInt16(buffer, ref offset);
        }

        public static short ToInt16(this ReadOnlySpan<byte> buffer, int offset = 0)
        {
            return Deserialization.ToInt16(buffer, offset);
        }

        public static short ToInt16(this ReadOnlySpan<byte> buffer, ref int offset)
        {
            return Deserialization.ToInt16(buffer, ref offset);
        }

        public static uint ToUInt32(this ReadOnlySpan<byte> buffer, int offset = 0)
        {
            return Deserialization.ToUInt32(buffer, offset);
        }

        public static uint ToUInt32(this ReadOnlySpan<byte> buffer, ref int offset)
        {
            return Deserialization.ToUInt32(buffer, ref offset);
        }

        public static int ToInt32(this ReadOnlySpan<byte> buffer, int offset = 0)
        {
            return Deserialization.ToInt32(buffer, offset);
        }

        public static int ToInt32(this ReadOnlySpan<byte> buffer, ref int offset)
        {
            return Deserialization.ToInt32(buffer, ref offset);
        }
        #endregion
    }
}

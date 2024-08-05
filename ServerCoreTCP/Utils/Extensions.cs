using System;
using System.Net.Sockets;

namespace NetCore.Utils
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

        public static void SetReuseAddress(this Socket socket, bool reuseAddress)
        {
            SocketUtils.SetReuseAddress(socket, reuseAddress);
        }
        #endregion

        #region Serialization
        /// <summary>
        /// Converts the value to bytes at the offset starts in the buffer.<br/> 
        /// The buffer must have enough space.
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <param name="buffer">The buffer</param>
        /// <param name="offset">The offset of the buffer</param>
        public static void FromUInt16(this ushort value, Span<byte> buffer, int offset = 0)
        {
            Serialization.FromUInt16(value, buffer, offset);
        }

        /// <summary>
        /// Converts the value to bytes at the offset starts in the buffer.<br/>
        /// The buffer must have enough space.
        /// Note: The offset is reference value.<br/>
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <param name="buffer">The buffer</param>
        /// <param name="offset">The offset of the buffer</param>
        public static void FromUInt16(this ushort value, Span<byte> buffer, ref int offset)
        {
            Serialization.FromUInt16(value, buffer, ref offset);
        }

        /// <summary>
        /// Converts the value to bytes at the offset starts in the buffer.<br/> 
        /// The buffer must have enough space.
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <param name="buffer">The buffer</param>
        /// <param name="offset">The offset of the buffer</param>
        public static void FromInt16(this short value, Span<byte> buffer, int offset = 0)
        {
            Serialization.FromInt16(value, buffer, offset);
        }

        /// <summary>
        /// Converts the value to bytes at the offset starts in the buffer.<br/>
        /// The buffer must have enough space.
        /// Note: The offset is reference value.<br/>
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <param name="buffer">The buffer</param>
        /// <param name="offset">The offset of the buffer</param>
        public static void FromInt16(this short value, Span<byte> buffer, ref int offset)
        {
            Serialization.FromInt16(value, buffer, ref offset);
        }

        /// <summary>
        /// Converts the value to bytes at the offset starts in the buffer.<br/> 
        /// The buffer must have enough space.
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <param name="buffer">The buffer</param>
        /// <param name="offset">The offset of the buffer</param>
        public static void FromUInt32(this uint value, Span<byte> buffer, int offset = 0)
        {
            Serialization.FromUInt32(value, buffer, offset);
        }

        /// <summary>
        /// Converts the value to bytes at the offset starts in the buffer.<br/>
        /// The buffer must have enough space.
        /// Note: The offset is reference value.<br/>
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <param name="buffer">The buffer</param>
        /// <param name="offset">The offset of the buffer</param>
        public static void FromUInt32(this uint value, Span<byte> buffer, ref int offset)
        {
            Serialization.FromUInt32(value, buffer, ref offset);
        }

        /// <summary>
        /// Converts the value to bytes at the offset starts in the buffer.<br/> 
        /// The buffer must have enough space.
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <param name="buffer">The buffer</param>
        /// <param name="offset">The offset of the buffer</param>
        public static void FromInt32(this int value, Span<byte> buffer, int offset = 0)
        {
            Serialization.FromInt32(value, buffer, offset);
        }

        /// <summary>
        /// Converts the value to bytes at the offset starts in the buffer.<br/>
        /// The buffer must have enough space.
        /// Note: The offset is reference value.<br/>
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <param name="buffer">The buffer</param>
        /// <param name="offset">The offset of the buffer</param>
        public static void FromInt32(this int value, Span<byte> buffer, ref int offset)
        {
            Serialization.FromInt32(value, buffer, ref offset);
        }
        #endregion

        #region Deserialization
        /// <summary>
        /// Converts the buffer starts at the offset to value. <br/>
        /// The bytes starts at the offet of the buffer must have correct data to convert to the type of value.
        /// </summary>
        /// <param name="buffer">The buffer to convert</param>
        /// <param name="offset">The offset of the buffer</param>
        /// <returns>The converted value</returns>
        public static ushort ToUInt16(this ReadOnlySpan<byte> buffer, int offset = 0)
        {
            return Deserialization.ToUInt16(buffer, offset);
        }

        /// <summary>
        /// Converts the buffer starts at the offset to value. <br/>
        /// The bytes starts at the offet of the buffer must have correct data to convert to the type of value.<br/>
        /// Note: The offset is reference value.
        /// </summary>
        /// <param name="buffer">The buffer to convert</param>
        /// <param name="offset">The offset of the buffer</param>
        /// <returns>The converted value</returns>
        public static ushort ToUInt16(this ReadOnlySpan<byte> buffer, ref int offset)
        {
            return Deserialization.ToUInt16(buffer, ref offset);
        }

        /// <summary>
        /// Converts the buffer starts at the offset to value. <br/>
        /// The bytes starts at the offet of the buffer must have correct data to convert to the type of value.
        /// </summary>
        /// <param name="buffer">The buffer to convert</param>
        /// <param name="offset">The offset of the buffer</param>
        /// <returns>The converted value</returns>
        public static short ToInt16(this ReadOnlySpan<byte> buffer, int offset = 0)
        {
            return Deserialization.ToInt16(buffer, offset);
        }

        /// <summary>
        /// Converts the buffer starts at the offset to value. <br/>
        /// The bytes starts at the offet of the buffer must have correct data to convert to the type of value.<br/>
        /// Note: The offset is reference value.
        /// </summary>
        /// <param name="buffer">The buffer to convert</param>
        /// <param name="offset">The offset of the buffer</param>
        /// <returns>The converted value</returns>
        public static short ToInt16(this ReadOnlySpan<byte> buffer, ref int offset)
        {
            return Deserialization.ToInt16(buffer, ref offset);
        }

        /// <summary>
        /// Converts the buffer starts at the offset to value. <br/>
        /// The bytes starts at the offet of the buffer must have correct data to convert to the type of value.
        /// </summary>
        /// <param name="buffer">The buffer to convert</param>
        /// <param name="offset">The offset of the buffer</param>
        /// <returns>The converted value</returns>
        public static uint ToUInt32(this ReadOnlySpan<byte> buffer, int offset = 0)
        {
            return Deserialization.ToUInt32(buffer, offset);
        }

        /// <summary>
        /// Converts the buffer starts at the offset to value. <br/>
        /// The bytes starts at the offet of the buffer must have correct data to convert to the type of value.<br/>
        /// Note: The offset is reference value.
        /// </summary>
        /// <param name="buffer">The buffer to convert</param>
        /// <param name="offset">The offset of the buffer</param>
        /// <returns>The converted value</returns>
        public static uint ToUInt32(this ReadOnlySpan<byte> buffer, ref int offset)
        {
            return Deserialization.ToUInt32(buffer, ref offset);
        }

        /// <summary>
        /// Converts the buffer starts at the offset to value. <br/>
        /// The bytes starts at the offet of the buffer must have correct data to convert to the type of value.
        /// </summary>
        /// <param name="buffer">The buffer to convert</param>
        /// <param name="offset">The offset of the buffer</param>
        /// <returns>The converted value</returns>
        public static int ToInt32(this ReadOnlySpan<byte> buffer, int offset = 0)
        {
            return Deserialization.ToInt32(buffer, offset);
        }

        /// <summary>
        /// Converts the buffer starts at the offset to value. <br/>
        /// The bytes starts at the offet of the buffer must have correct data to convert to the type of value.<br/>
        /// Note: The offset is reference value.
        /// </summary>
        /// <param name="buffer">The buffer to convert</param>
        /// <param name="offset">The offset of the buffer</param>
        /// <returns>The converted value</returns>
        public static int ToInt32(this ReadOnlySpan<byte> buffer, ref int offset)
        {
            return Deserialization.ToInt32(buffer, ref offset);
        }
        #endregion
    }
}

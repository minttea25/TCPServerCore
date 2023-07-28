using System;
using System.Collections.Generic;

using Google.Protobuf;

namespace ServerCoreTCP.ProtobufWrapper
{
    public static class PacketWrapper
    {
        /// <summary>
        /// Serialize the message with PacketWrapper [Memory]
        /// </summary>
        /// <typeparam name="T">Google.Protobuf.IMessage</typeparam>
        /// <param name="message">The message to serialize.</param>
        /// <returns>The serialized buffer with PacketWrapper.</returns>
        public static Memory<byte> MSerialize<T>(T message) where T : IMessage
        {
            ushort messageType = (ushort)PacketBase.PacketMap[typeof(T)];
            ushort size = (ushort)(message.CalculateSize() + sizeof(ushort) + sizeof(ushort));

            int offset = 0;
            Memory<byte> buffer = MSendBufferTLS.Reserve(size);
            if (BitConverter.TryWriteBytes(buffer.Span.Slice(offset, sizeof(ushort)), size) == false)
            {
                return null;
            }
            offset += sizeof(ushort);
            if (BitConverter.TryWriteBytes(buffer.Span.Slice(offset, sizeof(ushort)), messageType) == false)
            {
                return null;
            }
            offset += sizeof(ushort);
            message.WriteTo(buffer.Span.Slice(offset));

            return MSendBufferTLS.Return(size);
        }

        /// <summary>
        /// Serialize the message with PacketWrapper [ArraySegment]
        /// </summary>
        /// <typeparam name="T">Google.Protobuf.IMessage</typeparam>
        /// <param name="message">The message to serialize.</param>
        /// <returns>The serialized buffer with PacketWrapper.</returns>
        public static ArraySegment<byte> Serialize<T>(T message) where T : IMessage
        {
            ushort messageType = (ushort)PacketBase.PacketMap[typeof(T)];
            ushort size = (ushort)(message.CalculateSize() + sizeof(ushort) + sizeof(ushort));

            int offset = 0;
            ArraySegment<byte> buffer = SendBufferTLS.Reserve(size);
            if (BitConverter.TryWriteBytes(buffer.Slice(offset, sizeof(ushort)), size) == false)
            {
                return null;
            }
            offset += sizeof(ushort);
            if (BitConverter.TryWriteBytes(buffer.Slice(offset, sizeof(ushort)), messageType) == false)
            {
                return null;
            }
            offset += sizeof(ushort);
            message.WriteTo(buffer.Slice(offset));

            return SendBufferTLS.Return(size);
        }

        /// <summary>
        /// Extended function of PacketWrapper.MSerialize().
        /// </summary>
        /// <typeparam name="T">Google.Protobuf.IMessage</typeparam>
        /// <param name="message">The message to serialize.</param>
        /// <returns>The serialized buffer with PacketWrapper.</returns>
        public static Memory<byte> MSerializeWrapper<T>(this T message) where T : IMessage
        {
            return MSerialize(message);
        }

        /// <summary>
        /// Extended function of PacketWrapper.Serialize().
        /// </summary>
        /// <typeparam name="T">Google.Protobuf.IMessage</typeparam>
        /// <param name="message">The message to serialize.</param>
        /// <returns>The serialized buffer with PacketWrapper.</returns>
        public static ArraySegment<byte> SerializeWrapper<T>(this T message) where T : IMessage
        {
            return Serialize(message);
        }
    }
}

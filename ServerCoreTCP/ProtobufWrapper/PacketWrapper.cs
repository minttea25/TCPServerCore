﻿using System;
using System.Collections.Generic;

using Google.Protobuf;

namespace ServerCoreTCP.ProtobufWrapper
{
    public static class PacketWrapper
    {
        public const int HeaderMessageLengthSize = sizeof(ushort);
        public const int HeaderPacketTypeSize = sizeof(ushort);

        /// <summary>
        /// Serialize the message with PacketWrapper [Memory]
        /// </summary>
        /// <typeparam name="T">Google.Protobuf.IMessage</typeparam>
        /// <param name="message">The message to serialize.</param>
        /// <returns>The serialized buffer with PacketWrapper.</returns>
        public static Memory<byte> MSerialize<T>(T message) where T : IMessage
        {
            ushort size = (ushort)(message.CalculateSize() + HeaderPacketTypeSize);
            ushort messageType = (ushort)PacketBase.PacketMap[typeof(T)];

            int offset = 0;
            Memory<byte> buffer = MSendBufferTLS.Reserve(HeaderMessageLengthSize + size);
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

            return MSendBufferTLS.Return(HeaderMessageLengthSize + size);
        }

        /// <summary>
        /// Serialize the message with PacketWrapper [ArraySegment]
        /// </summary>
        /// <typeparam name="T">Google.Protobuf.IMessage</typeparam>
        /// <param name="message">The message to serialize.</param>
        /// <returns>The serialized buffer with PacketWrapper.</returns>
        public static ArraySegment<byte> Serialize<T>(T message) where T : IMessage
        {
            ushort size = (ushort)(message.CalculateSize() + HeaderPacketTypeSize);
            ushort messageType = (ushort)PacketBase.PacketMap[typeof(T)];

            int offset = 0;
            ArraySegment<byte> buffer = SendBufferTLS.Reserve(HeaderMessageLengthSize + size);
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

            return SendBufferTLS.Return(HeaderMessageLengthSize + size);
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
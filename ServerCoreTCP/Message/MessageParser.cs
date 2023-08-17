#if MESSAGE_PACKET

using ServerCoreTCP.Utils;
using System;
using Google.Protobuf;

namespace ServerCoreTCP.Message
{
    public static class MessageParser
    {
        const int SendDefaultReserveSize = 1024;

        /// <summary>
        /// Serialize the message: [size][message] [Memory]
        /// </summary>
        /// <typeparam name="T">Google.Protobuf.IMessage</typeparam>
        /// <param name="message">The message to serialize.</param>
        /// <returns>The serialized buffer of Memory.</returns>
        public static Memory<byte> MSerialize<T>(T message) where T : IMessage
        {
            int size = message.CalculateSize();
            Memory<byte> buffer = MSendBufferTLS.Reserve(SendDefaultReserveSize);
            int writtenBytes = Base128Encoding.WriteUInt32((uint)size, buffer.Span);
            message.WriteTo(buffer.Span.Slice(writtenBytes, size));
            return MSendBufferTLS.Return(size + writtenBytes);
        }

        /// <summary>
        /// Serialize the message: [size][message] [ArraySegment]
        /// </summary>
        /// <typeparam name="T">Google.Protobuf.IMessage</typeparam>
        /// <param name="message">The message to serialize.</param>
        /// <returns>The serialized buffer of ArraySegment.</returns>
        public static ArraySegment<byte> Serialize<T>(T message) where T : IMessage
        {
            int size = message.CalculateSize();
            ArraySegment<byte> buffer = SendBufferTLS.Reserve(SendDefaultReserveSize);
            int writtenBytes = Base128Encoding.WriteUInt32((uint)size, buffer);
            message.WriteTo(buffer.Slice(writtenBytes, size));
            return SendBufferTLS.Return(size + writtenBytes);
        }

        /// <summary>
        /// Extended function of Protobuf.MSerialize().
        /// </summary>
        /// <typeparam name="T">Google.Protobuf.IMessage</typeparam>
        /// <param name="message">The message to serialize.</param>
        /// <returns>The serialized buffer of Memory.</returns>
        public static Memory<byte> MSerializeProtobuf<T>(this T message) where T : IMessage
        {
            return MSerialize(message);
        }

        /// <summary>
        /// Extended function of Protobuf.Serialize().
        /// </summary>
        /// <typeparam name="T">Google.Protobuf.IMessage</typeparam>
        /// <param name="message">The message to serialize.</param>
        /// <returns>The serialized buffer of ArraySegment.</returns>
        public static ArraySegment<byte> SerializeProtobuf<T>(this T message) where T : IMessage
        {
            return Serialize(message);
        }
    }
}
#endif
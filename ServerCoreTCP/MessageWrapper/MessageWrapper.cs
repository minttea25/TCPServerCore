using System;
using System.Collections.Generic;

using Google.Protobuf;
using ServerCoreTCP.CLogger;
using ServerCoreTCP.Utils;

namespace ServerCoreTCP.MessageWrapper
{
    public static class MessageWrapper
    {
        public readonly static Dictionary<Type, ushort> PacketMap = new Dictionary<Type, ushort>()
        {
            // example
            //{ typeof(Vector3), (ushort)PacketType.Pvector3 },
        };

        public const int HeaderSize = sizeof(ushort);
        public const int MessageTypeSize = sizeof(ushort);

        /// <summary>
        /// Serialize the message with PacketWrapper using little-endian [ArraySegment]
        /// </summary>
        /// <typeparam name="T">Google.Protobuf.IMessage</typeparam>
        /// <param name="message">The message to serialize.</param>
        /// <returns>The serialized buffer with PacketWrapper.</returns>
        public static ArraySegment<byte> Serialize<T>(T message) where T : IMessage
        {
            try
            {
                // [packetSize (except this header)(2)][messageType(2)][message]
                ushort messageType = PacketMap[typeof(T)];
                ushort packetSize = (ushort)((ushort)(message.CalculateSize()) + MessageTypeSize);
                int offset = 0;

                ArraySegment<byte> buffer = SendBufferTLS.Use(HeaderSize + packetSize);

                // contains offset += sizeof(ushort)
                packetSize.FromUInt16(buffer, ref offset);  // 2 bytes
                // contains offset += sizeof(ushort)
                messageType.FromUInt16(buffer, ref offset); // 2 bytes
                message.WriteTo(buffer.Slice(offset));

                // needless code
                // offset += packetSize - sizeof(ushort);

                return buffer;
            }
            catch (KeyNotFoundException knfe)
            {
                CoreLogger.LogError("MessageWrapper.Serialize", knfe, "Can not find key={0} in PacketMap", typeof(T));
                return null;
            }
            catch (Exception e)
            {
                CoreLogger.LogError("MessageWrapper.Serialize", e, "Other Exception");
                return null;
            }
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
#if MESSAGE_WRAPPER_PACKET
using System;
using System.Collections.Generic;

using Google.Protobuf;
using ServerCoreTCP.CLogger;

namespace ServerCoreTCP.MessageWrapper
{
    public static class MessageWrapper
    {
        public readonly static Dictionary<Type, ushort> PacketMap = new Dictionary<Type, ushort>()
        {
            // example
            //{ typeof(Vector3), (ushort)PacketType.Pvector3 },
        };

        public const int HeaderMessageLengthSize = sizeof(ushort);
        public const int HeaderPacketTypeSize = sizeof(ushort);

        /// <summary>
        /// Serialize the message with PacketWrapper [ArraySegment]
        /// </summary>
        /// <typeparam name="T">Google.Protobuf.IMessage</typeparam>
        /// <param name="message">The message to serialize.</param>
        /// <returns>The serialized buffer with PacketWrapper.</returns>
        public static ArraySegment<byte> Serialize<T>(T message) where T : IMessage
        {
            ushort size = (ushort)(message.CalculateSize() + HeaderPacketTypeSize);
            try
            {
                ushort messageType = PacketMap[typeof(T)];
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
            catch(KeyNotFoundException knfe)
            {
                CoreLogger.LogError("MessageWrapper.Serialize", knfe, "Can not find key={0} in PacketMap", typeof(T));
                return null;
            }
            catch(Exception e)
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
#endif
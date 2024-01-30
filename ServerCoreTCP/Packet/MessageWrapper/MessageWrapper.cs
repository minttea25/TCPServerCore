using System;
using System.Collections.Generic;

using Google.Protobuf;
using ServerCoreTCP.CLogger;
using ServerCoreTCP.Utils;

namespace ServerCoreTCP.MessageWrapper
{
    public static class MessageWrapper
    {
#if PACKET_TYPE_INT
        public readonly static Dictionary<Type, uint> PacketMap = new Dictionary<Type, uint>()
        {
            // example
            //{ typeof(Vector3), (uint)PacketType.Pvector3 },
        };
#else
        public readonly static Dictionary<Type, ushort> PacketMap = new Dictionary<Type, ushort>()
        {
            // example
            //{ typeof(Vector3), (ushort)PacketType.Pvector3 },
        };
#endif

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

#if PACKET_TYPE_INT
                // [packetSize (except this header)(2)][messageType(4)][message]
                uint messageType = PacketMap[typeof(T)];
#else
                // [packetSize (except this header)(2)][messageType(2)][message]
                ushort messageType = PacketMap[typeof(T)];
#endif
                ushort packetSize = (ushort)(Defines.PACKET_DATATYPE_SIZE + message.CalculateSize());
                int offset = 0;

                ArraySegment<byte> buffer = SendBufferTLS.Use(Defines.PACKET_HEADER_SIZE + packetSize);

                // contains offset += sizeof(ushort)
                packetSize.FromUInt16(buffer, ref offset);  // 2 bytes
#if PACKET_TYPE_INT
                // contains offset += sizeof(uint)
                messageType.FromUInt32(buffer, ref offset); // 4 bytes
#else
                // contains offset += sizeof(ushort)
                messageType.FromUInt16(buffer, ref offset); // 2 bytes
#endif
                message.WriteTo(buffer.Slice(offset));

                // needless code
                // offset += packetSize - sizeof(ushort) or sizeof(uint);

                return buffer;
            }
            catch (KeyNotFoundException knfe)
            {
                CoreLogger.LogError("MessageWrapper.Serialize", knfe, "Can not find key={0} in PacketMap. The MessageManager may not be initialized or the parameter message is not derived from Google.Protobuf.IMessage, and can be IMessage itself.", typeof(T));
                throw new Exception($"Can not find key={typeof(T)} in PacketMap: {knfe}");
                //return null;
            }
            catch (Exception e)
            {
                CoreLogger.LogError("MessageWrapper.Serialize", e, "Other Exception");
                throw new Exception($"Other Exception: {e}");
                //return null;
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
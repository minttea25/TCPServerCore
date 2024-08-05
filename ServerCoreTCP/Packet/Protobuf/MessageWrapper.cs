#if PROTOBUF

using System;
using System.Collections.Generic;

using Google.Protobuf;
using NetCore.CLogger;
using NetCore.Utils;

namespace NetCore.Protobuf
{
    /// <summary>
    /// Serializes the message(Google.Protobuf.IMessage) with the specified packet type.
    /// </summary>
    public static class MessageWrapper
    {
        /// <summary>
        /// The serializer will find the packet id of the message.
        /// <br/>Add data at the MessageManager.Init().
        /// <br/>Note: It should contain the information of the ids about the message types. 
        /// </summary>
        public readonly static Dictionary<Type, ushort> PacketMap = new Dictionary<Type, ushort>()
        {
            // example
            //{ typeof(Vector3), (ushort)PacketType.Pvector3 },
        };

        /// <summary>
        /// Serialize the message with PacketWrapper using little-endian [ArraySegment]
        /// </summary>
        /// <typeparam name="T">Google.Protobuf.IMessage</typeparam>
        /// <param name="message">The message to serialize.</param>
        /// <returns>The serialized buffer with PacketWrapper.<br/>Null if failed.</returns>
        public static ArraySegment<byte> Serialize<T>(T message) where T : IMessage
        {
            try
            {
                // [packetSize (except this header)(2)][messageType(2)][message]
                ushort messageType = PacketMap[typeof(T)];

                ushort packetSize = (ushort)(Defines.PACKET_ID_SIZE + message.CalculateSize());
                int offset = 0;

                ArraySegment<byte> buffer = SendBufferTLS.Use(Defines.PACKET_HEADER_SIZE + packetSize);

                // contains offset += sizeof(ushort)
                packetSize.FromUInt16(buffer, ref offset);  // 2 bytes

                // contains offset += sizeof(ushort)
                messageType.FromUInt16(buffer, ref offset); // 2 bytes

                message.WriteTo(buffer.Slice(offset));

                // meaningless code
                // offset += packetSize - sizeof(ushort) or sizeof(uint);

                return buffer;
            }
            catch (KeyNotFoundException knfe)
            {
                CoreLogger.LogError("MessageWrapper.Serialize", knfe, "Can not find key={0} in PacketMap. Init MessageManager first, or check the init method.", typeof(T));
                //throw new Exception($"Can not find key={typeof(T)} in PacketMap. Init MessageManager first, or check the init method.");
                return null;
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
        /// <returns>The serialized buffer with PacketWrapper.<br/>Null if failed.</returns>
        public static ArraySegment<byte> SerializeWrapper<T>(this T message) where T : IMessage
        {
            return Serialize(message);
        }
    }
}
#endif
#if MESSAGE_WRAPPER_PACKET

using System;
using Google.Protobuf;

namespace ServerCoreTCP.MessageWrapper
{
    public abstract class PacketSession : Session
    {
        const int MinimumPacketLength = MessageWrapper.HeaderMessageLengthSize + MessageWrapper.HeaderPacketTypeSize;

        /// <summary>
        /// Send message to endpoint of the socket [Protobuf Wrapper]
        /// </summary>
        /// <typeparam name="T">Google.Protobuf.IMessage</typeparam>
        /// <param name="packet">The message to send.</param>
        public void Send<T>(T message) where T : IMessage
        {
            SendRaw(message.SerializeWrapper());
        }

        protected sealed override int OnRecvProcess(ArraySegment<byte> buffer)
        {
            if (buffer.Count < MinimumPacketLength) return 0;

            int processed = 0;

            while (processed < buffer.Count)
            {
                // size contains the length of the packet type and message.
                ushort size = BitConverter.ToUInt16(buffer.Slice(processed, MessageWrapper.HeaderMessageLengthSize));
                processed += MessageWrapper.HeaderMessageLengthSize;

                if (size + processed  > buffer.Count) break;

                // The data should be [packet type, 2][message].
                ReadOnlySpan<byte> data = buffer.Slice(processed, size);
                processed += size;

                OnRecv(data);
            }

            return processed;
        }
    }
}
#endif
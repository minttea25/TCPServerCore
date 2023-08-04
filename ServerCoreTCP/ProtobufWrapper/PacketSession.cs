#define MEMORY_BUFFER

using System;
using Google.Protobuf;

namespace ServerCoreTCP.ProtobufWrapper
{
    public abstract class PacketSession : Session
    {
        const int MinimumPacketLength = PacketWrapper.HeaderMessageLengthSize + PacketWrapper.HeaderPacketTypeSize;
        const int SendDefaultReserveSize = 1024;

        /// <summary>
        /// Send message to endpoint of the socket [Protobuf Wrapper]
        /// </summary>
        /// <typeparam name="T">Google.Protobuf.IMessage</typeparam>
        /// <param name="packet">The message to send.</param>
        public void Send<T>(T message) where T : IMessage
        {
#if MEMORY_BUFFER
            SendRaw(message.MSerializeWrapper());
#else
            SendRaw(message.SerializeWrapper());
#endif
        }

        protected sealed override int OnRecvProcess(Memory<byte> buffer)
        {
            if (buffer.Length < MinimumPacketLength) return 0;

            int processed = 0;

            while (processed < buffer.Length)
            {
                // size contains the length of the packet type and message.
                ushort size = BitConverter.ToUInt16(buffer.Span.Slice(processed, PacketWrapper.HeaderMessageLengthSize));
                processed += PacketWrapper.HeaderMessageLengthSize;

                if (size + processed > buffer.Length) break;

                // The data should be [packet type, 2][message].
                ReadOnlySpan<byte> data = buffer.Span.Slice(processed, size);
                processed += size;

                Console.WriteLine("The process length: {0}, size: {1}", data.Length, size);
                OnRecv(data);
            }

            return processed;
        }

        protected sealed override int OnRecvProcess(ArraySegment<byte> buffer)
        {
            if (buffer.Count < MinimumPacketLength) return 0;

            int processed = 0;

            while (processed < buffer.Count)
            {
                // size contains the length of the packet type and message.
                ushort size = BitConverter.ToUInt16(buffer.Slice(processed, PacketWrapper.HeaderMessageLengthSize));
                processed += PacketWrapper.HeaderMessageLengthSize;

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

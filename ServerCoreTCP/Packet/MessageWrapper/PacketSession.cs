using System;
using Google.Protobuf;
using ServerCoreTCP.Utils;

namespace ServerCoreTCP.MessageWrapper
{
    public abstract class PacketSession : Session
    {
        public const int MinimumPacketLength = Defines.PACKET_HEADER_SIZE + Defines.PACKET_DATATYPE_SIZE;

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
            ReadOnlySpan<byte> buf = buffer;
            if (buffer.Count < MinimumPacketLength) return 0;

            int processed = 0;

            while (processed < buffer.Count)
            {
                // contains processed += sizeof(ushort)
                ushort bodySize = buf.ToUInt16(ref processed); // parsing header (packet size)

                if (bodySize + processed  > buffer.Count) break;

                // The data should be [packet type(2 or 4)][message].
                ReadOnlySpan<byte> data = buffer.Slice(processed, bodySize);
                processed += bodySize;

                OnRecv(data);
            }

            return processed;
        }
    }
}
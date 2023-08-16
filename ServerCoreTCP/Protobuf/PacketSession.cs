using Google.Protobuf;
using ServerCoreTCP.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;

namespace ServerCoreTCP.Protobuf
{
    public abstract class PacketSession : Session
    {
        const int MinimumPacketLength = 2;

        /// <summary>
        /// Send message packet to endpoint of the socket. [Protobuf]
        /// </summary>
        /// <typeparam name="T">Google.Protobuf.IMessage and ServerCoreTCP.Protobuf.IPacket</typeparam>
        /// <param name="message">The message to send.</param>
        public void Send<T>(T message) where T : IMessage
        {
            int size = message.CalculateSize();
#if MEMORY_BUFFER
            var sendBuffer = Protobuf.MSerialize(message);
#else
            var sendBuffer = Protobuf.Serialize(message);
#endif

            lock (_lock)
            {
                _sendQueue.Enqueue(sendBuffer);

                if (_sendPendingList.Count == 0) RegisterSend();
            }
        }

#if MEMORY_BUFFER
        /// <summary>
        /// Check the received buffer. If there are multiple packet data on the buffer, each data is processed separately. OnRecv will be called here.
        /// </summary>
        /// <param name="buffer">The buffer received on socket.</param>
        /// <returns>The length of processed bytes.</returns>
        protected sealed override int OnRecvProcess(Memory<byte> buffer)
        {
            // If the size of received buffer is shorter than the header size, it is not the whole data.
            if (buffer.Length < MinimumPacketLength) return 0;

            int processed = 0;

            while (processed < buffer.Length)
            {
                // Note: the protobuf packet buffer (The size type is fixed32 that has 4 bytes on PROTO and it becomes uint type on C#)
                // [tag, 1][size, 4][tag, 1][pacektType, 1][data, ~]
                // Get total size of the unit packet (sizeof(uint) = 4)
                // Jump the tag size (1 byte)
                //int size = (int)BitConverter.ToUInt32(buffer.Span.Slice(processed + 1, HeaderSize));
                int size = (int)Base128Encoding.ReadUInt32(buffer.Span.Slice(processed), out int bytesRead);
                processed += bytesRead;

                if (size + processed > buffer.Length) break;

                ReadOnlySpan<byte> data = buffer.Span.Slice(processed, size);
                processed += size;

                OnRecv(data);
            }

            return processed;
        }
#else
        /// <summary>
        /// Check the received buffer. If there are multiple packet data on the buffer, each data is processed separately. OnRecv will be called here.
        /// </summary>
        /// <param name="buffer">The buffer received on socket.</param>
        /// <returns>The length of processed bytes.</returns>
        protected sealed override int OnRecvProcess(ArraySegment<byte> buffer)
        {
            // If the size of received buffer is shorter than the header size, it is not the whole data.
            if (buffer.Count < MinimumPacketLength) return 0;

            int processed = 0;

            while (processed < buffer.Count)
            {
                // Note: the protobuf packet buffer (The size type is fixed32 that has 4 bytes on PROTO and it becomes uint type on C#)
                // [tag, 1][size, 4][tag, 1][pacektType, 1][data, ~]
                // Get total size of the unit packet (sizeof(uint) = 4)
                // Jump the tag size (1 byte)
                int size = (int)Base128Encoding.ReadUInt32(buffer.Slice(processed), out int bytesRead);
                processed += bytesRead;

                if (size + processed > buffer.Count) break;

                ReadOnlySpan<byte> data = buffer.Slice(processed, size);
                processed += size;

                OnRecv(data);
            }

            return processed;
        }
#endif
    }
}

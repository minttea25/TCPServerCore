//#define MEMORY_BUFFER

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Google.Protobuf;

namespace ServerCoreTCP.CustomBuffer
{
    public abstract class PacketSession : Session
    {
        const int HeaderSize = sizeof(ushort);

        /// <summary>
        /// Send message packet to endpoint of the socket. [Custom Packet]
        /// </summary>
        /// <param name="packet">The packet to send</param>
        public void Send(IPacket packet)
        {
#if MEMORY_BUFFER
            SendRaw(packet.MSerialize());
#else
            SendRaw(packet.Serialize());
#endif
        }

        protected sealed override int OnRecvProcess(Memory<byte> buffer)
        {
            if (buffer.Length < HeaderSize) return 0;

            int processed = 0;

            while (processed < buffer.Length)
            {
                // size: the whole size of the packet
                ushort size = BitConverter.ToUInt16(buffer.Span.Slice(processed, HeaderSize));

                if (size + processed > buffer.Length) break;

                ReadOnlySpan<byte> data = buffer.Span.Slice(processed, size);
                processed += size;

                OnRecv(data);
            }

            return processed;
        }

        protected sealed override int OnRecvProcess(ArraySegment<byte> buffer)
        {
            if (buffer.Count < HeaderSize) return 0;

            int processed = 0;

            while (processed < buffer.Count)
            {
                // size: the whole size of the packet
                ushort size = BitConverter.ToUInt16(buffer.Slice(processed, HeaderSize));

                if (size + processed > buffer.Count) break;

                ReadOnlySpan<byte> data = buffer.Slice(processed, size);
                processed += size;

                OnRecv(data);
            }

            return processed;
        }
    }
}

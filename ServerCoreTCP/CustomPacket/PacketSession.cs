#if CUSTOM_PACKET

using System;

namespace ServerCoreTCP.CustomPacket
{
    public abstract class PacketSession : Session
    {
        const int MinimumPacketLength = HeaderSize;
        const int HeaderSize = sizeof(ushort);

        public void Send(IPacket data)
        {

            SendRaw(data.Serialize());

        }

        protected sealed override int OnRecvProcess(ArraySegment<byte> buffer)
        {
            if (buffer.Count < MinimumPacketLength) return 0;

            int processed = 0;

            while (processed < buffer.Count)
            {
                if (buffer.Count < HeaderSize) break;

                // size contains the length of the packet type and message.
                ushort size = BitConverter.ToUInt16(buffer.Slice(processed, HeaderSize));
                processed += HeaderSize;

                if (size + processed > buffer.Count) break;

                ReadOnlySpan<byte> data = buffer.Slice(processed, size);
                OnRecv(data);
                processed += size;
            }

            return processed;
        }
    }
}
#endif
#if FLATBUFFERS
using Google.FlatBuffers;
using ServerCoreTCP;
using System;
using System.Collections.Generic;

namespace Test.Flatbuffers
{
    public class PacketManager
    {
        #region Singleton
        static PacketManager _instance = null;
        public static PacketManager Instance
        {
            get
            {
                _instance ??= new PacketManager();
                return _instance;
            }
        }
        #endregion

        readonly Dictionary<ushort, Action<ByteBuffer, Session>> _handlers = new Dictionary<ushort, Action<ByteBuffer, Session>>();

        public void Init()
        {
            _handlers.Add(ushort.MaxValue - 1, PacketHandler.TestPacketHandler);
        }

        public void OnRecvPacket(Session session, ArraySegment<byte> buffer, int offset, int count, Action<ByteBuffer, Session> callback = null)
        {
            ushort pktId = (ushort)(buffer[offset] | (buffer[offset + 1] << 8));

            if (_handlers.TryGetValue(pktId, out var handler))
            {
                ByteBuffer bb = new(buffer.Array, offset + Defines.PACKET_ID_SIZE);
                if (callback != null) callback.Invoke(bb, session);
                else handler.Invoke(bb, session);
            }
        }
    }
}

#endif
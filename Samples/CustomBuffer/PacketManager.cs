using System;
using System.Collections.Generic;

using ServerCoreTCP;

namespace ServerCoreTCP.CustomBuffer
{
    public class PacketManager
    {
        #region Singleton
        static PacketManager _instance = new PacketManager();
        public static PacketManager Instance { get { return _instance; } }
        #endregion

#if MEMORY_BUFFER
        /// <summary>
        /// Key is Packets(ushort); Value: the func returns a created packet with the received buffer.
        /// </summary>
        Dictionary<ushort, Func<Session, Memory<byte>, IPacket>> _packetFactory = new Dictionary<ushort, Func<Session, Memory<byte>, IPacket>>();
#else
        /// <summary>
        /// Key is Packets(ushort); Value: the func returns a created packet with the received buffer.
        /// </summary>
        Dictionary<ushort, Func<Session, ArraySegment<byte>, IPacket>> _packetFactory = new Dictionary<ushort, Func<Session, ArraySegment<byte>, IPacket>>();
#endif
        /// <summary>
        /// Value: action which handles the packet(ushort, Packets).
        /// </summary>
        Dictionary<ushort, Action<IPacket, Session>> _handlers = new Dictionary<ushort, Action<IPacket, Session>>();

        public PacketManager()
        {
            _packetFactory.Add((ushort)Packets.TestPacket, MakePacket<TestPacket>);
            _handlers.Add((ushort)Packets.TestPacket, PacketHandler.TestPacketHandler);
            
            
        }
#if MEMORY_BUFFER
        public void OnRecvPacket(Session session, Memory<byte> buffer, Action<Session, IPacket> callback = null)
        {
            int offset = 0;

            // The whole size of the received packet
            ushort size = BitConverter.ToUInt16(buffer.Span.Slice(offset, sizeof(ushort)));
            offset += sizeof(ushort);

            // The type of the received packet
            ushort pkt = BitConverter.ToUInt16(buffer.Span.Slice(offset, sizeof(ushort)));
            offset += sizeof(ushort);

            if (_packetFactory.TryGetValue(pkt, out var factory))
            {
                IPacket packet = factory.Invoke(session, buffer);

                callback?.Invoke(session, packet);

                HandlePacket(packet, session);
            }
        }

        static T MakePacket<T>(Session session, Memory<byte> buffer) where T : IPacket, new()
        {
            T packet = new T();
            packet.MDeserialize(buffer);
            return packet;
        }
#else
        public void OnRecvPacket(Session session, ArraySegment<byte> buffer, Action<Session, IPacket> callback = null)
        {
            int offset = 0;

            // The whole size of the received packet
            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset + offset);
            offset += sizeof(ushort);

            // The type of the received packet
            ushort pkt = BitConverter.ToUInt16(buffer.Array, buffer.Offset + offset);
            offset += sizeof(ushort);

            if (_packetFactory.TryGetValue(pkt, out var factory))
            {
                IPacket packet = factory.Invoke(session, buffer);

                callback?.Invoke(session, packet);

                HandlePacket(packet, session);
            }
        }

        static T MakePacket<T>(Session session, ArraySegment<byte> buffer) where T : IPacket, new()
        {
            T packet = new T();
            packet.Deserialize(buffer);
            return packet;
        }
#endif

        void HandlePacket(IPacket packet, Session session)
        {
            if (_handlers.TryGetValue(packet.PacketType, out var action))
            {
                action.Invoke(packet, session);
            }
        }
    }
}

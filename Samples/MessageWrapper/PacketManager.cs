using Google.Protobuf;
using System;
using System.Collections.Generic;

namespace ServerCoreTCP.MessageWrapper
{
    public class PacketManager
    {
        public const int PacketTypeLength = sizeof(ushort);

        #region Singleton
        readonly static PacketManager _instance = new PacketManager();
        public static PacketManager Instance => _instance;
        #endregion

        readonly Dictionary<ushort, MessageParser> _messageTypes = new Dictionary<ushort, MessageParser>();
        readonly Dictionary<ushort, Action<IMessage, Session>> _handlers = new Dictionary<ushort, Action<IMessage, Session>>();

        PacketManager()
        {
            // _messageTypes.Add((ushort)PacketType.Pvector3, Vector3.Parser);
            // _handlers.Add((ushort)PacketType.Pvector3, PacketHandler.Vector3PacketHandler);
        }

        /// <summary>
        /// Assemble the data to message and handles the result according to the Paceket Type.
        /// </summary>
        /// <param name="session">The session that received the data.</param>
        /// <param name="buffer">The buffer that contains the packet type and serialized message.</param>
        /// <param name="callback">The another callback function, not PacketHandler.</param>
        public void OnRecvPacket(Session session, ReadOnlySpan<byte> buffer, Action<Session, IMessage> callback = null)
        {
            // Note: buffer contains the type and serialized message.
            ushort packetType = ReadPacketType(buffer);

            if (_messageTypes.TryGetValue(packetType, out var parser))
            {
                var msg = parser.ParseFrom(buffer.Slice(PacketTypeLength));
                callback?.Invoke(session, msg);
                HandlePacket(packetType, msg, session);
            }
        }

        static ushort ReadPacketType(ReadOnlySpan<byte> buffer)
        {
            return BitConverter.ToUInt16(buffer.Slice(0, PacketTypeLength));
        }

        void HandlePacket(ushort packetType, IMessage msg, Session session)
        {
            if (_handlers.TryGetValue(packetType, out var handler))
            {
                handler.Invoke(msg, session);
            }
        }
    }
}

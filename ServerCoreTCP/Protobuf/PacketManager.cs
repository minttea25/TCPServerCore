using System;
using System.Collections.Generic;

using Google.Protobuf;

namespace ServerCoreTCP.Protobuf
{
    public class PacketManager
    {
        public const int PacketTypeBytesOffset = 6;
        public const int PacketTypeBytesLength = 1;

        #region Singleton
        readonly static PacketManager _instance = new();
        public static PacketManager Instance => _instance;
        #endregion

        public Dictionary<ushort, MessageParser> _messageAssmeblers => _messageTypes; 

        readonly Dictionary<ushort, MessageParser> _messageTypes = new();
        readonly Dictionary<ushort, Action<IMessage, Session>> _handlers = new();

        PacketManager()
        {
            _messageTypes.Add((ushort)PacketType.Ptest1, Test1.Parser);
            _handlers.Add((ushort)PacketType.Ptest1, PacketHandler.Test1PacketHandler);

            _messageTypes.Add((ushort)PacketType.Ptest2, Test2.Parser);
            _handlers.Add((ushort)PacketType.Ptest2, PacketHandler.Test2PacketHandler);

            _messageTypes.Add((ushort)PacketType.Pvector3, Vector3.Parser);
            _handlers.Add((ushort)PacketType.Pvector3, PacketHandler.Vector3PacketHandler);
        }

        public void OnRecvPacket(Session session, ReadOnlySpan<byte> buffer, Action<Session, IMessage> callback = null)
        {
            ushort packetType = (ushort)ReadPacketType(buffer);

            if (_messageTypes.TryGetValue(packetType, out var parser))
            {
                var msg = parser.ParseFrom(buffer);
                HandlePacket(packetType, msg, session);
            }
        }

        static int ReadPacketType(ReadOnlySpan<byte> buffer)
        {
            ReadOnlySpan<byte> span = buffer.Slice(PacketTypeBytesOffset, PacketTypeBytesLength);
            using CodedInputStream stream = new(span.ToArray());
            return stream.ReadEnum();
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

﻿using Google.Protobuf;
using System;
using System.Collections.Generic;

namespace ServerCoreTCP.ProtobufWrapper
{
    public class PacketManager
    {
        public const int PacketSizeLength = sizeof(ushort);
        public const int PacketTypeLength = sizeof(ushort);

        #region Singleton
        readonly static PacketManager _instance = new();
        public static PacketManager Instance => _instance;
        #endregion

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
            ushort packetType = ReadPacketType(buffer);

            if (_messageTypes.TryGetValue(packetType, out var parser))
            {
                var msg = parser.ParseFrom(buffer.Slice(PacketSizeLength + PacketTypeLength));
                callback?.Invoke(session, msg);
                HandlePacket(packetType, msg, session);
            }
        }

        static ushort ReadPacketType(ReadOnlySpan<byte> buffer)
        {
            return BitConverter.ToUInt16(buffer.Slice(PacketSizeLength, PacketTypeLength));
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

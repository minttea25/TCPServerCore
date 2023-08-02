﻿using System;
using System.Collections.Generic;
using System.Threading;
using Google.Protobuf;
using ServerCoreTCP.Utils;

namespace ServerCoreTCP.Protobuf
{
    public class PacketManager
    {
        public const int PacketTypeTag = 1;

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

        /// <summary>
        /// Assemble the data to message and handles the result according to the Paceket Type.
        /// </summary>
        /// <param name="session">The session that received the data.</param>
        /// <param name="buffer">The buffer that contains only serialized message. (The message continas the Packet Type)</param>
        /// <param name="callback">The another callback function, not PacketHandler.</param>
        public void OnRecvPacket(Session session, ReadOnlySpan<byte> buffer, Action<Session, IMessage> callback = null)
        {
            ushort packetType = (ushort)ReadPacketType(buffer);

            if (_messageTypes.TryGetValue(packetType, out var parser))
            {
                var msg = parser.ParseFrom(buffer);
                callback?.Invoke(session, msg);
                HandlePacket(packetType, msg, session);
            }
        }

        static int ReadPacketType(ReadOnlySpan<byte> buffer)
        {
            uint tag = Base128Encoding.ReadUInt32(buffer, out int bytesRead);
            if (tag != (PacketTypeTag << 3)) return 0; // return invalid type

            return (int)Base128Encoding.ReadUInt32(buffer.Slice(bytesRead), out _);
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

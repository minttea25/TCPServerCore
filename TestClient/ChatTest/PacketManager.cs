using System;
using System.Collections.Generic;

using ChatTest;
using Google.Protobuf;
using ServerCoreTCP;
using ServerCoreTCP.Utils;

namespace TestClient.ChatTest
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
            _messageTypes.Add((ushort)PacketType.PCResEnterRoom, C_ResEnterRoom.Parser);
            _handlers.Add((ushort)PacketType.PCResEnterRoom, PacketHandler.ResEnterRoomPacketHandler);

            _messageTypes.Add((ushort)PacketType.PCEnterRoom, C_EnterRoom.Parser);
            _handlers.Add((ushort)PacketType.PCEnterRoom, PacketHandler.EnterRoomPacketHandler);

            _messageTypes.Add((ushort)PacketType.PCLeaveRoom, C_LeaveRoom.Parser);
            _handlers.Add((ushort)PacketType.PCLeaveRoom, PacketHandler.LeaveRoomPacketHandler);

            _messageTypes.Add((ushort)PacketType.PCChat, C_Chat.Parser);
            _handlers.Add((ushort)PacketType.PCChat, PacketHandler.ChatPacketHandler);
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

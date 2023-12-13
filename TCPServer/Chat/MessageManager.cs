using System;
using System.Collections.Generic;

using Google.Protobuf;

using ServerCoreTCP;
using ServerCoreTCP.MessageWrapper;
using ServerCoreTCP.Utils;

namespace Chat
{
    public class MessageManager
    {
        #region Singleton
        static MessageManager _instance = null;
        public static MessageManager Instance
        {
            get
            {
                if (_instance == null) _instance = new MessageManager();
                return _instance;
            }
        }
        #endregion

#if PACKET_TYPE_INT
        readonly Dictionary<uint, MessageParser> _parsers = new Dictionary<uint, MessageParser>();
        readonly Dictionary<uint, Action<IMessage, Session>> _handlers = new Dictionary<uint, Action<IMessage, Session>>();
#else
        readonly Dictionary<ushort, MessageParser> _parsers = new Dictionary<ushort, MessageParser>();
        readonly Dictionary<ushort, Action<IMessage, Session>> _handlers = new Dictionary<ushort, Action<IMessage, Session>>();
#endif

        MessageManager()
        {
        }

        /// <summary>
        /// Must be called before use MessageManager in multi-thread environment.
        /// </summary>
        public void Init()
        {
#if PACKET_TYPE_INT
            MessageWrapper.PacketMap.Add(typeof(ChatBase), (uint)PacketType.P_ChatBase);
            MessageWrapper.PacketMap.Add(typeof(SSendChatText), (uint)PacketType.P_SSendChatText);
            MessageWrapper.PacketMap.Add(typeof(SSendChatIcon), (uint)PacketType.P_SSendChatIcon);
            MessageWrapper.PacketMap.Add(typeof(CRecvChatText), (uint)PacketType.P_CRecvChatText);
            MessageWrapper.PacketMap.Add(typeof(CRecvChatIcon), (uint)PacketType.P_CRecvChatIcon);
            MessageWrapper.PacketMap.Add(typeof(SReqRoomList), (uint)PacketType.P_SReqRoomList);
            MessageWrapper.PacketMap.Add(typeof(CResRoomList), (uint)PacketType.P_CResRoomList);
            MessageWrapper.PacketMap.Add(typeof(SReqCreateRoom), (uint)PacketType.P_SReqCreateRoom);
            MessageWrapper.PacketMap.Add(typeof(CResCreateRoom), (uint)PacketType.P_CResCreateRoom);
            MessageWrapper.PacketMap.Add(typeof(SLeaveRoom), (uint)PacketType.P_SLeaveRoom);
            MessageWrapper.PacketMap.Add(typeof(CLeaveRoom), (uint)PacketType.P_CLeaveRoom);
            MessageWrapper.PacketMap.Add(typeof(CRemovedRoom), (uint)PacketType.P_CRemovedRoom);
            MessageWrapper.PacketMap.Add(typeof(CUserAuthRes), (uint)PacketType.P_CUserAuthRes);
            MessageWrapper.PacketMap.Add(typeof(SUserAuthReq), (uint)PacketType.P_SUserAuthReq);

#else
            MessageWrapper.PacketMap.Add(typeof(ChatBase), (ushort)PacketType.P_ChatBase);
            MessageWrapper.PacketMap.Add(typeof(SSendChatText), (ushort)PacketType.P_SSendChatText);
            MessageWrapper.PacketMap.Add(typeof(SSendChatIcon), (ushort)PacketType.P_SSendChatIcon);
            MessageWrapper.PacketMap.Add(typeof(CRecvChatText), (ushort)PacketType.P_CRecvChatText);
            MessageWrapper.PacketMap.Add(typeof(CRecvChatIcon), (ushort)PacketType.P_CRecvChatIcon);
            MessageWrapper.PacketMap.Add(typeof(SReqRoomList), (ushort)PacketType.P_SReqRoomList);
            MessageWrapper.PacketMap.Add(typeof(CResRoomList), (ushort)PacketType.P_CResRoomList);
            MessageWrapper.PacketMap.Add(typeof(SReqCreateRoom), (ushort)PacketType.P_SReqCreateRoom);
            MessageWrapper.PacketMap.Add(typeof(CResCreateRoom), (ushort)PacketType.P_CResCreateRoom);
            MessageWrapper.PacketMap.Add(typeof(SLeaveRoom), (ushort)PacketType.P_SLeaveRoom);
            MessageWrapper.PacketMap.Add(typeof(CLeaveRoom), (ushort)PacketType.P_CLeaveRoom);
            MessageWrapper.PacketMap.Add(typeof(CRemovedRoom), (ushort)PacketType.P_CRemovedRoom);
            MessageWrapper.PacketMap.Add(typeof(CUserAuthRes), (ushort)PacketType.P_CUserAuthRes);
            MessageWrapper.PacketMap.Add(typeof(SUserAuthReq), (ushort)PacketType.P_SUserAuthReq);

#endif

            _parsers.Add(MessageWrapper.PacketMap[typeof(SSendChatText)], SSendChatText.Parser);
            _handlers.Add(MessageWrapper.PacketMap[typeof(SSendChatText)], MessageHandler.SSendChatTextMessageHandler);

            _parsers.Add(MessageWrapper.PacketMap[typeof(SSendChatIcon)], SSendChatIcon.Parser);
            _handlers.Add(MessageWrapper.PacketMap[typeof(SSendChatIcon)], MessageHandler.SSendChatIconMessageHandler);

            _parsers.Add(MessageWrapper.PacketMap[typeof(SReqRoomList)], SReqRoomList.Parser);
            _handlers.Add(MessageWrapper.PacketMap[typeof(SReqRoomList)], MessageHandler.SReqRoomListMessageHandler);

            _parsers.Add(MessageWrapper.PacketMap[typeof(SReqCreateRoom)], SReqCreateRoom.Parser);
            _handlers.Add(MessageWrapper.PacketMap[typeof(SReqCreateRoom)], MessageHandler.SReqCreateRoomMessageHandler);

            _parsers.Add(MessageWrapper.PacketMap[typeof(SLeaveRoom)], SLeaveRoom.Parser);
            _handlers.Add(MessageWrapper.PacketMap[typeof(SLeaveRoom)], MessageHandler.SLeaveRoomMessageHandler);

            _parsers.Add(MessageWrapper.PacketMap[typeof(SUserAuthReq)], SUserAuthReq.Parser);
            _handlers.Add(MessageWrapper.PacketMap[typeof(SUserAuthReq)], MessageHandler.SUserAuthReqMessageHandler);


        }

#if PACKET_TYPE_INT
        /// <summary>
        /// Assemble the data to message and handles the result according to the Paceket Type.
        /// </summary>
        /// <param name="session">The session that received the data.</param>
        /// <param name="buffer">The buffer that contains the packet type and serialized message.</param>
        /// <param name="callback">The another callback function, not PacketHandler.</param>
        public void OnRecvPacket(Session session, ReadOnlySpan<byte> buffer, Action<uint, Session, IMessage> callback = null)
        {
            // Note: buffer contains the type (uint or ushort) and serialized message.
            uint packetType = ReadPacketType(buffer);

            if (_parsers.TryGetValue(packetType, out var parser))
            {
                var msg = parser.ParseFrom(buffer.Slice(Defines.PACKET_DATATYPE_SIZE));

                if (callback != null) callback.Invoke(packetType, session, msg);
                else HandlePacket(packetType, msg, session);
            }
        }
#else
        /// <summary>
        /// Assemble the data to message and handles the result according to the Paceket Type.
        /// </summary>
        /// <param name="session">The session that received the data.</param>
        /// <param name="buffer">The buffer that contains the packet type and serialized message.</param>
        /// <param name="callback">The another callback function, not PacketHandler.</param>
        public void OnRecvPacket(Session session, ReadOnlySpan<byte> buffer, Action<ushort, Session, IMessage> callback = null)
        {
            // Note: buffer contains the type (uint or ushort) and serialized message.
            ushort packetType = ReadPacketType(buffer);

            if (_parsers.TryGetValue(packetType, out var parser))
            {
                var msg = parser.ParseFrom(buffer.Slice(Defines.PACKET_DATATYPE_SIZE));

                if (callback != null) callback.Invoke(packetType, session, msg);
                else HandlePacket(packetType, msg, session);
            }
        }
#endif

#if PACKET_TYPE_INT
        static uint ReadPacketType(ReadOnlySpan<byte> buffer)
        {
            return buffer.ToUInt32();
        }

        void HandlePacket(uint packetType, IMessage msg, Session session)
        {
            if (_handlers.TryGetValue(packetType, out var handler))
            {
                handler.Invoke(msg, session);
            }
        }
#else
        static ushort ReadPacketType(ReadOnlySpan<byte> buffer)
        {
            return buffer.ToUInt16();
        }

        public void HandlePacket(ushort packetType, IMessage msg, Session session)
        {
            if (_handlers.TryGetValue(packetType, out var handler))
            {
                handler.Invoke(msg, session);
            }
        }
#endif
    }
}

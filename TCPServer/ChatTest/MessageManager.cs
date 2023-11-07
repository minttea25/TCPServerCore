﻿using Google.Protobuf;
using System;
using System.Collections.Generic;
using ServerCoreTCP;
using ServerCoreTCP.MessageWrapper;

namespace Chat
{
    public enum PacketType : ushort
    {
        P_INVALID = 0,
        P_CResSendChat = 1,
        P_ChatBase = 2,
        P_SChatText = 3,
        P_SChatIcon = 4,
        P_CRecvChatText = 5,
        P_CRecvChatIcon = 6,
        P_SReqRoomList = 7,
        P_CResRoomList = 8,
        P_RoomInfo = 9,
        P_SReqCreateRoom = 10,
        P_CResCreateRoom = 11,
        P_SEnterRoom = 12,
        P_CEnterRoom = 13,
        P_CNewUserEnterRoom = 14,
        P_SLeaveRoom = 15,
        P_CLeaveRoom = 16,
        P_SRemoveRoom = 17,
        P_CRemovedRoom = 18,
        P_UserInfo = 19,
        P_CUserAuthRes = 20,
        P_SUserAuthReq = 21,

    }

    public class MessageManager
    {
        public const int MessageTypeLength = sizeof(ushort);

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

        readonly Dictionary<ushort, MessageParser> _messageTypes = new Dictionary<ushort, MessageParser>();
        readonly Dictionary<ushort, Action<IMessage, Session>> _handlers = new Dictionary<ushort, Action<IMessage, Session>>();

        MessageManager()
        {
            MessageWrapper.PacketMap.Add(typeof(CResSendChat), (ushort)PacketType.P_CResSendChat);
            MessageWrapper.PacketMap.Add(typeof(ChatBase), (ushort)PacketType.P_ChatBase);
            MessageWrapper.PacketMap.Add(typeof(SChatText), (ushort)PacketType.P_SChatText);
            MessageWrapper.PacketMap.Add(typeof(SChatIcon), (ushort)PacketType.P_SChatIcon);
            MessageWrapper.PacketMap.Add(typeof(CRecvChatText), (ushort)PacketType.P_CRecvChatText);
            MessageWrapper.PacketMap.Add(typeof(CRecvChatIcon), (ushort)PacketType.P_CRecvChatIcon);
            MessageWrapper.PacketMap.Add(typeof(SReqRoomList), (ushort)PacketType.P_SReqRoomList);
            MessageWrapper.PacketMap.Add(typeof(CResRoomList), (ushort)PacketType.P_CResRoomList);
            MessageWrapper.PacketMap.Add(typeof(RoomInfo), (ushort)PacketType.P_RoomInfo);
            MessageWrapper.PacketMap.Add(typeof(SReqCreateRoom), (ushort)PacketType.P_SReqCreateRoom);
            MessageWrapper.PacketMap.Add(typeof(CResCreateRoom), (ushort)PacketType.P_CResCreateRoom);
            MessageWrapper.PacketMap.Add(typeof(SEnterRoom), (ushort)PacketType.P_SEnterRoom);
            MessageWrapper.PacketMap.Add(typeof(CEnterRoom), (ushort)PacketType.P_CEnterRoom);
            MessageWrapper.PacketMap.Add(typeof(CNewUserEnterRoom), (ushort)PacketType.P_CNewUserEnterRoom);
            MessageWrapper.PacketMap.Add(typeof(SLeaveRoom), (ushort)PacketType.P_SLeaveRoom);
            MessageWrapper.PacketMap.Add(typeof(CLeaveRoom), (ushort)PacketType.P_CLeaveRoom);
            MessageWrapper.PacketMap.Add(typeof(SRemoveRoom), (ushort)PacketType.P_SRemoveRoom);
            MessageWrapper.PacketMap.Add(typeof(CRemovedRoom), (ushort)PacketType.P_CRemovedRoom);
            MessageWrapper.PacketMap.Add(typeof(UserInfo), (ushort)PacketType.P_UserInfo);
            MessageWrapper.PacketMap.Add(typeof(CUserAuthRes), (ushort)PacketType.P_CUserAuthRes);
            MessageWrapper.PacketMap.Add(typeof(SUserAuthReq), (ushort)PacketType.P_SUserAuthReq);


            _messageTypes.Add(MessageWrapper.PacketMap[typeof(ChatBase)], ChatBase.Parser);
            _handlers.Add(MessageWrapper.PacketMap[typeof(ChatBase)], MessageHandler.ChatBaseMessageHandler);

            _messageTypes.Add(MessageWrapper.PacketMap[typeof(SChatText)], SChatText.Parser);
            _handlers.Add(MessageWrapper.PacketMap[typeof(SChatText)], MessageHandler.SChatTextMessageHandler);

            _messageTypes.Add(MessageWrapper.PacketMap[typeof(SChatIcon)], SChatIcon.Parser);
            _handlers.Add(MessageWrapper.PacketMap[typeof(SChatIcon)], MessageHandler.SChatIconMessageHandler);

            _messageTypes.Add(MessageWrapper.PacketMap[typeof(SReqRoomList)], SReqRoomList.Parser);
            _handlers.Add(MessageWrapper.PacketMap[typeof(SReqRoomList)], MessageHandler.SReqRoomListMessageHandler);

            _messageTypes.Add(MessageWrapper.PacketMap[typeof(RoomInfo)], RoomInfo.Parser);
            _handlers.Add(MessageWrapper.PacketMap[typeof(RoomInfo)], MessageHandler.RoomInfoMessageHandler);

            _messageTypes.Add(MessageWrapper.PacketMap[typeof(SReqCreateRoom)], SReqCreateRoom.Parser);
            _handlers.Add(MessageWrapper.PacketMap[typeof(SReqCreateRoom)], MessageHandler.SReqCreateRoomMessageHandler);

            _messageTypes.Add(MessageWrapper.PacketMap[typeof(SEnterRoom)], SEnterRoom.Parser);
            _handlers.Add(MessageWrapper.PacketMap[typeof(SEnterRoom)], MessageHandler.SEnterRoomMessageHandler);

            _messageTypes.Add(MessageWrapper.PacketMap[typeof(SLeaveRoom)], SLeaveRoom.Parser);
            _handlers.Add(MessageWrapper.PacketMap[typeof(SLeaveRoom)], MessageHandler.SLeaveRoomMessageHandler);

            _messageTypes.Add(MessageWrapper.PacketMap[typeof(SRemoveRoom)], SRemoveRoom.Parser);
            _handlers.Add(MessageWrapper.PacketMap[typeof(SRemoveRoom)], MessageHandler.SRemoveRoomMessageHandler);

            _messageTypes.Add(MessageWrapper.PacketMap[typeof(UserInfo)], UserInfo.Parser);
            _handlers.Add(MessageWrapper.PacketMap[typeof(UserInfo)], MessageHandler.UserInfoMessageHandler);

            _messageTypes.Add(MessageWrapper.PacketMap[typeof(SUserAuthReq)], SUserAuthReq.Parser);
            _handlers.Add(MessageWrapper.PacketMap[typeof(SUserAuthReq)], MessageHandler.SUserAuthReqMessageHandler);


        }

        /// <summary>
        /// Assemble the data to message and handles the result according to the Paceket Type.
        /// </summary>
        /// <param name="session">The session that received the data.</param>
        /// <param name="buffer">The buffer that contains the packet type and serialized message.</param>
        /// <param name="callback">The another callback function, not PacketHandler.</param>
        public void OnRecvPacket(Session session, ReadOnlySpan<byte> buffer, Action<ushort, Session, IMessage> callback = null)
        {
            // Note: buffer contains the type and serialized message.
            ushort packetType = ReadPacketType(buffer);

            if (_messageTypes.TryGetValue(packetType, out var parser))
            {
                var msg = parser.ParseFrom(buffer.Slice(MessageTypeLength));
                if (callback != null) callback?.Invoke(packetType, session, msg);
                else HandlePacket(packetType, msg, session);
            }
        }

        static ushort ReadPacketType(ReadOnlySpan<byte> buffer)
        {
            return BitConverter.ToUInt16(buffer.Slice(0, MessageTypeLength));
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
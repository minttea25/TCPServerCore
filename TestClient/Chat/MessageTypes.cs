using System;

namespace Chat
{
#if PACKET_TYPE_INT
    public enum PacketType : uint
    {
        P_INVALID = 0,
        P_ChatBase = 1,
        P_SSendChatText = 2,
        P_SSendChatIcon = 3,
        P_CRecvChatText = 4,
        P_CRecvChatIcon = 5,
        P_SReqRoomList = 6,
        P_CResRoomList = 7,
        P_SReqCreateRoom = 8,
        P_CResCreateRoom = 9,
        P_SLeaveRoom = 10,
        P_CLeaveRoom = 11,
        P_CRemovedRoom = 12,
        P_CUserAuthRes = 13,
        P_SUserAuthReq = 14,

    }
#else
    public enum PacketType : ushort
    {
        P_INVALID = 0,
        P_ChatBase = 1,
        P_SSendChatText = 2,
        P_SSendChatIcon = 3,
        P_CRecvChatText = 4,
        P_CRecvChatIcon = 5,
        P_SReqRoomList = 6,
        P_CResRoomList = 7,
        P_SReqCreateRoom = 8,
        P_CResCreateRoom = 9,
        P_SLeaveRoom = 10,
        P_CLeaveRoom = 11,
        P_CRemovedRoom = 12,
        P_CUserAuthRes = 13,
        P_SUserAuthReq = 14,

    }
#endif
}

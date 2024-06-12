#if PROTOBUF

using System;

using ServerCoreTCP;
using Google.Protobuf;

namespace Chat.Protobuf
{
    public class MessageHandler
    {
        public static void CRecvChatTextMessageHandler(IMessage message, Session session)
        {
            CRecvChatText msg = message as CRecvChatText;

            // TODO
        }

        public static void CRecvChatIconMessageHandler(IMessage message, Session session)
        {
            CRecvChatIcon msg = message as CRecvChatIcon;

            // TODO
        }

        public static void CResRoomListMessageHandler(IMessage message, Session session)
        {
            CResRoomList msg = message as CResRoomList;

            // TODO
        }

        public static void CResCreateRoomMessageHandler(IMessage message, Session session)
        {
            CResCreateRoom msg = message as CResCreateRoom;

            // TODO
        }

        public static void CLeaveRoomMessageHandler(IMessage message, Session session)
        {
            CLeaveRoom msg = message as CLeaveRoom;

            // TODO
        }

        public static void CRemovedRoomMessageHandler(IMessage message, Session session)
        {
            CRemovedRoom msg = message as CRemovedRoom;

            // TODO
        }

        public static void CUserAuthResMessageHandler(IMessage message, Session session)
        {
            CUserAuthRes msg = message as CUserAuthRes;

            // TODO
        }


    }
}

#endif
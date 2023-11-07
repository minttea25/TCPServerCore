using System;

using ServerCoreTCP;
using Google.Protobuf;
using TestClient;

namespace Chat
{
    public class MessageHandler
    {
        public static void CResSendChatMessageHandler(IMessage message, Session session)
        {
            CResSendChat msg = message as CResSendChat;

            // TODO

        }

        public static void ChatBaseMessageHandler(IMessage message, Session session)
        {
            ChatBase msg = message as ChatBase;

            // TODO
        }

        public static void CRecvChatTextMessageHandler(IMessage message, Session session)
        {
            CRecvChatText msg = message as CRecvChatText;

            // TODO
            var text = msg.Msg;
            Program.Logger.Information("Recv msg: {text}", text);
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

        public static void RoomInfoMessageHandler(IMessage message, Session session)
        {
            RoomInfo msg = message as RoomInfo;

            // TODO
        }

        public static void CResCreateRoomMessageHandler(IMessage message, Session session)
        {
            CResCreateRoom msg = message as CResCreateRoom;

        }

        public static void CEnterRoomMessageHandler(IMessage message, Session session)
        {
            CEnterRoom msg = message as CEnterRoom;
            ServerSession s = session as ServerSession;


            // TODO
            s.TrafficTestAuto();
        }

        public static void CNewUserEnterRoomMessageHandler(IMessage message, Session session)
        {
            CNewUserEnterRoom msg = message as CNewUserEnterRoom;

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

        public static void UserInfoMessageHandler(IMessage message, Session session)
        {
            UserInfo msg = message as UserInfo;

            // TODO
        }

        public static void CUserAuthResMessageHandler(IMessage message, Session session)
        {
            CUserAuthRes msg = message as CUserAuthRes;
            ServerSession s = session as ServerSession;

            // TODO
            var res = msg.AuthRes;
            var info = msg.UserInfo;
            s.userInfo = info;
            Program.Logger.Information("Authed. res =  {res}", res);

            SEnterRoom req = new SEnterRoom()
            {
                UserInfo = s.userInfo,
                RoomId = s.enteredRoomNo,
            };

            s.Send_(req);
        }
    }
}
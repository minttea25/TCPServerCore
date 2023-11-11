using System;
using ServerCoreTCP;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using ChatServer;
using ChatServer.Data;
using ServerCoreTCP.CLogger;

namespace Chat
{
    public class MessageHandler
    {
        public static void RecvLog<T>(T message) where T : IMessage
        {
            CoreLogger.LogRecv(message);
        }

        public static void SEnterRoomMessageHandler(IMessage message, Session session)
        {
            SEnterRoom msg = message as SEnterRoom;
            ClientSession s = session as ClientSession;

            RecvLog(msg);

            Room room;
            if (RoomManager.Instance.Exist(msg.RoomId, out room) == true)
            {

            }
            else
            {
                RoomManager.Instance.CreateNewRoom(msg.RoomId, out room);
            }

            CEnterRoom res = new()
            {
                UserInfo = s.User.UserInfo,
                RoomId = msg.RoomId,
                Res = EnterRoomRes.Ok
            };
            ServerLogger.Send(s, res);
        }

        public static void ChatBaseMessageHandler(IMessage message, Session session)
        {
            ChatBase msg = message as ChatBase;

            // TODO
        }

        public static void SChatTextMessageHandler(IMessage message, Session session)
        {
            SChatText msg = message as SChatText;
            ClientSession s = session as ClientSession;

            RecvLog(msg);

            var roomId = msg.RoomId;
            if (RoomManager.Instance.Exist(roomId, out Room room))
            {
                CResSendChat res = new()
                {
                    SenderId = s.User.Id,
                    Error = SendChatError.Success,
                };
                s.Send(res);
                //ServerLogger.Send(s, res);
                room.SendChatText(s, msg.Msg);
            }
            else
            {
                CoreLogger.LogError("MessageHandler", "Can not find key={0} in RoomManager.Rooms", roomId);
                CResSendChat res = new()
                {
                    SenderId = s.User.Id,
                    Error = SendChatError.NoSuchRoom,
                };
                return;
            }
        }

        public static void SChatIconMessageHandler(IMessage message, Session session)
        {
            SChatIcon msg = message as SChatIcon;
            ClientSession s = session as ClientSession;


        }

        public static void SReqRoomListMessageHandler(IMessage message, Session session)
        {
            SReqRoomList msg = message as SReqRoomList;
            ClientSession s = session as ClientSession;

        }

        public static void RoomInfoMessageHandler(IMessage message, Session session)
        {
            RoomInfo msg = message as RoomInfo;

            // TODO
        }

        public static void SReqCreateRoomMessageHandler(IMessage message, Session session)
        {
            SReqCreateRoom msg = message as SReqCreateRoom;
            ClientSession s = session as ClientSession;

            // TODO
        }

        public static void SLeaveRoomMessageHandler(IMessage message, Session session)
        {
            SLeaveRoom msg = message as SLeaveRoom;
            ClientSession s = session as ClientSession;

            RecvLog(msg);

            s.LeaveRoom(msg.RoomId);
            SessionManager.Instance.ClearSession(s);
        }

        public static void SRemoveRoomMessageHandler(IMessage message, Session session)
        {
            SRemoveRoom msg = message as SRemoveRoom;
            ClientSession s = session as ClientSession;


        }

        public static void UserInfoMessageHandler(IMessage message, Session session)
        {
            UserInfo msg = message as UserInfo;

        }

        public static void SUserAuthReqMessageHandler(IMessage message, Session session)
        {
            SUserAuthReq msg = message as SUserAuthReq;
            ClientSession s = session as ClientSession;

            RecvLog(msg);

            User user = DataManager.Instance.AddNewUser(msg.UserName);
            if (user != null)
            {
                s.SetUser(user);
                CUserAuthRes res = new CUserAuthRes()
                {
                    UserInfo = new() { UserId = user.Id, UserName = user.UserName },
                    AuthRes = UserAuthRes.UserAuthOk,
                };
                ServerLogger.Send(s, res);
            }
            else
            {
                CUserAuthRes res = new CUserAuthRes()
                {
                    UserInfo = new() { UserId = 0, UserName = "" },
                    AuthRes = UserAuthRes.UserAuthDuplicatedName,
                };
                ServerLogger.Send(s, res);
            }
        }
    }
}
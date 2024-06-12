#if PROTOBUF

using System;

using ServerCoreTCP;
using Google.Protobuf;

namespace Chat.Protobuf
{
    public class MessageHandler
    {
        public static void SSendChatTextMessageHandler(IMessage message, Session session)
        {
            SSendChatText msg = message as SSendChatText;

            // TODO
            Console.WriteLine(msg);
        }

        public static void SSendChatIconMessageHandler(IMessage message, Session session)
        {
            SSendChatIcon msg = message as SSendChatIcon;

            // TODO
        }

        public static void SReqRoomListMessageHandler(IMessage message, Session session)
        {
            SReqRoomList msg = message as SReqRoomList;

            // TODO
        }

        public static void SReqCreateRoomMessageHandler(IMessage message, Session session)
        {
            SReqCreateRoom msg = message as SReqCreateRoom;

            // TODO
        }

        public static void SLeaveRoomMessageHandler(IMessage message, Session session)
        {
            SLeaveRoom msg = message as SLeaveRoom;

            // TODO
        }

        public static void SUserAuthReqMessageHandler(IMessage message, Session session)
        {
            SUserAuthReq msg = message as SUserAuthReq;

            // TODO
        }


    }
}
#endif
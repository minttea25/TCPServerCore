using Google.Protobuf;
using ServerCoreTCP;
using System;

using ChatTest;

namespace TestClient.ChatTest
{
    public class PacketHandler
    {
        public static void ResEnterRoomPacketHandler(IMessage message, Session session)
        {
            C_ResEnterRoom res = message as C_ResEnterRoom;
            ServerSession s = session as ServerSession;

            Program.Logger.Information("ResEnterRoomPacket: {res}", res);

            if (res.Success == false)
            {
                Console.WriteLine($"An error ocuured to enter the room: {res.ErrorType}");
                s.Disconnect();
                return;
            }

            s.EnterCompleted(res.UserId, Program.ReqRoomNo);
        }

        public static void EnterRoomPacketHandler(IMessage message, Session session)
        {
            C_EnterRoom enter = message as C_EnterRoom;

            Program.Logger.Information("EnterRoomPacket: {enter}", enter);

            Console.WriteLine($"{enter.UserName} entered.");
        }

        public static void LeaveRoomPacketHandler(IMessage message, Session session)
        {
            C_LeaveRoom leave = message as C_LeaveRoom;
            ServerSession s = session as ServerSession;

            Program.Logger.Information("LeaveRoomPacket: ", leave);

            if (leave.UserName == Program.UserName)
            {
                Console.WriteLine("Leaved the room.");
                s.Disconnect();
            }
            else Console.WriteLine($"{leave.UserName} is leaved the room.");
        }

        public static void ChatPacketHandler(IMessage message, Session session)
        {
            C_Chat chat = message as C_Chat;
            ServerSession s = session as ServerSession;

            Program.Logger.Information("ChatPacket: {chat}", chat);

            if (chat.UserId == s.UserId) return;
            else Console.WriteLine($"{chat.UserName}: {chat.Msg}");
        }
    }
}

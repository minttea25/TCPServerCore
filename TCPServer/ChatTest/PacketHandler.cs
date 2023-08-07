using Google.Protobuf;
using ServerCoreTCP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ChatTest;

namespace TCPServer.ChatTest
{
    public class PacketHandler
    {
        public static void ReqEnterRoomPacketHandler(IMessage message, Session session)
        {
            S_ReqEnterRoom pkt = message as S_ReqEnterRoom;
            ClientSession s = session as ClientSession;

            Console.WriteLine(pkt);

            uint reqRoomNo = pkt.RoomNo;

            if (reqRoomNo == 0)
            {
                C_ResEnterRoom r = new()
                {
                    UserId = s.SessionId,
                    Success = false,
                    ErrorType = AuthErrorType.InvalidRoom
                };
                s.Send(r);
                s.Disconnect();
                return;
            }

            // check the room with the id exists
            if (Program.Rooms.TryGetValue(reqRoomNo, out var _) == false)
            {
                Room room = new(id: reqRoomNo);
                Program.AddRoom(reqRoomNo, room);
            }

            C_ResEnterRoom res = new()
            {
                UserId = s.SessionId,
                Success = true,
                ErrorType = AuthErrorType.Success
            };
            Console.WriteLine(res);

            s.Send(res);

            s.SetUserName(pkt.UserName);
            Program.Rooms[reqRoomNo].AddJob(() => Program.Rooms[reqRoomNo].Enter(s));
        }

        public static void LeaveRoomPacketHandler(IMessage message, Session session)
        {
            S_LeaveRoom leave = message as S_LeaveRoom;
            ClientSession s = session as ClientSession;

            Console.WriteLine(leave);

            s.Room.AddJob(() => s.Room.Leave(s));
        }

        public static void ChatPacketHandler(IMessage message, Session session)
        {
            S_Chat chat = message as S_Chat;
            ClientSession s = session as ClientSession;

            Console.WriteLine(chat);

            s.Room.AddJob(() => s.Room.SendChat(s, chat.Msg));
        }
    }
}

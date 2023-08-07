using System;
using System.Net;
using System.Threading;

using ServerCoreTCP;

namespace TestClient
{
    class Program
    {
        public static string UserName;
        public static uint ReqRoomNo;
        public static ServerSession session;

        static void Main(string[] args)
        {
            Console.Write("Enter the UserName: ");
            UserName = Console.ReadLine();

            Console.Write("Enter the RoomNo to enter: ");
            ReqRoomNo = uint.Parse(Console.ReadLine());

            Thread.Sleep(1000);

            string host = Dns.GetHostName(); // local host name of my pc
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new(address: ipAddr, port: 8888);

            Connector connector = new();
            //connector.Connect(endPoint, () => { return SessionManager.Instance.CreateNewSession(); }, 1);
            connector.Connect(endPoint, () => { session = SessionManager.Instance.CreateNewSession(); return session; }, 1);

            while (true)
            {
                string chat = Console.ReadLine();

                if (chat.Equals("leave"))
                {
                    session.LeaveRoom();
                    Console.WriteLine("Leaving Room...");
                    break;
                }

                if (session != null)
                {
                    session.SendChat(chat);
                }
            }
        }
    }
}

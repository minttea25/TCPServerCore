using System;
using System.Net;
using System.Text;
using System.Threading;
using Serilog;
using Serilog.Core;
using ServerCoreTCP;
using ServerCoreTCP.LoggerDebug;

namespace TestClient
{
    class Program
    {
        public readonly static Logger Logger = new LoggerConfiguration().WriteTo.File(
                    path: LoggerHelper.GetFileName("ClientLogs"),
                    encoding: Encoding.Unicode,
                    flushToDiskInterval: TimeSpan.FromSeconds(1)
                    ).CreateLogger();

        public static string UserName;
        public static uint ReqRoomNo;
        public static ServerSession session;

        static void Main(string[] args)
        {
#if DEBUG
            CoreLogger.Logging = true;
#else
            CoreLogger.Logger = new LoggerConfiguration().WriteTo.File(
                    path: LoggerHelper.GetFileName("CoreLogs"),
                    encoding: Encoding.Unicode,
                    flushToDiskInterval: TimeSpan.FromSeconds(1)
                    ).CreateLogger();
#endif

            Console.Write("Enter the UserName: ");
            UserName = Console.ReadLine();

            Console.Write("Enter the RoomNo to enter: ");
            ReqRoomNo = uint.Parse(Console.ReadLine());

            Thread.Sleep(1000);

            string host = Dns.GetHostName(); // local host name of my pc
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(address: ipAddr, port: 8888);

            Connector connector = new Connector();
            connector.OnConnectCompleted += OnConnectCompleted;
#if RELEASE
            connector.Connect(endPoint, () => { session = SessionManager.Instance.CreateNewSession(); return session; }, timeoutMilliSeconds: 5000);
#else
            //connector.Connect(endPoint, () => { session = SessionManager.Instance.CreateNewSession(); return session; }, timeoutMilliSeconds: 5000, count: 1);
#endif
            Console.WriteLine("Connect after");
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

            Logger.Dispose();
        }

        static void OnConnectCompleted(object sender, Connector.ConnectError error)
        {
            if (error == Connector.ConnectError.Success)
            {
                //
            }
            else
            {
                Console.WriteLine($"error: {error}");
            }
        }
    }

    
}

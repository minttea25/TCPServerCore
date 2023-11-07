using System;
using System.Net;
using System.Text;
using System.Threading;
using Chat;
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
                    ).WriteTo.Console().CreateLogger();

        public static string UserName;

        public static Random rand = new();

        static void Main(string[] args)
        {
            MessageManager.Instance.Init();

#if DEBUG
            CoreLogger.Logging = true;
#else
            CoreLogger.Logger = new LoggerConfiguration().WriteTo.File(
                    path: LoggerHelper.GetFileName("CoreLogs"),
                    encoding: Encoding.Unicode,
                    flushToDiskInterval: TimeSpan.FromSeconds(1)
                    ).CreateLogger();
#endif

            string host = Dns.GetHostName(); // local host name of my pc
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(address: ipAddr, port: 8888);

            Thread.Sleep(1000);

            UserName = "test" + rand.Next(1, 1000);

            ClientService clientService = new ClientService(endPoint, () => { return SessionManager.Instance.CreateNewSession(); }, 100);
            clientService.Start();

            while (true)
            {
                string s = Console.ReadLine();
                if (s == "exit") break;
            }
            SessionManager.Instance.ExitAll();
            Logger.Dispose();
        }
    }


}
using System;
using System.Net;
using System.Text;
using System.Threading;
using Chat;
using Serilog;
using Serilog.Core;
using ServerCoreTCP;
using ServerCoreTCP.CLogger;

namespace TestClient
{
    class Program
    {
        public static Random rand = new();

        static ServerSession session = null;

        static void NetworkTask()
        {
            Thread.CurrentThread.Name = "NetworkTask";
            while (true)
            {
                session?.FlushSend();

                Thread.Yield();
            }
        }

        static void Main(string[] args)
        {
            Thread.CurrentThread.Name = "Main Thread";
            MessageManager.Instance.Init();

            CoreLogger.CreateLoggerWithFlag(
                (uint)(CoreLogger.LoggerSinks.CONSOLE | CoreLogger.LoggerSinks.FILE),
                LoggerConfig.GetDefault());



            string host = Dns.GetHostName(); // local host name of my pc
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(address: ipAddr, port: 8888);

            Thread.Sleep(2000);


            ClientServiceConfig config = ClientServiceConfig.GetDefault();
            config.ClientServiceSessionCount = 1;

            // CAUTION JUST FOR TEST FOR ONLY 1 SESSION
            ClientService clientService
                = new ClientService(
                    endPoint, () => { return  session = new ServerSession(); },
                    config);
            clientService.Start();

            Thread networkTask = new(NetworkTask);
            networkTask.Start();

            while (true)
            {
                string s = Console.ReadLine();
                if (s == "exit") break;
            }



            CoreLogger.StopLogging();
        }
    }


}
using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Chat;
using Serilog;
using Serilog.Core;
using ServerCoreTCP;
using ServerCoreTCP.CLogger;
using ServerCoreTCP.MessageWrapper;
using ServerCoreTCP.Utils;

namespace TestClient
{
    class Program
    {
        public static Random rand = new();

        static ServerSession session = null;

        static void NetworkTask()
        {
            session?.FlushSend();
        }

        static void ClientCommand()
        {
            while (true)
            {
                string s = Console.ReadLine();
                if (s == "exit") break;
            }
            session.Disconnect();
        }

        static void Main(string[] args)
        {
            PacketSession.Encrypt = true ;
            MessageManager.Instance.Init();

            CoreLogger.CreateLoggerWithFlag(
                (uint)(CoreLogger.LoggerSinks.CONSOLE | CoreLogger.LoggerSinks.FILE),
                LoggerConfig.GetDefault());



            string host = Dns.GetHostName(); // local host name of my pc
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(address: ipAddr, port: 8888);

            Thread.Sleep(3000);


            ClientServiceConfig config = ClientServiceConfig.GetDefault();
            config.ClientServiceSessionCount = 1;

            // CAUTION JUST FOR TEST FOR ONLY 1 SESSION
            ClientService clientService
                = new ClientService(
                    endPoint, () => { return  session = new ServerSession(); },
                    config);
            clientService.Start();

            ThreadManager tasks = new ThreadManager(1);

            tasks.AddTask(NetworkTask, "NetworkTask");
            tasks.SetMainTask(ClientCommand);

            tasks.StartTasks();

            CoreLogger.StopLogging();
        }
    }


}
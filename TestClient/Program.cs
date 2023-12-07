using System;
using System.Net;
using System.Text;
using System.Threading;
using Serilog;
using Serilog.Core;
using ServerCoreTCP;
using ServerCoreTCP.CLogger;

namespace TestClient
{
    class Program
    {
        public static Random rand = new();

        static void Main(string[] args)
        {
            CoreLogger.CreateLoggerWithFlag(
                (uint)(CoreLogger.LoggerSinks.CONSOLE | CoreLogger.LoggerSinks.FILE),
                LoggerConfig.GetDefault());



            string host = Dns.GetHostName(); // local host name of my pc
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(address: ipAddr, port: 8888);

            Thread.Sleep(1000);


            ClientServiceConfig config = ClientServiceConfig.GetDefault();
            config.ClientServiceSessionCount = 1;

            //ClientService clientService 
            //    = new ClientService(
            //        endPoint, () => { return SessionManager.Instance.CreateNewSession(); }, 
            //        config);
            //clientService.Start();

            while (true)
            {
                string s = Console.ReadLine();
                if (s == "exit") break;
            }



            CoreLogger.StopLogging();
        }
    }


}
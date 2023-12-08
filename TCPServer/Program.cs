using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Chat;
using Serilog;
using ServerCoreTCP;
using ServerCoreTCP.CLogger;

namespace TCPServer
{
    class Program
    {
        static List<ClientSession> sessions = new();
        static object _lock = new();

        static void NetworkTask()
        {
            Thread.CurrentThread.Name = "NetworkTask";
            while (true)
            {
                foreach (var s in sessions)
                {
                    s.FlushSend();
                }
                Thread.Yield();
            }
        }
        

        static void Main(string[] args)
        {
            Thread.CurrentThread.Name = "Main Thread";
            MessageManager.Instance.Init();

            var config = LoggerConfig.GetDefault();
            config.RestrictedMinimumLevel = Serilog.Events.LogEventLevel.Error;
            CoreLogger.CreateLoggerWithFlag(
                (uint)(CoreLogger.LoggerSinks.CONSOLE | CoreLogger.LoggerSinks.FILE),
                config);

            string host = Dns.GetHostName(); // local host name of my pc
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(address: ipAddr, port: 8888);

            ServerServiceConfig serverConfig = new()
            {
                SessionPoolCount = 100,
                SocketAsyncEventArgsPoolCount = 300,
                ReuseAddress = true,
                RegisterListenCount = 10,
                ListenerBacklogCount = 100,

            };
            //var serverConfig = ServerServiceConfig.GetDefault();

            Server server = new(
                endPoint,
                () => { 
                    var s = new ClientSession(); 
                    lock(_lock)
                    {
                        sessions.Add(s);
                    }
                    return s;
                },
                serverConfig);

            Thread networkTask = new(NetworkTask);
            networkTask.Start();

            while (true)
            {
                string command = Console.ReadLine();

                switch (command)
                {
                    case "start":
                        server.Start();
                        break;

                    case "stop":
                        server.Stop();
                        break;
                    case "session_count":
                        break;
                    case "pooled_session_count":
                        break;
                    case "pooled_saea_count":
                        break;
                    case "collect":
                        break;
                    default:
                        Console.WriteLine($"Unknown Command: {command}");
                        break;
                }
            }
            CoreLogger.StopLogging();
        }
    }
}
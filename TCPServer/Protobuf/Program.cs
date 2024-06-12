#if PROTOBUF

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Chat;
using Serilog;
using ServerCoreTCP;
using ServerCoreTCP.CLogger;
using ServerCoreTCP.Utils;

namespace Chat.Protobuf
{
    class Program
    {
        static Server server = null;
        static List<ClientSession> sessions = new();
        static object _lock = new();

        static void NetworkTask()
        {
            foreach (var s in sessions)
            {
                s.FlushSend();
            }
            Thread.Yield();
        }

        static void ServerCommand()
        {
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
                        return;
                    case "session_count":
                        Console.WriteLine(server.Service.SessionTotalPoolCount);
                        break;
                    case "pooled_session_count":
                        Console.WriteLine(server.Service.SessionCurrentPooledCount);
                        break;
                    case "saea_count":
                        Console.WriteLine(server.Service.SAEATotalPoolCount);
                        break;
                    case "collect":
                        GC.Collect();
                        break;
                    default:
                        Console.WriteLine($"Unknown Command: {command}");
                        break;
                }
            }
        }

        static void Main(string[] args)
        {
            MessageManager.Instance.Init();

            var config = LoggerConfig.GetDefault();
            //config.RestrictedMinimumLevel = Serilog.Events.LogEventLevel.Error;
            CoreLogger.CreateLoggerWithFlag(
                (uint)(CoreLogger.LoggerSinks.CONSOLE | CoreLogger.LoggerSinks.FILE),
                config);

            string host = Dns.GetHostName(); // local host name of my pc
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(address: ipAddr, port: 8888);

            ServerServiceConfig serverConfig = new()
            {
                SessionPoolCount = 1,
                ReuseAddress = true,
                RegisterListenCount = 1,
                ListenerBacklogCount = 100,

            };
            //var serverConfig = ServerServiceConfig.GetDefault();

            server = new(
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

            TaskManager tasks = new();
            tasks.AddTask(NetworkTask);
            tasks.SetMain(ServerCommand);

            tasks.StartTasks();

            CoreLogger.StopLogging();
        }
    }
}
#endif
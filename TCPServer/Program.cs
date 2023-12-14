using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Chat;
using Serilog;
using ServerCoreTCP;
using ServerCoreTCP.CLogger;
using ServerCoreTCP.MessageWrapper;
using ServerCoreTCP.Utils;

namespace TCPServer
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
        }

        static void Main(string[] args)
        {


            PacketSession.Encrypt = true;

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

            ThreadManager tasks = new ThreadManager(1);
            tasks.AddTask(NetworkTask, "NetworkTask");
            tasks.SetMainTask(ServerCommand);

            tasks.StartTasks();

            CoreLogger.StopLogging();
        }
    }
}
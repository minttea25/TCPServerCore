using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Chat;
using Serilog;
using ServerCoreTCP;
using ServerCoreTCP.CLogger;

namespace ChatServer
{
    class Program
    {
        public static bool OnGoing = true;

        static void Main(string[] args)
        {
            //int coreCount = Environment.ProcessorCount;
            //ThreadPool.SetMinThreads(1, 1);
            //ThreadPool.SetMaxThreads(coreCount, coreCount);
            var config = LoggerConfig.GetDefault();
            config.RestrictedMinimumLevel = Serilog.Events.LogEventLevel.Error;
            CoreLogger.CreateLoggerWithFlag(
                (uint)(CoreLogger.LoggerSinks.CONSOLE | CoreLogger.LoggerSinks.FILE),
                config);

            MessageManager.Instance.Init();

            while (OnGoing)
            {
                string command = Console.ReadLine();

                switch (command)
                {
                    case "start":
                        if (Server.IsOn == false) Server.Instance.StartServer();
                        else Console.WriteLine("Already on going.");
                        break;

                    case "stop":
                        if (Server.IsOn == true) Server.Instance.StopServer();
                        else Console.WriteLine("Not started.");
                        break;
                    case "session_count":
                        if (Server.IsOn == true) Console.WriteLine($"{SessionManager.Instance.GetSessionCount()}");
                        else Console.WriteLine("Not started");
                        break;
                    case "pooled_session_count":
                        if (Server.IsOn == true) Console.WriteLine($"{Server.Instance._networkManager?.serverService?.SessionCurrentPooledCount}");
                        else Console.WriteLine("Not started");
                        break;
                    case "pooled_saea_count":
                        if (Server.IsOn == true) Console.WriteLine($"{Server.Instance._networkManager?.serverService?.SAEACurrentPooledCount}");
                        else Console.WriteLine("Not started");
                        break;
                    case "collect":
                        if (Server.IsOn == true)
                        {
                            GC.Collect();
                            Console.WriteLine("GC.Collect()");
                        }
                        else Console.WriteLine("Not started");
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
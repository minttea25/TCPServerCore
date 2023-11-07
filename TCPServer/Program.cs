using System;
using System.Threading;
using Serilog;
using Serilog.Core;
using ServerCoreTCP.Utils;

namespace ChatServer
{
    class Program
    {
        public static Logger Logger = ServerLogger.CreateServerLogger(LoggerSink.File);
        public static Logger ConsoleLogger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

        public static bool OnGoing = true;

        static void Main(string[] args)
        {
            //Server.Instance.StartServer();
            //int coreCount = Environment.ProcessorCount;
            //ThreadPool.SetMinThreads(1, 1);
            //ThreadPool.SetMaxThreads(coreCount, coreCount);

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
                    default:
                        Console.WriteLine($"Unknown Command: {command}");
                        break;
                }
            }

            Logger.Dispose();
            ConsoleLogger.Dispose();
        }
    }
}
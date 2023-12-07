using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
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
            var config = LoggerConfig.GetDefault();
            config.RestrictedMinimumLevel = Serilog.Events.LogEventLevel.Error;
            CoreLogger.CreateLoggerWithFlag(
                (uint)(CoreLogger.LoggerSinks.CONSOLE | CoreLogger.LoggerSinks.FILE),
                config);


            while (OnGoing)
            {
                string command = Console.ReadLine();

                switch (command)
                {
                    case "start":
                        break;

                    case "stop":
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
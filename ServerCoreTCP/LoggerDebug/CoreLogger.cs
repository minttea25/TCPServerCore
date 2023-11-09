using Serilog.Core;
using System;

namespace ServerCoreTCP.LoggerDebug
{
    /// <summary>
    /// Static Class for Logging of the ServerCoreTCP.
    /// </summary>
    public static class CoreLogger
    {
#if DEBUG
        public static Logger Logger => _logger;

        static bool _logging = false;
        static Logger _logger;

        /// <summary>
        /// The initial value is false; If it becomes true, the logger will be created.
        /// </summary>
        public static bool Logging
        {
            get => _logging;
            set
            {
                if (value == false)
                {
                    Console.WriteLine("Once set to true, it cannot be false again. (The logger instance is already created and logging)");
                    return;
                }

                _logging = value;
                _logger = new LoggerHelper()
                {
                    FlushToDistInterval = TimeSpan.FromSeconds(1),
                    FilePath = LoggerHelper.GetFileName("ServerCoreTCP")
                }.CreateLogger((ushort)(Sinks.CONSOLE));
            }
        }

        public static void StartLogging(Logger logger)
        {
            if (Logging == true)
            {
                Console.WriteLine("The logger is already created and started.");
                return;
            }
            _logger = logger;
        }
#else
        public static Logger Logger { get; set; } = null;
#endif
    }
}
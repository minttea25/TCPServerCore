using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCoreTCP
{
    /// <summary>
    /// Static Class for Logging of the ServerCoreTCP.
    /// </summary>
    public static class CoreLogger
    {
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
                _logger = LoggerFactory.MakeLogger("ServerCoreTCP", Encoding.Unicode, TimeSpan.FromSeconds(1));
            }
        }

        public static Logger Logger => _logger;
    }
}

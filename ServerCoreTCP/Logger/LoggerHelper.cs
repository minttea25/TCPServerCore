using System;
using System.IO;
using System.Text;
using Serilog.Events;

namespace ServerCoreTCP.CLogger
{
    /// <summary>
    /// The default values are from the default values of LoggerConfiguration, and the defualt encoding is UTF8.
    /// </summary>
    public struct LoggerConfig
    {
        public string DirPath;
        public string FileNameFormat;
        public string LogFileExtension;
        public string OutputTemplate;
        public Encoding FileEncoding;
        public TimeSpan? FlushToDistInterval;
        public LogEventLevel RestrictedMinimumLevel;

        public static LoggerConfig GetDefault()
        {
            return new LoggerConfig()
            {
                DirPath = LoggerHelper.TopDirPath_DEFAULT,
                FileNameFormat = LoggerHelper.FileNameFormat_DEFAULT,
                LogFileExtension = LoggerHelper.LogFileExtension_DEFAULT,
                OutputTemplate = LoggerHelper.OutputTemplate_DEFAULT,
                FileEncoding = Encoding.UTF8,
                RestrictedMinimumLevel = LogEventLevel.Verbose,
                FlushToDistInterval = TimeSpan.FromSeconds(1)
            };
        }
    }

    public class LoggerHelper
    {
        public const string TopDirPath_DEFAULT = "Logs";
        public const string FileNameFormat_DEFAULT = "yyyy_MM_dd_HH_mm";
        public const string LogFileExtension_DEFAULT = "log";
        public const string OutputTemplate_DEFAULT = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}";

        /// <summary>
        /// Return a filename based on DateTime.Now.
        /// </summary>
        /// <param name="dirPath">The root directory of the file.</param>
        /// <param name="extension">The extension of log file. e.g. log, txt, ...</param>
        /// <returns>A filename based on DateTime.Now</returns>
        public static string GetFileName(string dirPath, string extension)
        {
            return $"{dirPath}{Path.DirectorySeparatorChar}{string.Format(DateTime.Now.ToString(FileNameFormat_DEFAULT))}.{extension}";
        }
    }
}

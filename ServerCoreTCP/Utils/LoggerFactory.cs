using Serilog;
using Serilog.Core;
using System;
using System.IO;
using System.Text;
using Serilog.Sinks.SystemConsole.Themes;

namespace ServerCoreTCP.Utils
{
    /// <summary>
    /// Factory class for logging (Seirilog)
    /// </summary>
    public static class LoggerFactory
    {
        const string TopDirPath_DEFAULT = "Logs";
        const string FileNameFormat_DEFAULT = "yyyy_MM_dd_HH_mm";
        const string LogFileExtension_DEFAULT = "log";

        /// <summary>
        /// Return a filename based on DateTime.Now.
        /// </summary>
        /// <param name="fileHeaderName">The header of the filename.</param>
        /// <param name="topDirName">The root directory of the file.</param>
        /// <param name="extension">The extension of log file. e.g. log, txt, ...</param>
        /// <returns>A filename based on DateTime.Now</returns>
        public static string GetFileName(string fileHeaderName, string topDirName = TopDirPath_DEFAULT, string extension = LogFileExtension_DEFAULT)
        {
            return $"{topDirName}{Path.DirectorySeparatorChar}[{fileHeaderName}] {string.Format(DateTime.Now.ToString(FileNameFormat_DEFAULT))}.{extension}";
        }

        /// <summary>
        /// Make Serilog.Core.Logger sinked to File and Console.
        /// </summary>
        /// <param name="fileHeaderName">The header of the log file.</param>
        /// <param name="encoding">File encoding. e.g. Unicode, UTF-8, ...</param>
        /// <param name="flushToDiskInterval">The time interval of flushing to disk.</param>
        /// <param name="topDirName">The root directory of the log file.</param>
        /// <param name="fileExtension">The extension of log file. e.g. log, txt, ...</param>
        /// <returns>Created Logger.</returns>
        public static Logger MakeLogger(string fileHeaderName, Encoding encoding, TimeSpan? flushToDiskInterval, string topDirName = TopDirPath_DEFAULT, string fileExtension = LogFileExtension_DEFAULT)
        {
            string filepath = GetFileName(fileHeaderName, topDirName, fileExtension);

            var config = new LoggerConfiguration();
            config.WriteTo.Async(a =>
            {
                a.File(filepath, encoding: encoding, flushToDiskInterval: flushToDiskInterval);
                a.Console();
            });

            return config.CreateLogger();
        }

        /// <summary>
        /// Make Async Serilog.Core.Logger sinked to File and Console.
        /// </summary>
        /// <param name="fileHeaderName">The header of the log file.</param>
        /// <param name="encoding">File encoding. e.g. Unicode, UTF-8, ...</param>
        /// <param name="flushToDiskInterval">The time interval of flushing to disk.</param>
        /// <param name="topDirName">The root directory of the log file.</param>
        /// <param name="fileExtension">The extension of log file. e.g. log, txt, ...</param>
        /// <returns>Created Async Logger.</returns>
        public static Logger MakeAsyncLogger(string fileHeaderName, Encoding encoding, TimeSpan? flushToDiskInterval, string topDirName = TopDirPath_DEFAULT, string fileExtension = LogFileExtension_DEFAULT)
        {
            string filepath = GetFileName(fileHeaderName, topDirName, fileExtension);

            var config = new LoggerConfiguration();
            config.WriteTo.File(filepath, encoding: encoding, flushToDiskInterval: flushToDiskInterval);
            config.WriteTo.Console();

            return config.CreateLogger();
        }

        /// <summary>
        /// Make Serilog.Core.Logger sinked to File.
        /// </summary>
        /// <param name="fileHeaderName">The header of the log file.</param>
        /// <param name="encoding">File encoding. e.g. Unicode, UTF-8, ...</param>
        /// <param name="flushToDiskInterval">The time interval of flushing to disk.</param>
        /// <param name="topDirName">The root directory of the log file.</param>
        /// <param name="fileExtension">The extension of log file. e.g. log, txt, ...</param>
        /// <returns>Created Logger sinked to File.</returns>
        public static Logger MakeLoggerFile(string fileHeaderName, Encoding encoding, TimeSpan? flushToDiskInterval, string topDirName = TopDirPath_DEFAULT, string fileExtension = LogFileExtension_DEFAULT)
        {
            string filepath = GetFileName(fileHeaderName, topDirName, fileExtension);

            var config = new LoggerConfiguration();
            config.WriteTo.File(filepath, encoding: encoding, flushToDiskInterval: flushToDiskInterval);

            return config.CreateLogger();
        }

        /// <summary>
        /// Make Async Serilog.Core.Logger sinked to File and Console.
        /// </summary>
        /// <param name="fileHeaderName">The header of the log file.</param>
        /// <param name="encoding">File encoding. e.g. Unicode, UTF-8, ...</param>
        /// <param name="flushToDiskInterval">The time interval of flushing to disk.</param>
        /// <param name="topDirName">The root directory of the log file.</param>
        /// <param name="fileExtension">The extension of log file. e.g. log, txt, ...</param>
        /// <returns>Created Async Logger sinked to File.</returns>
        public static Logger MakeLoggerFileAsync(string fileHeaderName, Encoding encoding, TimeSpan? flushToDiskInterval, string topDirName = TopDirPath_DEFAULT, string fileExtension = LogFileExtension_DEFAULT)
        {
            string filepath = GetFileName(fileHeaderName, topDirName, fileExtension);

            var config = new LoggerConfiguration();
            config.WriteTo.Async(a => a.File(filepath, encoding: encoding, flushToDiskInterval: flushToDiskInterval));

            return config.CreateLogger();
        }

        /// <summary>
        /// Make Serilog.Core.Logger sinked to Console.
        /// </summary>
        /// <param name="theme">ConsoleTheme.</param>
        /// <returns>Created Logger sinked to Console.</returns>
        public static Logger MakeLoggerConsole(ConsoleTheme theme = null)
        {
            new LoggerConfiguration().WriteTo.Console(theme: theme);
            return new LoggerConfiguration().WriteTo.Console().CreateLogger();
        }

        /// <summary>
        /// Make Async Serilog.Core.Logger sinked to Console.
        /// </summary>
        /// <param name="theme">ConsoleTheme.</param>
        /// <returns>Created Async Logger sinked to Console.</returns>
        public static Logger MakeLoggerConsoleAsync(ConsoleTheme theme = null)
        {
            return new LoggerConfiguration().WriteTo.Async(a => a.Console(theme: theme)).CreateLogger();
        }
    }
}

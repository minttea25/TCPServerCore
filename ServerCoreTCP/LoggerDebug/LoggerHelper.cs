using System;
using Serilog;
using Serilog.Core;
using System.IO;
using System.Text;

namespace ServerCoreTCP.LoggerDebug
{
#if DEBUG
    internal enum Sinks : ushort
    {
        DEBUG = 1, 
        CONSOLE = 2,
        FILE = 3
    }
#endif

    public class LoggerHelper
    {
        const string TopDirPath_DEFAULT = "Logs";
        const string FileNameFormat_DEFAULT = "yyyy_MM_dd_HH_mm";
        const string LogFileExtension_DEFAULT = "log";
        const string OutputTemplate_DEFAULT = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}";

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

#if DEBUG
        internal string OutputTemplate { get; set; } = OutputTemplate_DEFAULT;

        internal string FilePath { get; set; } = null;
        internal Encoding FileEncoding { get; set; } = Encoding.Unicode;
        internal TimeSpan? FlushToDistInterval { get; set; } = null;

        internal Logger CreateLogger(ushort sinkSet)
        {
            if (FilePath == null) FilePath = GetFileName("DEFAULT");

            LoggerConfiguration config = new LoggerConfiguration();
            if ((sinkSet & (ushort)Sinks.DEBUG) == (ushort)Sinks.DEBUG)
            {
                config.WriteTo.Debug(outputTemplate: OutputTemplate);
            }

            if ((sinkSet & (ushort)Sinks.CONSOLE) == (ushort)Sinks.CONSOLE)
            {
                config.WriteTo.Console(outputTemplate: OutputTemplate);
            }

            if ((sinkSet & (ushort)Sinks.FILE) == (ushort)Sinks.FILE)
            {
                config.WriteTo.File(FilePath, encoding: FileEncoding, outputTemplate: OutputTemplate, flushToDiskInterval: FlushToDistInterval);
            }

            return config.CreateLogger();
        }

        internal Logger CreateLogger(Sinks sinks)
        {
            switch (sinks)
            {
                case Sinks.DEBUG:
                case Sinks.CONSOLE:
                case Sinks.FILE:
                    return CreateLogger((ushort)sinks);
                default:
                    throw new NotImplementedException();
            }
        }

        internal Logger CreateAsyncLogger(ushort sinkSet)
        {
            if (FilePath == null) FilePath = GetFileName("DEFAULT");

            LoggerConfiguration config = new LoggerConfiguration();
            config.WriteTo.Async(c =>
            {
                if ((sinkSet & (ushort)Sinks.DEBUG) == (ushort)Sinks.DEBUG)
                {
                    c.Debug(outputTemplate: OutputTemplate);
                }

                if ((sinkSet & (ushort)Sinks.CONSOLE) == (ushort)Sinks.CONSOLE)
                {
                    c.Console(outputTemplate: OutputTemplate);
                }

                if ((sinkSet & (ushort)Sinks.FILE) == (ushort)Sinks.FILE)
                {
                    c.File(FilePath, encoding: FileEncoding, outputTemplate: OutputTemplate, flushToDiskInterval: FlushToDistInterval);
                }
            });

            return config.CreateLogger();
        }

        internal Logger CreateAsyncLogger(Sinks sinks)
        {
            switch (sinks)
            {
                case Sinks.DEBUG:
                case Sinks.CONSOLE:
                case Sinks.FILE:
                    return CreateAsyncLogger((ushort)sinks);
                default:
                    throw new NotImplementedException();
            }
        }
#endif
    }
}

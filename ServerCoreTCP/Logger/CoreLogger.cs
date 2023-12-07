#nullable enable

using Google.Protobuf;
using Serilog;
using Serilog.Core;
using System;

namespace ServerCoreTCP.CLogger
{
    /// <summary>
    /// Static Class for Logging of the ServerCoreTCP.
    /// </summary>
    public class CoreLogger
    {
        public enum LoggerSinks : uint
        {
            DEBUG = 1,
            CONSOLE = 1 << 1,
            FILE = 1 << 2,
        }

        /// <summary>
        /// Logger can be created with 'CreateLoggerWithFlag' or user's needs in directly.
        /// </summary>
        public static Logger CLogger { get; set; } = null;

        public static void CreateLoggerWithFlag(uint flag, LoggerConfig loggerConfig)
        {
            LoggerConfiguration config = new LoggerConfiguration();

            if ((flag & (uint)LoggerSinks.DEBUG) == (uint)LoggerSinks.DEBUG)
            {
                config.WriteTo.Debug(
                    outputTemplate: loggerConfig.OutputTemplate,
                    restrictedToMinimumLevel: loggerConfig.RestrictedMinimumLevel);
            }

            if ((flag & (uint)LoggerSinks.CONSOLE) == (uint)LoggerSinks.CONSOLE)
            {
                config.WriteTo.Console(
                    outputTemplate: loggerConfig.OutputTemplate,
                    restrictedToMinimumLevel: loggerConfig.RestrictedMinimumLevel);
            }

            if ((flag & (uint)LoggerSinks.FILE) == (uint)LoggerSinks.FILE)
            {
                config.WriteTo.File(
                    path: LoggerHelper.GetFileName(loggerConfig.DirPath, loggerConfig.LogFileExtension),
                    outputTemplate: loggerConfig.OutputTemplate,
                    restrictedToMinimumLevel: loggerConfig.RestrictedMinimumLevel,
                    encoding: loggerConfig.FileEncoding,
                    flushToDiskInterval: loggerConfig.FlushToDistInterval);
            }

            CLogger = config.CreateLogger();
        }

        public static void StopLogging()
        {
            CLogger?.Dispose();
        }

        public static void LogInfo(string header, string messageTemplate, params object?[]? propertyValues)
        {
            string msg = $"[{header}] {messageTemplate}";
            CLogger?.Information(msg, propertyValues);
        }

        public static void LogError(string header, Exception ex, string messageTemplate, params object?[]? propertyValues)
        {
            string msg = $"[{header}] {messageTemplate}";
            CLogger?.Error(ex, msg, propertyValues);
        }

        public static void LogError(string header, string messageTemplate, params object?[]? propertyValues)
        {
            string msg = $"[{header}] {messageTemplate}";
            CLogger?.Error(msg, propertyValues);
        }

        public static void LogDebug(string header, string messageTemplate, params object?[]? propertyValues)
        {
            string msg = $"[{header}] {messageTemplate}";
            CLogger?.Debug(msg, propertyValues);
        }


#if CUSTOM_PACKET
#else
        public static void LogRecv<T>(T message, string header = "Recv") where T : IMessage
        {
            string msg = $"[{header}] [{typeof(T)}] {message}";
            CLogger?.Information(msg);
        }

        public static void LogSend<T>(T message, string header = "Send") where T : IMessage
        {
            string msg = $"[{header}] [{typeof(T)}] {message}";
            CLogger?.Information(msg);
        }
    }
#endif
}
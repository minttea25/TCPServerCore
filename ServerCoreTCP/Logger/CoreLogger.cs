#nullable enable

using Serilog;
using Serilog.Core;
using System;
using System.Runtime.CompilerServices;

namespace NetCore.CLogger
{
    /// <summary>
    /// A class for logging in NetCore.
    /// </summary>
    public class CoreLogger
    {
        public enum LoggerSinks : uint
        {
            CONSOLE = 1,
            FILE = 1 << 1,
        }

        /// <summary>
        /// Logger can be created with 'CreateLoggerWithFlag' or user's needs in directly.
        /// </summary>
        public static Logger? CLogger { get; set; } = null;

        
        public static void CreateLoggerWithFlag(uint flag, LoggerConfig loggerConfig)
        {
            LoggerConfiguration config = new LoggerConfiguration();

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

        public static void DisposeLogging()
        {
            CLogger?.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogInfo(string header, string messageTemplate, params object?[]? propertyValues)
        {
            //string msg = $"[{header}] {messageTemplate}";
            CLogger?.Information(GetLogFormat(header, messageTemplate), propertyValues);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogError(string header, Exception ex, string messageTemplate, params object?[]? propertyValues)
        {
            //string msg = $"[{header}] {messageTemplate}";
            CLogger?.Error(ex, GetLogFormat(header, messageTemplate), propertyValues);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogError(string header, string messageTemplate, params object?[]? propertyValues)
        {
            //string msg = $"[{header}] {messageTemplate}";
            CLogger?.Error(GetLogFormat(header, messageTemplate), propertyValues);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogDebug(string header, string messageTemplate, params object?[]? propertyValues)
        {
            //string msg = $"[{header}] {messageTemplate}";
            CLogger?.Debug(GetLogFormat(header, messageTemplate), propertyValues);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static string GetLogFormat(string header, string messageTemplate)
        {
            return $"[{header}] {messageTemplate}";
        }


#if PROTOBUF
        public static void LogRecv<T>(T message, string header = "Recv") where T : Google.Protobuf.IMessage
        {
            string msg = $"[{header}] [{typeof(T)}] {message}";
            CLogger?.Information(msg);
        }

        public static void LogSend<T>(T message, string header = "Send") where T : Google.Protobuf.IMessage
        {
            string msg = $"[{header}] [{typeof(T)}] {message}";
            CLogger?.Information(msg);
        }
#endif
    }
}
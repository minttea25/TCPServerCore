using Serilog;
using Serilog.Core;
using ServerCoreTCP.LoggerDebug;
using System;
using System.Text;

using Google.Protobuf;
using ServerCoreTCP;

namespace ChatServer
{
    public enum LoggerSink
    {
        Console = 0,
        File = 1,
        DB = 2,
    }

    public class ServerLogger
    {
        public static Logger CreateServerLogger(LoggerSink sink, bool asyncLogging = false)
        {
            LoggerConfiguration config = new LoggerConfiguration();

            if (asyncLogging == false)
            {
                AddSink(sink, config);
            }
            else
            {
                AddAsyncSink(sink, config);
            }
            return config.CreateLogger();
        }
        static void AddSink(LoggerSink sink, LoggerConfiguration config)
        {
            switch (sink)
            {
                case LoggerSink.Console:
                    config.WriteTo.Console();
                    break;
                case LoggerSink.File:
                    config.WriteTo.File(LoggerHelper.GetFileName("[ChatServer]"), flushToDiskInterval: TimeSpan.FromSeconds(1), encoding: Encoding.Unicode);
                    break;
                case LoggerSink.DB:
                    throw new NotImplementedException();
                    break;
            }
        }
        static void AddAsyncSink(LoggerSink sink, LoggerConfiguration config)
        {
            switch (sink)
            {
                case LoggerSink.Console:
                    config.WriteTo.Async(c => c.Console());
                    break;
                case LoggerSink.File:
                    config.WriteTo.Async(c => c.File(LoggerHelper.GetFileName("[ChatServer]"), flushToDiskInterval: TimeSpan.FromSeconds(1), encoding: Encoding.Unicode));
                    break;
                case LoggerSink.DB:
                    throw new NotImplementedException();
                    break;
            }
        }


        public static void Send<T>(ClientSession s, T message) where T : IMessage
        {
            var t = typeof(T);
            //Program.Logger.Information("[Sent] [{t}] {message} to {ConnectedEndPoint}", t, message, s.ConnectedEndPoint);
            Program.ConsoleLogger.Information("[Sent] [{t}] {message} to {ConnectedEndPoint}", t, message, s.ConnectedEndPoint);
            s.Send(message);
        }
    }
}
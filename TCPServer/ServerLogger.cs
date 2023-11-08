using Serilog;
using Serilog.Core;
using ServerCoreTCP.CLogger;
using System;
using System.Text;

using Google.Protobuf;
using ServerCoreTCP;

namespace ChatServer
{
    public class ServerLogger
    {
        public static void Send<T>(ClientSession s, T message) where T : IMessage
        {
            CoreLogger.LogInfo("Send", "[{0}] {1} to {2}", typeof(T), message, s.ConnectedEndPoint);
            s.Send(message);
        }
    }
}
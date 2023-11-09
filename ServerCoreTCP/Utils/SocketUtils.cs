using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using ServerCoreTCP;
using ServerCoreTCP.CLogger;

namespace ServerCoreTCP.Utils
{
    static class SocketUtils
    {
        public static void SetLinger(Socket socket, int lingerSeconds)
        {
            if (lingerSeconds <= 0) socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
            else socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, new LingerOption(true, lingerSeconds));
        }
        public static void SetNoDelay(Socket socket, bool noDelay)
        {
            socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, noDelay);
        }

        public static void SetKeepAlive(Socket socket, bool keepAlive)
        {
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, keepAlive);
        }

        public static void SetReuseAddress(Socket socket, bool reuseAddress)
        {
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, reuseAddress);
        }

        // low-level api
        public static void SetKeepAlive(Socket socket, int keepAliveTime, int keepAliveInterval)
        {
            try
            {
                // set keepalive true on socket level
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

                // set keepalive true on tcp level
                socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.KeepAlive, BitConverter.GetBytes(1));

                byte[] keepAliveTimeValues = BitConverter.GetBytes(keepAliveTime);
                byte[] keepAliveIntervalValues = BitConverter.GetBytes(keepAliveInterval);
                socket.IOControl(IOControlCode.KeepAliveValues, keepAliveTimeValues, keepAliveIntervalValues);
            }
            catch(Exception ex)
            {
                CoreLogger.LogError("SocketUtils.SetKeepAlive", ex, "Exception");
            }
        }
    }
}

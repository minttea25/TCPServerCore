using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace ServerCoreTCP.Utils
{
    static class Extends
    {
        public static void SetLinger(this Socket socket, int lingerSeconds)
        {
            SocketUtils.SetLinger(socket, lingerSeconds);
        }

        public static void SetNoDelay(this Socket socket, bool noDelay)
        {
            SocketUtils.SetNoDelay(socket, noDelay);
        }

        public static void SetKeepAlive(this Socket socket, bool keepAlive)
        {
            SocketUtils.SetKeepAlive(socket, keepAlive);
        }

        public static void SetReuseAddress(this Socket socket, bool reuseAddress)
        {
            SocketUtils.SetReuseAddress(socket, reuseAddress);
        }

        public static void SetKeepAlive(this Socket socket, int keepAliveTime, int keepAliveInterval)
        {
            SocketUtils.SetKeepAlive(socket, keepAliveTime, keepAliveInterval);
        }
    }
}

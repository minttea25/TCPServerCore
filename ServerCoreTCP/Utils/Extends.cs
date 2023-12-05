using System.Net.Sockets;

namespace ServerCoreTCP.Utils
{
    static class Extends
    {
#region Socket Util Extends
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
#endregion
    }
}

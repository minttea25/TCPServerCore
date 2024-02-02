using System.Net.Sockets;

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

        public static void SetReuseAddress(Socket socket, bool reuseAddress)
        {
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, reuseAddress);
        }
    }
}

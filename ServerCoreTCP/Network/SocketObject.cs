using System.Net.Sockets;

namespace ServerCoreTCP
{
    public abstract class SocketObject
    {
        internal Service m_service = null;
        internal abstract void Dispatch(object sender, SocketAsyncEventArgs eventArgs);
    }
}

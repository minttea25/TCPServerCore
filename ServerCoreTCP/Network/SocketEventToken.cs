using System.Net.Sockets;

namespace ServerCoreTCP
{
    internal abstract class SocketEventToken
    {
        internal enum SocketEventType : ushort
        {
            Connect, Disconnect, Accept, Recv, Send
        }

        internal SocketEventType EventType => m_eventType;
        protected SocketEventType m_eventType;

        internal SocketObject m_socketObject;

        internal SocketEventToken(SocketEventType eventType, SocketObject socketObject)
        {
            m_eventType = eventType;
            m_socketObject = socketObject;
        }

        internal void SetSocketObject(SocketObject socketObject)
        {
            m_socketObject = socketObject;
        }
    }

    internal class ConnectEventToken : SocketEventToken
    {
        internal Socket m_socket;
        internal Session m_session;
        internal ConnectEventToken(Connector connector, Socket socket, Session session) : base(SocketEventType.Connect, connector)
        {
            m_socket = socket;
            m_session = session;
        }
    }

    internal class AcceptEventToken : SocketEventToken
    {
        internal AcceptEventToken(Listener listener) : base(SocketEventType.Accept, listener)
        {

        }
    }

    internal class RecvEventToken : SocketEventToken
    {
        internal RecvEventToken(Session session) : base(SocketEventType.Recv, session)
        {

        }
    }

    internal class SendEventToken : SocketEventToken
    {
        internal SendEventToken(Session session) : base(SocketEventType.Send, session)
        {

        }
    }
}

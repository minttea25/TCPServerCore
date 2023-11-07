using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCoreTCP
{
    internal abstract class SocketEventToken
    {
        public enum SocketEventType : ushort
        {
            Connect, Disconnect, Accept, Recv, Send
        }

        public SocketEventType EventType => m_eventType;
        protected SocketEventType m_eventType;

        internal SocketObject m_socketObject;

        public SocketEventToken(SocketEventType eventType, SocketObject socketObject)
        {
            m_eventType = eventType;
            m_socketObject = socketObject;
        }

        public void SetSocketObject(SocketObject socketObject)
        {
            m_socketObject = socketObject;
        }
    }

    internal class ConnectEventToken : SocketEventToken
    {
        internal Socket m_socket;
        internal Session m_session;
        public ConnectEventToken(Connector connector, Socket socket, Session session) : base(SocketEventType.Connect, connector)
        {
            m_socket = socket;
            m_session = session;
        }
    }

    internal class AcceptEventToken : SocketEventToken
    {
        public AcceptEventToken(Listener listener) : base(SocketEventType.Accept, listener)
        {

        }
    }

    internal class RecvEventToken : SocketEventToken
    {
        public RecvEventToken(Session session) : base(SocketEventType.Recv, session)
        {

        }
    }

    internal class SendEventToken : SocketEventToken
    {
        public SendEventToken(Session session) : base(SocketEventType.Send, session)
        {

        }
    }
}

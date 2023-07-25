using System.Collections.Generic;

using ServerCoreTCP;

namespace TestClient
{
    public class SessionManager
    {
        #region Singleton
        readonly static SessionManager _instance = new();
        public static SessionManager Instance => _instance;
        #endregion

        readonly List<ServerSession> _sessions = new();
        readonly object _lock = new();

        public ServerSession CreateNewSession()
        {
            ServerSession session = new();
            lock (_lock)
            {
                _sessions.Add(session);
            }
            return session;
        }

        public void Broadcast(IPacket packet)
        {
            lock (_lock)
            {
                foreach (ServerSession session in _sessions)
                {
                    session.Send(packet);
                }
            }
        }
    }
}

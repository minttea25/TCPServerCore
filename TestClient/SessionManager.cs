using System.Collections.Generic;

using Google.Protobuf;

namespace TestClient
{
    public class SessionManager
    {
        #region Singleton
        readonly static SessionManager _instance = new SessionManager();
        public static SessionManager Instance => _instance;
        #endregion

        readonly List<ServerSession> _sessions = new List<ServerSession>();
        readonly object _lock = new object();

        public ServerSession CreateNewSession()
        {
            ServerSession session = new ServerSession();
            lock (_lock)
            {
                _sessions.Add(session);
            }
            return session;
        }
    }
}

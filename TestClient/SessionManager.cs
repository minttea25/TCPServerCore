using System.Collections.Generic;
using Chat;
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

        public void ExitAll()
        {
            lock (_lock)
            {
                foreach (var s in _sessions)
                {
                    SLeaveRoom req = new()
                    {
                        UserInfo = s.userInfo,
                        RoomId = s.enteredRoomNo
                    };

                    s.Send_(req);
                    s.Disconnect();
                }

                _sessions.Clear();
            }
        }
    }
}
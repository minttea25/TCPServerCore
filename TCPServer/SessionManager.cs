using System;
using System.Collections.Generic;

namespace TCPServer
{
    public class SessionManager
    {
        #region Singleton
        readonly static SessionManager _instance = new();
        public static SessionManager Instance => _instance;
        #endregion

        /// <summary>
        /// The identifier of sessions. It starts at 1. (0 is invalid id) 
        /// </summary>
        uint _sessionId = 0;
        readonly Dictionary<uint, ClientSession> _sessions = new();
        readonly object _lock = new();

        public ClientSession CreateNewSession()
        {
            lock (_lock)
            {
                uint id = _sessionId++;
                ClientSession session = new(id);

                Console.WriteLine($"Connected as id={id}");
                _sessions.Add(id, session);

                return session;
            }
        }

        public ClientSession Get(uint id)
        {
            lock (_lock)
            {
                if (_sessions.TryGetValue(id, out var session))
                {
                    return session;
                }
                else return null;
            }
        }

        public bool Remove(uint id)
        {
            lock (_lock)
            {
                return _sessions.Remove(id);
            }
        }

        public bool Remove(ClientSession session)
        {
            lock (_lock)
            {
                return _sessions.Remove(session.SessionId);
            }
        }
    }
}

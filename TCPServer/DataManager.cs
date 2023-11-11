using ChatServer.Data;
using System;
using System.Collections.Generic;
using System.Threading;

namespace ChatServer
{
    public class DataManager
    {
        #region Singleton
        static DataManager _instance = new DataManager();
        public static DataManager Instance => _instance;
        #endregion

        public Dictionary<ushort, User> Users => _users;

        Dictionary<ushort, User> _users = new();
        int _nextUid = 0;

        readonly object _lock = new object();

        DataManager()
        {
            // load data
            //LoadData();
        }

        void LoadData()
        {

        }

        void WriteData(User user, ushort id)
        {

        }

        public User AddNewUser(string name)
        {
            if (UsableName(name) == false) return null;

            ushort id = (ushort)Interlocked.Increment(ref _nextUid);
            User u = new User(id, name);

            WriteData(u, id);

            return u;
        }

        public bool UsableName(string name)
        {
            lock (_lock)
            {
                foreach (var u in Users.Values)
                {
                    if (u.UserName == name) return false;
                }
                return true;
            }
        }
    }
}
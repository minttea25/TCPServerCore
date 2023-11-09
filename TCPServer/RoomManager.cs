using ServerCoreTCP.Utils;
using System;
using System.Collections.Generic;
using System.Threading;

namespace ChatServer
{
    class RoomManager
    {
        const int FlushTickInterval = 10;

        #region Singleton
        readonly static RoomManager _instance = new RoomManager();
        public static RoomManager Instance => _instance;
        #endregion

        Dictionary<uint, Room> _rooms = new();

        static ReaderWriterLockSlim rwLock = new();

        public void FlushRoom()
        {
            rwLock.EnterReadLock();
            foreach (var room in _rooms.Values)
            {
                if (room.IsActivate == true)
                {
                    room.AddJob(() => room.Flush());
                }
            }
            rwLock.ExitReadLock();

            JobTimer.Instance.Push(FlushRoom, FlushTickInterval);
        }

        public void CreateNewRoom(uint roomId, out Room room)
        {
            rwLock.EnterWriteLock();
            if (_rooms.ContainsKey(roomId) == false)
            {
                room = new Room(roomId);
                _rooms.Add(roomId, room);
            }
            else room = _rooms[roomId];
            rwLock.ExitWriteLock();
        }

        public bool Exist(uint roomId, out Room room)
        {
            rwLock.EnterReadLock();
            bool ret = _rooms.ContainsKey(roomId);
            if (ret) room = _rooms[roomId];
            else room = null;
            rwLock.ExitReadLock();
            return ret;
        }

        public bool TryRemoveRoom(uint roomId)
        {
            rwLock.EnterWriteLock();
            bool ret = _rooms.Remove(roomId);
            rwLock.ExitWriteLock();
            return ret;
        }
    }
}
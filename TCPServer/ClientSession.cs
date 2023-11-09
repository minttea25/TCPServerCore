using Chat;
using ChatServer.Data;
using ServerCoreTCP.CLogger;
using ServerCoreTCP.MessageWrapper;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace ChatServer
{
    public class ClientSession : PacketSession
    {
        public uint ConnectedId = 0;
        public User User => _user;

        User _user;
        Dictionary<uint, Room> _rooms = new();

        static ReaderWriterLockSlim rwLock = new();

        public ClientSession()
        {
            ;
        }

        ~ClientSession()
        {
            _user = null;
            _rooms = null;
        }

        public void SetUser(User user)
        {
            _user = user;
        }

        public void SetConnectedId(uint id)
        {
            ConnectedId = id;
        }

        public void EnterRoom(Room room)
        {
            rwLock.EnterWriteLock();
            _rooms.Add(room.RoomNo, room);
            rwLock.ExitWriteLock();
        }

        public void LeaveRoom(uint roomId)
        {
            rwLock.EnterWriteLock();
            _ = _rooms.Remove(roomId);
            rwLock.ExitWriteLock();
        }

        public void LeaveAllRooms()
        {
            rwLock.EnterWriteLock();
            foreach (var r in _rooms.Values)
            {
                r.Leave(this);
            }
            _rooms.Clear();
            rwLock.ExitWriteLock();
        }

        public override void OnConnected(EndPoint endPoint)
        {
            CoreLogger.LogInfo("ClientSession", "[sid={0}] OnConnected: {1}", ConnectedId, endPoint);
        }

        public override void OnDisconnected(EndPoint endPoint, object error = null)
        {
            _ = SessionManager.Instance.ClearSession(this);

            CoreLogger.LogInfo("ClientSession", "[sid={0}] OnDisconnected: {1}", ConnectedId, endPoint);
        }

        public override void OnRecv(ReadOnlySpan<byte> buffer)
        {
            MessageManager.Instance.OnRecvPacket(this, buffer);
        }

        public override void OnSend(int numOfBytes)
        {
        }

        public override void InitSession()
        {
            SessionManager.Instance.AddNewSession(this);
        }

        public override void PreSessionCleanup()
        {
        }
    }
}
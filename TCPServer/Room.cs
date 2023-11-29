using Google.Protobuf;
using ServerCoreTCP.Utils;
using ServerCoreTCP.MessageWrapper;
using System;
using System.Collections.Generic;

using Chat;
using Google.Protobuf.WellKnownTypes;
using ServerCoreTCP.CLogger;
using ServerCoreTCP.Job;

namespace ChatServer
{
    public class Room
    {
        public bool IsActivate => _pendingMessages.Count != 0;
        public uint RoomNo => _roomNo;
        readonly uint _roomNo;

        Dictionary<uint, ClientSession> _users = new();
        JobSerializer _jobs = new JobSerializer();
        List<ArraySegment<byte>> _pendingMessages = new List<ArraySegment<byte>>();

        readonly object _roomLock = new();
        readonly object _sessionLock = new();
        readonly object _pendingQueueLock = new();

        public Room(uint roomId)
        {
            _roomNo = roomId;
        }

        ~Room()
        {
            _users = null;
            _jobs = null;
            _pendingMessages = null;
        }

        public IReadOnlyList<uint> GetUsers()
        {
            lock (_roomLock)
            {
                return new List<uint>(_users.Keys);
            }
        }

        public void AddJob(Action job)
        {
            _jobs.Add(job);
        }

        public void Flush()
        {
            lock (_pendingQueueLock)
            {
                foreach (ClientSession session in _users.Values)
                {
                    session.SendRaw(_pendingMessages);
                }
                _pendingMessages.Clear();
            }
        }

        public bool Enter(ClientSession session)
        {
            if (_users.ContainsKey(session.User.Id)) return false;

            lock (_sessionLock)
            {
                _users.Add(session.User.Id, session);
            }

            session.EnterRoom(this);

            return true;
        }

        public bool Leave(ClientSession session)
        {
            lock (_sessionLock)
            {
                bool ret = _users.Remove(session.User.Id);

                if (_users.Count == 0)
                {
                    if (RoomManager.Instance.TryRemoveRoom(RoomNo))
                    {
                        CoreLogger.LogInfo("Room", "The room [id={0}] is removed", RoomNo);
                    }
                }

                return ret;
            }

            CLeaveRoom msg = new()
            {
                RoomId = RoomNo,
                UserInfo = session.User.UserInfo
            };
            BroadCast(msg);
        }

        public void SendChatText(ClientSession session, string msg)
        {
            ChatBase chat = new();
            chat.Timestamp = Timestamp.FromDateTime(DateTime.UtcNow.AddHours(9));
            chat.ChatType = ChatType.Text;

            CRecvChatText res = new()
            {
                RoomId = RoomNo,
                SenderInfo = session.User.UserInfo,
                ChatBase = chat,
                Msg = msg,
            };
            BroadCast(res);
        }

        public void SendChatIcon(ClientSession session, uint iconId)
        {
            ChatBase chat = new();
            chat.Timestamp = Timestamp.FromDateTime(DateTime.UtcNow.AddHours(9));
            chat.ChatType = ChatType.Icon;

            CRecvChatIcon res = new()
            {
                RoomId = RoomNo,
                SenderInfo = session.User.UserInfo,
                ChatBase = chat,
                IconId = iconId
            };
            BroadCast(res);
        }

        public void BroadCast(ArraySegment<byte> buffer)
        {
            lock (_pendingQueueLock)
            {
                _pendingMessages.Add(buffer);
            }
        }

        public void BroadCast<T>(T message) where T : IMessage
        {
            var type = typeof(T);

            //Program.ConsoleLogger.Information("[BroadCast] {type} {message}", type, message);
            lock (_pendingQueueLock)
            {
                _pendingMessages.Add(message.SerializeWrapper());
            }
        }
    }
}
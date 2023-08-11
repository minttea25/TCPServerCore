﻿using Google.Protobuf;
using ServerCoreTCP;
using ServerCoreTCP.Utils;
using System;
using System.Collections.Generic;
using System.Threading;

using ChatTest;
using ServerCoreTCP.Protobuf;
using System.Collections.Concurrent;

namespace TCPServer
{
    public class Room
    {
        public readonly Dictionary<uint, string> Users = new();
        public uint RoomNo => _roomNo;
        readonly uint _roomNo;

        readonly List<ClientSession> _sessions = new();
        readonly JobQueue _jobs = new();
        readonly List<Memory<byte>> _pendingMessages = new();

        public Room(uint id)
        {
            _roomNo = id;
        }

        public void AddJob(Action job)
        {
            _jobs.Add(job);
        }

        public void Flush()
        {
            foreach (ClientSession session in _sessions)
            {
                session.SendRaw(_pendingMessages);
            }

            _pendingMessages.Clear();
        }

        public void Enter(ClientSession session)
        {
            _sessions.Add(session);
            session.Room = this;
            Users.Add(session.SessionId, session.UserName);

            C_EnterRoom pkt = new()
            {
                UserId = session.SessionId,
                UserName = session.UserName,
            };

            BroadCast(pkt);
        }

        public void Leave(ClientSession session)
        {
            C_LeaveRoom send = new()
            {
                UserName = session.UserName
            };
            Console.WriteLine(send);

            _sessions.Remove(session);
            Users.Remove(session.SessionId);

            session.Room.BroadCast(send);
            session.Room = null;
        }

        public void SendChat(ClientSession session, string msg)
        {
            C_Chat msgPkt = new()
            {
                UserId = session.SessionId,
                Msg = msg,
                UserName = Users[session.SessionId]
            };

            Console.WriteLine(msgPkt);

            BroadCast(msgPkt);
        }

        public void BroadCast(Memory<byte> buffer)
        {
            _pendingMessages.Add(buffer);
        }

        public void BroadCast<T>(T message) where T : IMessage
        {
            _pendingMessages.Add(message.MSerializeProtobuf());
        }
    }
}
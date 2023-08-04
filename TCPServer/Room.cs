using Google.Protobuf;
using ServerCoreTCP;
using ServerCoreTCP.ProtobufWrapper;
using ServerCoreTCP.Utils;
using System;
using System.Collections.Generic;
using System.Threading;

namespace TCPServer
{
    public class Room
    {
        readonly static Random rand = new();

        readonly List<ClientSession> _sessions = new();
        readonly JobQueue _jobs = new();
        List<Memory<byte>> _pendingMessages = new();

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

            // create a new point
            Vector3 loc = new()
            {
                X = rand.Next(-0, 10),
                Y = rand.Next(-0, 10),
                Z = rand.Next(-0, 10),
            };

            // send the position to connected session
            //session.Send(loc);

            // send the position of the connected session to all
            BroadCast(loc);
        }

        public void BroadCast(Memory<byte> buffer)
        {
            _pendingMessages.Add(buffer);
        }

        public void BroadCast<T>(T message) where T : IMessage
        {
            _pendingMessages.Add(PacketWrapper.MSerialize(message));
        }
    }
}

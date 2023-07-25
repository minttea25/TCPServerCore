﻿using System;
using System.Net;

using ServerCoreTCP;
using TestNamespace;

namespace TCPServer
{
    public class ClientSession : Session
    {
        public readonly uint SessionId;

        public ClientSession(uint sessionId)
        {
            SessionId = sessionId;
        }

        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine("OnConnected: {0}", endPoint);
        }

        public override void OnDisconnected(EndPoint endPoint, object error = null)
        {
            Console.WriteLine("OnDisconnected: {0}", endPoint);
        }

        public override void OnRecv(ArraySegment<byte> buffer)
        {
            PacketManager.Instance.OnRecvPacket(this, buffer);
        }

        public override void OnRecv(Memory<byte> buffer)
        {
            //PacketManager.Instance.OnRecvPacket(this, buffer);
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine("Sent: {0} bytes", numOfBytes);
        }
    }
}
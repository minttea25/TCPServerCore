using System;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using ServerCoreTCP.CLogger;
using ServerCoreTCP.Core;
using ServerCoreTCP.Utils;

namespace ServerCoreTCP.MessageWrapper
{
    public abstract class PacketSession : Session
    {
        public const int MinimumPacketLength = Defines.PACKET_HEADER_SIZE + Defines.PACKET_DATATYPE_SIZE;

        List<ArraySegment<byte>> _reserveQueue = new List<ArraySegment<byte>>();
        object _queueLock = new object();

        int _reservedSendBytes = 0;
        long _lastSendTick = 0;


        /// <summary>
        /// Send message to endpoint of the socket [Protobuf Wrapper]
        /// </summary>
        /// <typeparam name="T">Google.Protobuf.IMessage</typeparam>
        /// <param name="packet">The message to send.</param>
        //public void Send<T>(T message) where T : IMessage
        //{
        //    SendRaw(message.SerializeWrapper());
        //}

        public void Send<T>(T message) where T : IMessage
        {
            ArraySegment<byte> msg = message.SerializeWrapper();

            if (msg == null) return;

            lock (_queueLock)
            {
                _reserveQueue.Add(msg);
                _reservedSendBytes += msg.Count;
            }
        }

        public void FlushSend()
        {
            // capture and copy the elements in the queue
            List<ArraySegment<byte>> sendList = null;

            var dt = Global.G_Stopwatch.ElapsedTicks - _lastSendTick;
            if (dt < Defines.SessionSendFlushMinIntervalMilliseconds
                && _reservedSendBytes < Defines.SessionSendFlushMinReservedByteLength) return;
            int b;
            lock (_queueLock)
            {
                if (_reserveQueue.Count == 0) return;

                // send list
                _lastSendTick = Global.G_Stopwatch.ElapsedTicks;
                b = _reservedSendBytes;
                _reservedSendBytes = 0;

                sendList = _reserveQueue;
                _reserveQueue = new List<ArraySegment<byte>>();
            }

            SendRaw(sendList);
            Console.WriteLine($"Sent: {sendList.Count} and {b} bytes");
        }

        protected sealed override int OnRecvProcess(ArraySegment<byte> buffer)
        {
            if (buffer.Count < MinimumPacketLength) return 0;

            ReadOnlySpan<byte> buf = buffer;

            int processed = 0;

            while (processed < buffer.Count)
            {
                // contains processed += sizeof(ushort)
                ushort bodySize = buf.ToUInt16(ref processed); // parsing header (packet size)

                if (bodySize + processed  > buffer.Count) break;

                // The data should be [packet type(2 or 4)][message].
                ReadOnlySpan<byte> data = buffer.Slice(processed, bodySize);
                processed += bodySize;

                OnRecv(data);
            }

            return processed;
        }
    }
}
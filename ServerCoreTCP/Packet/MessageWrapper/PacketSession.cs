using Google.Protobuf;
using ServerCoreTCP.CLogger;
using ServerCoreTCP.Core;
using ServerCoreTCP.Utils;
using System;
using System.Collections.Generic;

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
        /// Serialize the message and add the serialized message in the pending queue of the packet session.
        /// (Note: It does not send the message right now.)
        /// If serialization is failed, do nothing.
        /// </summary>
        /// <typeparam name="T">Google.Protobuf.IMessage</typeparam>
        /// <param name="message">The message data</param>
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

        /// <summary>
        /// Flushes serialized data in the pending queue according to specified conditions.
        /// It contains to send data actually.
        /// </summary>
        public void FlushSend()
        {
            // capture and copy the elements in the queue
            List<ArraySegment<byte>> sendList = null;

            long dt = Global.G_Stopwatch.ElapsedTicks - _lastSendTick;
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
#if DEBUG
            Console.WriteLine($"Sent: {sendList.Count} and {b} bytes");
#endif
        }

        /// <summary>
        /// Slices the buffer that contains multiple data and calls OnRecv of the session with sliced data.
        /// </summary>
        /// <param name="buffer">The buffer that the socket received at once</param>
        /// <returns>The analyzed total length of the data</returns>
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
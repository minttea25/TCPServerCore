#if FLATBUFFERS
using ServerCoreTCP.Core;
using ServerCoreTCP.Utils;
using System;
using System.Collections.Generic;

#if PROTOBUF
using Google.Protobuf;
using ServerCoreTCP.Protobuf;
#endif

#if FLATBUFFERS
using Google.FlatBuffers;
using ServerCoreTCP.Flatbuffers;
#endif

namespace ServerCoreTCP
{
    public abstract class PacketSession : Session
    {
        List<ArraySegment<byte>> _reserveQueue = new List<ArraySegment<byte>>();
        readonly object _queueLock = new object();

        int _reservedSendBytes = 0;
        long _lastSendTick = 0;

#if PROTOBUF
        /// <summary>
        /// Serialize the message and add the serialized message in the pending queue of the packet session.
        /// <br/>If serialization is failed, do nothing.
        /// <br/>Note: It does not send the message directly.
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
        /// Slices the buffer that contains multiple data and calls OnRecv of the session with sliced data.
        /// </summary>
        /// <param name="buffer">The buffer that the socket received at once</param>
        /// <returns>The analyzed total length of the data</returns>
        protected sealed override int OnRecvProcess(ArraySegment<byte> buffer)
        {
            if (buffer.Count < Defines.PACKET_HEADER_SIZE) return 0;

            ReadOnlySpan<byte> buf = buffer;

            int processed = 0;

            while (processed < buffer.Count)
            {
                // contains processed += sizeof(ushort)
                ushort bodySize = buf.ToUInt16(ref processed); // parsing header (packet size)

                if (bodySize + processed  > buffer.Count) break;

                // The data should be [packet type(2][message].
                ReadOnlySpan<byte> data = buffer.Slice(processed, bodySize);
                processed += bodySize;

                OnRecv(data);
            }

            return processed;
        }
#endif

#if FLATBUFFERS
        public void Send(FlatBufferBuilder fb, ushort id)
        {
            Send(PacketWrapper.Serialize(fb, id));
        }

        public void Send(ArraySegment<byte> data)
        {
            if (data == null) return;

            lock (_queueLock)
            {
                _reserveQueue.Add(data);
                _reservedSendBytes += data.Count;
            }
        }

        /// <summary>
        /// Slices the buffer that contains multiple data and calls OnRecv of the session with sliced data.
        /// </summary>
        /// <param name="buffer">The buffer that the socket received at once</param>
        /// <returns>The analyzed total length of the data</returns>
        protected sealed override int OnRecvProcess(ArraySegment<byte> buffer)
        {
            if (buffer.Count < Defines.PACKET_HEADER_SIZE) return 0;

            int processed = 0;

            while (processed < buffer.Count)
            {
                // bodysize =  sizeof(packet header) + serialized data of flatbuffers
                ushort packetSize = (ushort)(buffer[processed] | (buffer[processed + 1] << 8));
                if (processed + packetSize > buffer.Count) break;

                processed += Defines.PACKET_SIZETYPE_SIZE;

                var validLen = packetSize - Defines.PACKET_SIZETYPE_SIZE;
                // buffer except size data
                OnRecv(buffer, processed, validLen);

                processed += validLen;
            }

            return processed;
        }
#endif
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

        
    }
}
#endif
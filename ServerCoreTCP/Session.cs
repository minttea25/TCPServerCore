#define MEMORY_BUFFER


// Use one of 3.
//#define PROTOBUF
#define PROTOBUF_WRAPPER
//#define CUSTOM_PACKET

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;

using Google.Protobuf;
using ServerCoreTCP.ProtobufWrapper;
using ServerCoreTCP.CustomBuffer;

namespace ServerCoreTCP
{
    public abstract class Session
    {
        const int RecvBufferSize = 1024;

#if PROTOBUF
        const int HeaderSize = sizeof(uint);
#else
        const int HeaderSize = sizeof(ushort);
#endif

        public EndPoint EndPoint
        {
            get
            {
                return _socket?.RemoteEndPoint;
            }
        }

        Socket _socket;
#if MEMORY_BUFFER
        readonly MRecvBuffer _recvBuffer = new(RecvBufferSize);
        readonly Queue<Memory<byte>> _sendQueue = new();
        readonly List<Memory<byte>> _sendPendingList = new();
        readonly List<ArraySegment<byte>> _sendBufferList = new();
#else
        readonly RecvBuffer _recvBuffer = new(RecvBufferSize);
        readonly Queue<ArraySegment<byte>> _sendQueue = new();
        readonly List<ArraySegment<byte>> _sendPendingList = new();
#endif

        /// <summary>
        /// Each session reuses one SendEventArgs.
        /// </summary>
        readonly SocketAsyncEventArgs _sendEvent = new();
        /// <summary>
        /// Each session reuses one RecvEventArgs.
        /// </summary>
        readonly SocketAsyncEventArgs _recvEvent = new();

        /// <summary>
        /// Note: Send/Recv can be occurred in multiple threads
        /// </summary>
        readonly object _lock = new();

#region Abstract Methods
        /// <summary>
        /// Called when the socket is connected.
        /// </summary>
        /// <param name="endPoint">The endpoint of connected socket</param>
        public abstract void OnConnected(EndPoint endPoint);
        /// <summary>
        /// Called when the socket received data(buffer)
        /// </summary>
        /// <param name="buffer">The buffer of unit packet received.</param>
        public abstract void OnRecv(ReadOnlySpan<byte> buffer);
        /// <summary>
        /// Called when the socket sent data.
        /// </summary>
        /// <param name="numOfBytes">The length of bytes transferred.</param>
        public abstract void OnSend(int numOfBytes);
        /// <summary>
        /// Called when the socket is disconnected
        /// </summary>
        /// <param name="endPoint">The end point of the socket.</param>
        /// <param name="error">The additional object of error</param>
        public abstract void OnDisconnected(EndPoint endPoint, object error = null);
        
#endregion

        /// <summary>
        /// The value to check the session disconnected. (Used with Interlocked)
        /// </summary>
        int _disconnected = 0;

        /// <summary>
        /// A session must be initialized with this method with socket.
        /// </summary>
        /// <param name="socket">The socket to be connected to the session.</param>
        public void Init(Socket socket)
        {
            _socket = socket;

            _sendEvent.Completed += new(OnSendCompleted);
            _recvEvent.Completed += new(OnRecvCompleted);

            RegisterRecv();
        }

#if PROTOBUF
        /// <summary>
        /// Send message packet to endpoint of the socket. [Protobuf]
        /// </summary>
        /// <typeparam name="T">Google.Protobuf.IMessage and ServerCoreTCP.Protobuf.IPacket</typeparam>
        /// <param name="message">The message to send.</param>
        public void Send<T>(T message) where T : IMessage, Protobuf.IPacket
        {
            int size = (int)message.CalcSize();
#if MEMORY_BUFFER
            Memory<byte> buffer = MSendBufferTLS.Reserve(size);
            message.WriteTo(buffer.Span);
            var sendBuffer = MSendBufferTLS.Return(size);
#else
            ArraySegment<byte> buffer = SendBufferTLS.Reserve(size);
            message.WriteTo(buffer);
            var sendBuffer = SendBufferTLS.Return(size);
#endif

            lock (_lock)
            {
                _sendQueue.Enqueue(sendBuffer);

                if (_sendPendingList.Count == 0) RegisterSend();
            }
        }
#endif

#if PROTOBUF_WRAPPER
        /// <summary>
        /// Send message to endpoint of the socket [Protobuf Wrapper]
        /// </summary>
        /// <typeparam name="T">Google.Protobuf.IMessage</typeparam>
        /// <param name="packet">The message to send.</param>
        public void Send<T>(T message) where T : IMessage
        {
#if MEMORY_BUFFER
            Send(message.MSerializeWrapper());
#else
            Send(message.SerializeWrapper());
#endif
        }
#endif

#if CUSTOM_PACKET
        /// <summary>
        /// Send message packet to endpoint of the socket. [Custom Packet]
        /// </summary>
        /// <param name="packet">The packet to send</param>
        public void Send(CustomBuffer.IPacket packet)
        {
#if MEMORY_BUFFER
            Send(packet.MSerialize());
#else
            Send(packet.Serialize());
#endif
        }
#endif

        /// <summary>
        /// Send data to endpoint of the socket. [ArraySegment]
        /// </summary>
        /// <param name="sendBuffer">Serialized data to send</param>
        public void Send(ArraySegment<byte> sendBuffer)
        {
#if MEMORY_BUFFER
            throw new Exception("The session uses Memory<byte> buffer now.");
#else
            lock (_lock)
            {
                _sendQueue.Enqueue(sendBuffer);

                if (_sendPendingList.Count == 0) RegisterSend();
            }
#endif
        }

        /// <summary>
        /// Send data to endpoint of the socket. [Memory]
        /// </summary>
        /// <param name="sendBuffer">Serialized data to send</param>
        public void Send(Memory<byte> sendBuffer)
        {
#if MEMORY_BUFFER
            lock (_lock)
            {
                _sendQueue.Enqueue(sendBuffer);

                if (_sendPendingList.Count == 0) RegisterSend();
            }
#else
            throw new Exception("The session uses ArraySegment<byte> buffer now.");
#endif
        }

        /// <summary>
        /// Reserve 'Send' for async-send (Note: It needs to be protected for race-condition.)
        /// </summary>
        void RegisterSend()
        {
            // If it is already disconnected, return
            if (_disconnected == 1) return;

            // NOTE:
            // DO NOT ADD ITEMS to eventArgs.BufferList through Add method.
            // It does not work well and causes SocketError.InvalidArguments.
            // USE EventArgs.BufferList = list instead.
            // WHY? https://stackoverflow.com/questions/11820677/how-use-bufferlist-with-socketasynceventargs-and-not-get-socketerror-invalidargu
#if MEMORY_BUFFER
            //sendEvent.SetBuffer(_sendMemory);
            while (_sendQueue.Count > 0)
            {
                _sendPendingList.Add(_sendQueue.Dequeue());
            }

            foreach (Memory<byte> buff in _sendPendingList)
            {
                if (MemoryMarshal.TryGetArray<byte>(buff, out var segment))
                {
                    _sendBufferList.Add(segment);
                }
                else
                {
                    Console.WriteLine($"Error: ReisterSend"); ;
                }
            }
            _sendEvent.BufferList = _sendBufferList;
            _sendPendingList.Clear();
#else
            //sendEvent.SetBuffer(_sendSegment.Array, 0, _sendSegment.Count);
            while (_sendQueue.Count > 0)
            {
                _sendPendingList.Add(_sendQueue.Dequeue());
            }

            _sendEvent.BufferList = _sendPendingList;
#endif

            try
            {
                bool pending = _socket.SendAsync(_sendEvent);
                if (pending == false) OnSendCompleted(null, _sendEvent);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: RegisterSend - {0}", e);
            }
        }

        /// <summary>
        /// Reserve 'Receive' for async-receive
        /// </summary>
        void RegisterRecv()
        {
            if (_disconnected == 1) return;

#if MEMORY_BUFFER
            _recvBuffer.CleanUp();
            _recvEvent.SetBuffer(_recvBuffer.WriteMemory);
#else
            _recvBuffer.CleanUp(); // expensive
            var segment = _recvBuffer.WriteSegment;
            _recvEvent.SetBuffer(segment.Array, segment.Offset, segment.Count);
#endif

            try
            {
                bool pending = _socket.ReceiveAsync(_recvEvent);
                if (pending == false) OnRecvCompleted(null, _recvEvent);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: RegisterRecv - {0}", ex);
            }
        }

        /// <summary>
        /// Callback that is called when send-operation is completed.
        /// </summary>
        /// <param name="sender">[Ignored] The source of the event</param>
        /// <param name="e">An object that contains the socket-async-send-event data</param>
        void OnSendCompleted(object sender, SocketAsyncEventArgs e)
        {
            lock (_lock)
            {
                // Check the length of bytes transferred and SocketError==Success
                if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
                {
                    try
                    {
                        OnSend(e.BytesTransferred);
#if MEMORY_BUFFER
                        _sendBufferList.Clear();
                        //e.SetBuffer(null);
                        _sendEvent.BufferList = null;
#else
                        _sendEvent.BufferList = null;
                        _sendPendingList.Clear();
#endif
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error: OnSendCompleted - {0}", ex);
                    }
                }
                else if (e.BytesTransferred == 0)
                {
                    Console.WriteLine($"The length of sent data was 0.");
                }
                else if (e.SocketError != SocketError.Success)
                {
                    Console.WriteLine($"SocketError: {e.SocketError}");
                }
                else
                {
                    Disconnect();
                }
            }
        }

        /// <summary>
        /// Callback that is called when recv-operation is completed.
        /// </summary>
        /// <param name="sender">[Ignored] The source of the event</param>
        /// <param name="e">An object that contains the socket-async-recv-event data</param>
        void OnRecvCompleted(object sender, SocketAsyncEventArgs e)
        {
#if DEBUG
            Console.WriteLine($"Received: {e.BytesTransferred} bytes");
#endif

            // Check the length of bytes transferred and SocketError==Success
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                try
                {
                    if (_recvBuffer.OnWrite(e.BytesTransferred) == false)
                    {
                        Disconnect();
                        return;
                    }
#if MEMORY_BUFFER
                    int processLength = OnRecvProcess(_recvBuffer.DataMemory);
#else
                    int processLength = OnRecvProcess(_recvBuffer.DataSegment);
#endif
                    if (processLength <= 0)
                    {
                        Disconnect();
                        return;
                    }

                    if (_recvBuffer.DataSize < processLength)
                    {
                        Disconnect();
                        return;
                    }

                    if (_recvBuffer.OnRead(processLength) == false)
                    {
                        Disconnect();
                        return;
                    }

                    // Wait to receive again.
                    RegisterRecv();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: OnRecvCompleted - {0}", ex);
                }
            }
            else if (e.BytesTransferred == 0)
            {
                Console.WriteLine($"The length of received data is 0.");
            }
            else if (e.SocketError != SocketError.Success)
            {
                Console.WriteLine($"SocketError: {e.SocketError}");
            }
            else
            {
                Disconnect();
            }

        }

        /// <summary>
        /// Check the received buffer. If there are multiple packet data on the buffer, each data is processed separately. OnRecv will be called here.
        /// </summary>
        /// <param name="buffer">The buffer received on socket.</param>
        /// <returns>The length of processed bytes.</returns>
        int OnRecvProcess(Memory<byte> buffer) 
        {
            // If the size of received buffer is shorter than the header size, it is not the whole data.
            if (buffer.Length < HeaderSize) return 0;

            int processed = 0;

            while (processed < buffer.Length)
            {
#if PROTOBUF
                // Note: the protobuf packet buffer (The size type is fixed32 that has 4 bytes on PROTO and it becomes uint type on C#)
                // [tag, 1][size, 4][tag, 1][pacektType, 1][data, ~]
                // Get total size of the unit packet (sizeof(uint) = 4)
                // Jump the tag size (1 byte)
                int size = (int)BitConverter.ToUInt32(buffer.Span.Slice(processed + 1, HeaderSize));
#elif PROTOBUF_WRAPPER
                // Get total size of the unit packet (ushort)
                ushort size = BitConverter.ToUInt16(buffer.Span.Slice(processed, HeaderSize));
#else
                // Get total size of the unit packet (ushort)
                ushort size = BitConverter.ToUInt16(buffer.Span.Slice(processed, HeaderSize));
#endif
                if (size + processed > buffer.Length) break;

                ReadOnlySpan<byte> data = buffer.Span.Slice(processed, size);
                OnRecv(data);

                processed += size;
            }

            return processed;
        }

        /// <summary>
        /// Check the received buffer. If there are multiple packet data on the buffer, each data is processed separately. OnRecv will be called here.
        /// </summary>
        /// <param name="buffer">The buffer received on socket.</param>
        /// <returns>The length of processed bytes.</returns>
        int OnRecvProcess(ArraySegment<byte> buffer)
        {
            // If the size of received buffer is shorter than the header size, it is not the whole data.
            if (buffer.Count < HeaderSize) return 0;

            int processed = 0;

            while (processed < buffer.Count)
            {
#if PROTOBUF
                // Note: the protobuf packet buffer (The size type is fixed32 that has 4 bytes on PROTO and it becomes uint type on C#)
                // [tag, 1][size, 4][tag, 1][pacektType, 1][data, ~]
                // Get total size of the unit packet (sizeof(uint) = 4)
                // Jump the tag size (1 byte)
                int size = (int)BitConverter.ToUInt32(buffer.Slice(processed + 1, HeaderSize));
#elif PROTOBUF_WRAPPER
                // Get total size of the unit packet (ushort)
                ushort size = BitConverter.ToUInt16(buffer.Slice(processed, HeaderSize));
#else
                // Get total size of the unit packet (ushort)
                ushort size = BitConverter.ToUInt16(buffer.Slice(processed, HeaderSize));
#endif
                if (size + processed > buffer.Count) break;

                ReadOnlySpan<byte> data = buffer.Slice(processed, size);
                OnRecv(data);

                processed += size;
            }

            return processed;
        }


        /// <summary>
        /// Close the socket and clear the session.
        /// </summary>
        void Disconnect()
        {
            // Check that it is already disconnected
            if (Interlocked.Exchange(ref _disconnected, 1) == 1) return;

            OnDisconnected(_socket.RemoteEndPoint);

            // Shutdown send/recv both
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();

            Clear();
        }

        void Clear()
        {
            lock (_lock)
            {
                _sendQueue.Clear();
                _sendPendingList.Clear();
#if MEMORY_BUFFER
                _sendBufferList.Clear();
#endif
            }
        }
    }
}

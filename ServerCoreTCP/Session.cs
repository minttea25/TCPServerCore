using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCoreTCP
{
    public abstract class Session
    {
        const int RecvBufferSize = 65535;

        Socket _socket;
        readonly RecvBuffer _recvBuffer = new(RecvBufferSize);

        /// <summary>
        /// Each session reuses one SendEventArgs.
        /// </summary>
        readonly SocketAsyncEventArgs sendEvent = new();
        /// <summary>
        /// Each session reuses one RecvEventArgs.
        /// </summary>
        readonly SocketAsyncEventArgs recvEvent = new();

        /// <summary>
        /// Note: Send/Recv can be occurred in multiple threads
        /// </summary>
        readonly object _lock = new();

        /// <summary>
        /// [Temp] The byte segment to store send buffer temporarily.
        /// </summary>
        ArraySegment<byte> _sendSegment;

        #region Abstract Methods
        /// <summary>
        /// Called when the socket is connected.
        /// </summary>
        /// <param name="endPoint">The endpoint of connected socket</param>
        public abstract void OnConnected(EndPoint endPoint);
        /// <summary>
        /// Called when the socket received data(buffer)
        /// </summary>
        /// <param name="buffer">The data received</param>
        /// <returns>The length of bytes processed in this method</returns>
        public abstract int OnRecv(ArraySegment<byte> buffer);
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

            sendEvent.Completed += new(OnSendCompleted);
            recvEvent.Completed += new(OnRecvCompleted);

            RegisterRecv();
        }

        /// <summary>
        /// Send data to endpoint of the socket.
        /// </summary>
        /// <param name="sendBuffer">A byte buffer which contains data</param>
        public void Send(ArraySegment<byte> sendBuffer)
        {
            lock (_lock)
            {
                _sendSegment = sendBuffer;
                RegisterSend();
            }
        }

        /// <summary>
        /// Reserve 'Send' for async-send (Note: It needs to be protected for race-condition.)
        /// </summary>
        void RegisterSend()
        {
            // If it is already disconnected, return
            if (_disconnected == 1) return;

            // TODO - Check memory and byte[]
            //sendEvent.SetBuffer(new Memory<byte>(buffer.Array, 0, buffer.Count));
            // sendEvent.SetBuffer(buffer, 0, buffer.Length);

            // Set data buffer on the buffer of SendEventArgs
            sendEvent.SetBuffer(_sendSegment.Array, 0, _sendSegment.Count);
            // After using data buffer, make it null
            _sendSegment = null;

            try
            {
                bool pending = _socket.SendAsync(sendEvent);
                if (pending == false) OnSendCompleted(null, sendEvent);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: RegisterSend - {0}", e);
            }
        }

        /// <summary>
        /// Reserve 'Recieve' for async-receive
        /// </summary>
        void RegisterRecv()
        {
            if (_disconnected == 1) return;

            _recvBuffer.CleanUp(); // expensive

            var segment = _recvBuffer.WriteSegment;
            recvEvent.SetBuffer(segment.Array, segment.Offset, segment.Count);

            try
            {
                bool pending = _socket.ReceiveAsync(recvEvent);

                if (pending == false) OnRecvCompleted(null, recvEvent);
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
                        // e.SetBuffer(null);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error: OnSendCompleted - {0}", ex);
                    }
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
            // Check the length of bytes transferred and SocketError==Success
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                try
                {
                    // Check if there is enough size to write on RecvBuffer.
                    if (_recvBuffer.OnWrite(e.BytesTransferred) == false)
                    {
                        Disconnect();
                        return;
                    }

                    // Check that the data has been processed normally from OnRecv.
                    var processLength = OnRecv(_recvBuffer.DataSegment);
                    if (processLength < 0 || _recvBuffer.DataSize < processLength)
                    {
                        Disconnect();
                        return;
                    }

                    // Check the written data was readable normally => notice 'the data is read'
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
            else
            {
                Disconnect();
            }
            
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
            ;
        }
    }
}

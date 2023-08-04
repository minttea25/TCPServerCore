using System;
using System.Net;
using System.Net.Sockets;

namespace ServerCoreTCP
{
    public class Connector
    {
        Func<Session> _sessionFactory;

        /// <summary>
        /// Create a socket and connect to end point
        /// </summary>
        /// <param name="endPoint">The endpoint to connect to</param>
        /// <param name="session">The session of the socket</param>
        /// <param name="count">[Debug] The count of socket to connect</param>
        public void Connect(IPEndPoint endPoint, Func<Session> sessionFactory, int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                Socket socket = new(
                    endPoint.AddressFamily,
                    SocketType.Stream,
                    ProtocolType.Tcp);
                this._sessionFactory = sessionFactory;

                SocketAsyncEventArgs e = new();
                e.Completed += OnConnectCompleted;
                e.RemoteEndPoint = endPoint;
                // Set socket as UserToken object
                e.UserToken = socket;

                RegisterConnect(e);
            }
        }

        public void ConnectSync(IPEndPoint endPoint, Func<Session> sessionFactory, Action<Socket> callback = null)
        {
            Socket socket = new(
                    endPoint.AddressFamily,
                    SocketType.Stream,
                    ProtocolType.Tcp);
            this._sessionFactory = sessionFactory;

            socket.Connect(endPoint);
            callback?.Invoke(socket);
        }

        /// <summary>
        /// Register function waiting for completed connection
        /// </summary>
        /// <param name="e">An object that contains the socket-async-event data</param>
        void RegisterConnect(SocketAsyncEventArgs e)
        {
            // Check the user token of event is socket & Cast UserToken to Socket
            if (e.UserToken is not Socket socket) return;

            bool pending = socket.ConnectAsync(e);

            // If pending is false => call callback function right away.
            if (pending == false) OnConnectCompleted(null, e);

            // If pending is true
            // => when the socket is connected, the registered delegate (OnConnectedCompleted) would be invoked.
        }

        /// <summary>
        /// Callback that is called when Connected
        /// </summary>
        /// <param name="sender">[Ignored] The source of the event</param>
        /// <param name="e">An object that contains the socket-async-event data</param>
        void OnConnectCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                //Logger.Instance.LogInfo($@"Connected");
                // TODO with Session
                Session session = _sessionFactory.Invoke();
                session.Init(e.ConnectSocket);
                session.OnConnected(e.RemoteEndPoint);
            }
            else
            {
                // error
                //Logger.Instance.LogError($@"Connector: {e.SocketError}");
            }
        }
    }
}

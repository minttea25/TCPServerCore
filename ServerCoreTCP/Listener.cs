using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCoreTCP
{
    public class Listener
    {
        readonly Socket _listenSocket;
        readonly Func<Session> _sessionFactory;

        /// <summary>
        /// Create a socket and bind the endpoint.
        /// </summary>
        /// <param name="endPoint">The endpoint to bind to socket</param>
        /// <param name="session">The session of the socket</param>
        public Listener(IPEndPoint endPoint, Func<Session> sessionFactory)
        {
            _listenSocket = new(
                endPoint.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);

            this._sessionFactory = sessionFactory;

            _listenSocket.Bind(endPoint);
        }

        /// <summary>
        /// Create a socket and bind IP address and port to socket.
        /// </summary>
        /// <param name="ipAddress">The IP address of endpoint</param>
        /// <param name="port">The port number of endpoint</param>
        public Listener(IPAddress ipAddress, int port)
        {
            IPEndPoint endPoint = new(ipAddress, port);
            _listenSocket = new(
                endPoint.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);

            _listenSocket.Bind(endPoint);
        }

        /// <summary>
        /// Place the socket in a listening state.
        /// </summary>
        /// <param name="backLog">The maximum length of the pending connections queue</param>
        /// <param name="register">The maximum number of waiting for accept.</param>
        public void Listen(int backLog = 100, int register = 10)
        {
            _listenSocket.Listen(backLog);

            for (int i=0; i<register; ++i)
            {
                SocketAsyncEventArgs e = new();
                e.Completed += new(OnAcceptCompleted);
                RegisterAccept(e);
            }

            Console.WriteLine("Socket is listening...");
        }

        /// <summary>
        /// Register as waiting for Accept
        /// </summary>
        /// <param name="e">An object that contains the socket-async-event data</param>
        void RegisterAccept(SocketAsyncEventArgs e)
        {
            // Reset for re-using
            e.AcceptSocket = null;

            bool pending = _listenSocket.AcceptAsync(e);

            // If pending is false => call callback function right away
            if (pending == false) OnAcceptCompleted(null, e);

            // If pending is true
            // => when the accept is completed, the registered delegate (OnAcceptCompelted) would be invoked.
        }

        /// <summary>
        /// Callback that is called when Accept.
        /// </summary>
        /// <param name="sender">[Ignored] The source of the event</param>
        /// <param name="e">An object that contains the socket-async-event data</param>
        void OnAcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                //Logger.Instance.LogInfo($@"Accpeted: {e.RemoteEndPoint}");
                // TODO with session

                // You must create session here
                // Why? Each thread has each session.

                // create session through factory
                Session session = _sessionFactory.Invoke();
                session.Init(e.AcceptSocket);
                session.OnConnected(e.AcceptSocket.RemoteEndPoint);
            }
            else
            {
                // error
                //Logger.Instance.LogError($@"Listner: {e.SocketError}");
            }

            // After Accept, wait again for other Accepts.
            RegisterAccept(e);
        }
    }
}

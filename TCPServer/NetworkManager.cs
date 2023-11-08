using ServerCoreTCP;
using ServerCoreTCP.CLogger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer
{
    public class NetworkManager
    {
        readonly int Port = 8888;
        public Func<ClientSession> SessionFactory => m_emptySessionFactory;
        public ServerService serverService;

        Func<ClientSession> m_emptySessionFactory = null;

        public NetworkManager(Func<ClientSession> sessionFactory, int port = 8888)
        {
            m_emptySessionFactory = sessionFactory;
            Port = port;
        }

        public void StartServer(int register = 10, int backLog = 100)
        {
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, Port);

            CoreLogger.LogInfo("NetworkManager", "Server started");

            serverService = new ServerService(endPoint, m_emptySessionFactory);
            serverService.Start();
        }

        public void StopServer()
        {
            string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            CoreLogger.LogInfo("Server", "Server stopped at {0}", time);
        }
    }
}
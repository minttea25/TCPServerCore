using ServerCoreTCP;
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
        public Func<ClientSession> SessionFactory => _sessionFactory;

        Func<ClientSession> _sessionFactory = null;

        public NetworkManager(Func<ClientSession> sessionFactory, int port = 8888)
        {
            _sessionFactory = sessionFactory;
            Port = port;
        }

        public void StartServer(int register = 10, int backLog = 100)
        {
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, Port);

            ServerService serverService = new ServerService(endPoint, _sessionFactory);
            serverService.Start();

            Program.ConsoleLogger.Information("Now listening on port: {Port}", Port);
        }

        public void StopServer()
        {
            string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Program.ConsoleLogger.Information("Server stopped at {time}", time);
        }
    }
}
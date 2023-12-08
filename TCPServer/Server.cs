using ServerCoreTCP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TCPServer
{
    public class Server
    {
        bool _isRunning = false;
        readonly ServerService service;

        public Server(IPEndPoint endPoint, Func<ClientSession> factory, ServerServiceConfig config) 
        {
            service = new(endPoint, factory, config);
        }

        public void Start()
        {
            if (_isRunning == true) return;
            _isRunning = true;
            service.Start();
        }

        public void Stop()
        {
            if (_isRunning == false) return;
            _isRunning = false;
            service.Stop();
        }

    }
}

#if PROTOBUF

using ServerCoreTCP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Chat.Protobuf
{
    public class Server
    {
        bool _isRunning = false;
        public readonly ServerService Service;

        public Server(IPEndPoint endPoint, Func<ClientSession> factory, ServerServiceConfig config) 
        {
            Service = new(endPoint, factory, config);
        }

        public void Start()
        {
            if (_isRunning == true) return;
            _isRunning = true;
            Service.Start();
        }

        public void Stop()
        {
            if (_isRunning == false) return;
            _isRunning = false;
            Service.Stop();
        }

    }
}

#endif
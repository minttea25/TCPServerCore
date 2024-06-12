#if FLATBUFFERS

using ServerCoreTCP;
using System;
using System.Net;

namespace Test.Flatbuffers
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
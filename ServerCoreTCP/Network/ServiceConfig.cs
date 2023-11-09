using System;

namespace ServerCoreTCP
{
    [Serializable]
    public struct ClientServiceConfig
    {
        public int ClientServiceSessionCount;

        public int ConnectTimeOutMilliseconds;

        //public int SendTimeOutMilliseconds;
        //public int RecvTimeOutMilliseconds;
        public bool ReuseAddress;


        public static ClientServiceConfig GetDefault()
        {
            return new ClientServiceConfig()
            {
                ClientServiceSessionCount = 1,
                ConnectTimeOutMilliseconds = 3000,
                ReuseAddress = true,
            };
        }
    }

    [Serializable]
    public struct ServerServiceConfig
    {
        // Note: SocketAsyncEventArgs must be larger than
        // (SessionPoolCount * 2 + RegisterListenCount)

        public int SocketAsyncEventArgsPoolCount;
        public int SessionPoolCount;

        public int ListenerBacklogCount;
        public int RegisterListenCount;

        //public int SendTimeOutMilliseconds;
        //public int RecvTimeOutMilliseconds;
        public bool KeepAlive;
        public bool NoDelay; // for Nagle algorithm
        public bool ReuseAddress;
        public int Linger; // if 0, linger set false

        public static ServerServiceConfig GetDefault()
        {
            return new ServerServiceConfig()
            {
                SocketAsyncEventArgsPoolCount = 500,
                SessionPoolCount = 200,
                ListenerBacklogCount = 100,
                RegisterListenCount = 10,
                KeepAlive = false,
                NoDelay = false,
                ReuseAddress = true,
                Linger = 0
            };
        }
    }
}

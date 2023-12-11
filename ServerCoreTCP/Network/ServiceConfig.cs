using System;

namespace ServerCoreTCP
{
    [Serializable]
    public struct ClientServiceConfig
    {
        public int ClientServiceSessionCount { get; set; }
        public bool ReuseAddress { get; set; }

        public static ClientServiceConfig GetDefault()
        {
            return new ClientServiceConfig()
            {
                ClientServiceSessionCount = 1,
                ReuseAddress = true,
            };
        }
    }

    [Serializable]
    public struct ServerServiceConfig
    {
        /// <summary>
        /// The count of SocketAsyncEventArgs must be larger than (SessionPoolCount * 2 + RegisterListenCount)
        /// </summary>
        public int SocketAsyncEventArgsPoolCount { get; set; }
        public int SessionPoolCount { get; set; }
        
        public int ListenerBacklogCount { get; set; }
        /// <summary>
        /// If 0, the listener will throw exception. It must be larger than 0.
        /// </summary>
        public int RegisterListenCount { get; set; }

        /// <summary>
        /// About Nagle algorithm
        /// </summary>
        public bool NoDelay { get; set; }
        public bool ReuseAddress { get; set; }
        /// <summary>
        /// If 0, linger set false, otherwise set the time in seconds.
        /// </summary>
        public int Linger { get; set; }

        public static ServerServiceConfig GetDefault()
        {
            return new ServerServiceConfig()
            {
                SocketAsyncEventArgsPoolCount = 500,
                SessionPoolCount = 200,
                ListenerBacklogCount = 100,
                RegisterListenCount = 10,
                NoDelay = false,
                ReuseAddress = true,
                Linger = 0
            };
        }
    }
}

using System;

namespace ServerCoreTCP
{
    [Serializable]
    public struct ClientServiceConfig
    {
        public const float SAEACountMultiplier = 1.1f;

        public readonly int SocketAsyncEventArgsPoolCount => m_socketAsyncEventArgsPoolCount;
        public int ClientServiceSessionCount { get; set; }
        public bool ReuseAddress { get; set; }

        int m_socketAsyncEventArgsPoolCount;

        public static ClientServiceConfig GetDefault(int sessionCount = 1)
        {
            ClientServiceConfig config = new ClientServiceConfig()
            {
                ClientServiceSessionCount = sessionCount,
                ReuseAddress = true,
            };
            config.m_socketAsyncEventArgsPoolCount = GetSAEAPoolCount(sessionCount);

            return config;
        }

        static int GetSAEAPoolCount(int sessionCount)
        {
            return (int)((sessionCount * 2) * SAEACountMultiplier);
        }
    }

    [Serializable]
    public struct ServerServiceConfig
    {
        public const float SAEACountMultiplier = 1.1f;

        /// <summary>
        /// The count of SocketAsyncEventArgs must be larger than (SessionPoolCount * 2 + RegisterListenCount)
        /// </summary>
        public readonly int SocketAsyncEventArgsPoolCount => m_socketAsyncEventArgsPoolCount;
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

        int m_socketAsyncEventArgsPoolCount;

        public static ServerServiceConfig GetDefault(int sessionCount = 100, int registerListenCount = 10)
        {
            ServerServiceConfig config = new ServerServiceConfig()
            {
                SessionPoolCount = sessionCount,
                ListenerBacklogCount = 100,
                RegisterListenCount = registerListenCount,
                NoDelay = false,
                ReuseAddress = true,
                Linger = 0,
            };

            config.m_socketAsyncEventArgsPoolCount = GetSAEAPoolCount(config.SessionPoolCount, config.RegisterListenCount);
            return config;
            
        }

        static int GetSAEAPoolCount(int sessionPoolCount, int registerListenCount)
        {
            return (int)((sessionPoolCount * 2 + registerListenCount) * SAEACountMultiplier);
        }
    }
}

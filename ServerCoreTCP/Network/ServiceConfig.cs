using System;

namespace ServerCoreTCP
{
    [Serializable]
    public struct ClientServiceConfig
    {
        /// <summary>
        /// The value of setting the margin counts of the SocketAsyncEventArgs.
        /// </summary>
        public const float SAEACountMultiplier = 1.1f;

        /// <summary>
        /// The count of SocketAsyncEventArgs must be larger than (SessionPoolCount * 2 + RegisterListenCount)
        /// </summary>
        public readonly int SocketAsyncEventArgsPoolCount => m_socketAsyncEventArgsPoolCount;

        /// <summary>
        /// The session count of the client service.
        /// </summary>
        public int ClientServiceSessionCount { get; set; }
        /// <summary>
        /// Use reuse-address option of the socket.
        /// </summary>
        public bool ReuseAddress { get; set; }

        int m_socketAsyncEventArgsPoolCount;

        /// <summary>
        /// ReuseAddress: true
        /// </summary>
        /// <param name="sessionCount">SessionCount</param>
        /// <returns></returns>
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

        /// <summary>
        /// Calculate an additional margin of 10 percent. (SessionCount * 2 )
        /// </summary>
        /// <param name="sessionCount">SessionCount</param>
        /// <returns></returns>
        static int GetSAEAPoolCount(int sessionCount)
        {
            return (int)((sessionCount * 2) * SAEACountMultiplier);
        }
    }

    [Serializable]
    public struct ServerServiceConfig
    {
        /// <summary>
        /// The value of setting the margin counts of the SocketAsyncEventArgs.
        /// </summary>
        public const float SAEACountMultiplier = 1.1f;

        /// <summary>
        /// The count of SocketAsyncEventArgs must be larger than (SessionPoolCount * 2 + RegisterListenCount)
        /// </summary>
        public readonly int SocketAsyncEventArgsPoolCount => m_socketAsyncEventArgsPoolCount;
        public int SessionPoolCount { get; set; }
        
        /// <summary>
        /// The count of backlogs of listening.
        /// </summary>
        public int ListenerBacklogCount { get; set; }
        /// <summary>
        /// If 0, the listener will throw exception. It must be larger than 0.
        /// </summary>
        public int RegisterListenCount { get; set; }

        /// <summary>
        /// Use Nagle algorithm option of the socket.
        /// </summary>
        public bool NoDelay { get; set; }
        /// <summary>
        /// Reuse address option of the socket
        /// </summary>
        public bool ReuseAddress { get; set; }
        /// <summary>
        /// If 0, linger set false, otherwise set the time in seconds.
        /// </summary>
        public int Linger { get; set; }

        int m_socketAsyncEventArgsPoolCount;

        /// <summary>
        /// NoDelay: false, ReuseAddress: true, Linger: false
        /// </summary>
        /// <param name="sessionCount">SessionPoolCount</param>
        /// <param name="registerListenCount">RegisterListenCount</param>
        /// <returns></returns>
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

        /// <summary>
        /// Calculate an additional margin of 10 percent. (SessionPoolCount * 2 + RegisterListenCount)
        /// </summary>
        /// <param name="sessionPoolCount">SessionPoolCount</param>
        /// <param name="registerListenCount">RegisterListenCount</param>
        /// <returns></returns>
        static int GetSAEAPoolCount(int sessionPoolCount, int registerListenCount)
        {
            return (int)((sessionPoolCount * 2 + registerListenCount) * SAEACountMultiplier);
        }
    }
}

namespace ServerCoreTCP
{
    public class Defines
    {
        #region Packet
        /// <summary>
        /// The size of packet body: sizeof(ushort).
        /// </summary>
        public const int PACKET_HEADER_SIZE = sizeof(ushort);
        /// <summary>
        /// The size of packet type.
        /// </summary>
#if PACKET_TYPE_INT
        public const int PACKET_DATATYPE_SIZE = sizeof(uint);
#else
        public const int PACKET_DATATYPE_SIZE = sizeof(ushort);
#endif
        #endregion

        #region Buffer
        public const int RecvBufferSize = 65535 * 10;
        public const int SendBufferSize = 65535 * 10;
        #endregion

        #region Session
        /// <summary>
        /// It can be changed by user.
        /// </summary>
        public static long SessionSendFlushMinIntervalMilliseconds = 100;
        public const int SessionSendFlushMinReservedByteLength = 10000;
        #endregion

    }
}

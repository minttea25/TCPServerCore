namespace ServerCoreTCP
{
    public class Defines
    {
        #region Packet
        /// <summary>
        /// The size of packet body: sizeof(ushort).
        /// </summary>
        public const int PACKET_HEADER_SIZE = sizeof(ushort) + sizeof(ushort);
        public const int PACKET_ID_SIZE = sizeof(ushort);
        public const int PACKET_SIZETYPE_SIZE = sizeof(ushort);
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

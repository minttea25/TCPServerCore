namespace NetCore
{
    public class Defines
    {
        #region Packet
        public const int PACKET_HEADER_SIZE = sizeof(ushort) + sizeof(ushort);
        public const int PACKET_ID_SIZE = sizeof(ushort);
        public const int PACKET_SIZETYPE_SIZE = sizeof(ushort);
        #endregion

        #region Buffer
        public const int RecvBufferSize = 65535 * 10;
        public const int SendBufferSize = 65535 * 10;
        #endregion

        #region Session
        public const long SessionSendFlushMinIntervalMilliseconds = 100;
        public const int SessionSendFlushMinReservedByteLength = 10000;
        #endregion

    }
}

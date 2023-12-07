using System;
using System.Collections.Generic;
using System.Text;

namespace ServerCoreTCP
{
#if PACKET_TYPE_INT
#else
#endif
    public class Defines
    {
        #region Packet
        public const int PACKET_HEADER_SIZE = sizeof(ushort);
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

    }
}

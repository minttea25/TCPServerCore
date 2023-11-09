using ServerCoreTCP.CLogger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCoreTCP
{
    /// <summary>
    /// Each session should access the send buffer through this class.
    /// </summary>
    public class SendBufferTLS
    {
        public static int BufferSize { get; private set; } = 65535 * 100;

        //public readonly static ThreadLocal<SendBuffer> TLS_CurrentBuffer 
        //    = new ThreadLocal<SendBuffer>(() => new SendBuffer(BufferSize));

        public readonly static ThreadLocal<SendBuffer> TLS_CurrentBuffer
            = new ThreadLocal<SendBuffer>(() => { return null; });


        /// <summary>
        /// Helper method of SendBuffer.Reserve
        /// </summary>
        /// <param name="reserveSize">The size to reserve.</param>
        /// <returns>The segment of Memory reserved</returns>
        public static ArraySegment<byte> Reserve(int reserveSize)
        {
            if (reserveSize > BufferSize)
            {
                CoreLogger.LogError("SendBuffer.Reserve", "The reserveSize[{0}] is bigger than the bufferSize[{1}]", reserveSize, BufferSize);
                return null;
            }

            if (TLS_CurrentBuffer.Value == null)
            {
                TLS_CurrentBuffer.Value = new SendBuffer(BufferSize);
            }

            if (TLS_CurrentBuffer.Value.FreeSize < reserveSize) TLS_CurrentBuffer.Value = new SendBuffer(BufferSize);

            return TLS_CurrentBuffer.Value.Reserve(reserveSize);
        }

        /// <summary>
        /// Helper method of SendBuffer.Return
        /// </summary>
        /// <param name="usedSize">The size used actually.</param>
        /// <returns>The segment of array used</returns>
        public static ArraySegment<byte> Return(int usedSize)
        {
            return TLS_CurrentBuffer.Value.Return(usedSize);
        }

        public static ArraySegment<byte> Use(int size)
        {
            return TLS_CurrentBuffer.Value.Use(size);
        }
    }

    /// <summary>
    /// The buffer for Send using byte array
    /// </summary>
    public class SendBuffer
    {
        readonly byte[] buffer;
        int usedSize = 0;

        public int FreeSize { get { return buffer.Length - usedSize; } }

        public SendBuffer(int bufferSize)
        {
            buffer = new byte[bufferSize];
        }

        /// <summary>
        /// Return a segment which can be used of the size of reserveSize.
        /// </summary>
        /// <param name="reserveSize"></param>
        /// <returns>If there is no enough size to reserve, returns null.</returns>
        public ArraySegment<byte> Reserve(int reserveSize)
        {
            if (reserveSize > FreeSize)
            {
                usedSize = 0;
            }

            // return buffer segment from usedSize to usedSize + reserveSize
            return new ArraySegment<byte>(buffer, usedSize, reserveSize);
        }

        /// <summary>
        /// Return the used segment actually after calling Reserve.
        /// </summary>
        /// <param name="usedSize">Used buffer size actullay</param>
        /// <returns>The segment of the actually used buffer.</returns>
        public ArraySegment<byte> Return(int usedSize)
        {
            ArraySegment<byte> segment = new ArraySegment<byte>(buffer, this.usedSize, usedSize);
            this.usedSize += usedSize;
            return segment;
        }

        public ArraySegment<byte> Use(int size)
        {
            if (size > FreeSize) usedSize = 0;
            ArraySegment<byte> segment = new ArraySegment<byte>(buffer, usedSize, size);
            usedSize += size;
            return segment;
        }
    }
}

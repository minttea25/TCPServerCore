using ServerCoreTCP.Debug;
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
    public class MSendBufferTLS
    {
        public static int BufferSize { get; private set; } = 65535;

        public readonly static ThreadLocal<MSendBuffer> TLS_CurrentBuffer
            = new ThreadLocal<MSendBuffer>(valueFactory: () => new MSendBuffer(BufferSize));

        /// <summary>
        /// Helper method of MSendBuffer.Reserve
        /// </summary>
        /// <param name="reserveSize">The size to reserve.</param>
        /// <param name="extend">If true, when reserve size > usable size, reset the buffer through pointer = 0. </param>
        /// <returns>The segment of Memory reserved</returns>
        public static Memory<byte> Reserve(int reserveSize)
        {
#if DEBUG
            if (reserveSize > BufferSize)
            {
                if (CoreLogger.Logger != null)
                    CoreLogger.Logger.Error("The reserveSize[{reserveSize}] is bigger than the bufferSize[{BufferSize}] => return null", reserveSize, BufferSize);
                return null;
            }
#endif

            return TLS_CurrentBuffer.Value.Reserve(reserveSize);
        }

        /// <summary>
        /// Helper method of MSendBuffer.Return
        /// </summary>
        /// <param name="usedSize">The size used actually.</param>
        /// <returns>The segment of Memory used</returns>
        public static Memory<byte> Return(int usedSize)
        {
            return TLS_CurrentBuffer.Value.Return(usedSize);
        }

        public static Memory<byte> Use(int size)
        {
            return TLS_CurrentBuffer.Value.Use(size);
        }

    }

    /// <summary>
    /// The buffer for Send using Memory(byte)
    /// </summary>
    public class MSendBuffer
    {
        readonly byte[] buffer;
        int usedSize = 0;

        public int FreeSize { get { return buffer.Length - usedSize; } }
        
        public MSendBuffer(int bufferSize)
        {
            buffer = new byte[bufferSize];
        }

        /// <summary>
        /// Return a segment of Memory which can be used of the size of reserveSize.
        /// </summary>
        /// <param name="reserveSize">The size reserved</param>
        /// <param name="extend">If true, when the reserveSize > usableSize, reset the pointer = 0.</param>
        /// <returns>A segment of Memory which can be used of the size of reserveSize.</returns>
        public Memory<byte> Reserve(int reserveSize)
        {
            if (reserveSize > FreeSize)
            {
                usedSize = 0;
            }

            // return sliced memory from usedSize to usedSize + reserveSize
            return new Memory<byte>(buffer, start: usedSize, length: reserveSize);
        }

        /// <summary>
        /// Return the used segment of Memory actually after calling Reserve.
        /// </summary>
        /// <param name="usedSize">Used buffer size actullay</param>
        /// <returns>The Memory segment of the actually used buffer.</returns>
        public Memory<byte> Return(int usedSize)
        {
            Memory<byte> m = new Memory<byte>(buffer, start: this.usedSize, length: usedSize);
            this.usedSize += usedSize;
            return m;
        }

        public Memory<byte> Use(int size)
        {
            if (size > FreeSize) usedSize = 0;

            Memory<byte> m = new Memory<byte>(buffer, start: usedSize, length: size);
            usedSize += size;
            return m;
        }
    }
}

using ServerCoreTCP.CLogger;
using System;
using System.Threading;

namespace ServerCoreTCP
{
    /// <summary>
    /// Each session should access the send buffer through this class.
    /// </summary>
    public class SendBufferTLS
    {
        public readonly static ThreadLocal<SendBuffer> TLS_CurrentBuffer
            = new ThreadLocal<SendBuffer>(() => { return null; });


        /// <summary>
        /// Helper method of SendBuffer.Reserve
        /// </summary>
        /// <param name="reserveSize">The size to reserve.</param>
        /// <returns>The segment of Memory reserved</returns>
        public static ArraySegment<byte> Reserve(int reserveSize)
        {
            if (ValidateBuffer(reserveSize) == false) return null;

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
            if (ValidateBuffer(size) == false) return null;

            return TLS_CurrentBuffer.Value.Use(size);
        }

        static bool ValidateBuffer(int size)
        {
            if (size > Defines.SendBufferSize)
            {
                CoreLogger.LogError("SendBuffer.Reserve", "The reserveSize[{0}] is bigger than the bufferSize[{1}]", size, Defines.SendBufferSize);
                return false;
            }

            if (TLS_CurrentBuffer.Value == null) TLS_CurrentBuffer.Value = new SendBuffer(Defines.SendBufferSize);

            if (TLS_CurrentBuffer.Value.FreeSize < size) TLS_CurrentBuffer.Value = new SendBuffer(Defines.SendBufferSize);

            return true;
        }
    }

    /// <summary>
    /// The buffer for Send using byte array
    /// </summary>
    public class SendBuffer
    {
        readonly byte[] _buffer;
        int _usedSize = 0;

        public int FreeSize { get { return _buffer.Length - _usedSize; } }

        public SendBuffer(int bufferSize)
        {
            _buffer = new byte[bufferSize];
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
                _usedSize = 0;
            }

            // return buffer segment from usedSize to usedSize + reserveSize
            return new ArraySegment<byte>(_buffer, _usedSize, reserveSize);
        }

        /// <summary>
        /// Return the used segment actually after calling Reserve.
        /// </summary>
        /// <param name="usedSize">Used buffer size actullay</param>
        /// <returns>The segment of the actually used buffer.</returns>
        public ArraySegment<byte> Return(int usedSize)
        {
            ArraySegment<byte> segment = new ArraySegment<byte>(_buffer, _usedSize, usedSize);
            _usedSize += usedSize;
            return segment;
        }

        public ArraySegment<byte> Use(int size)
        {
            if (size > FreeSize) _usedSize = 0; // TODO : need to be refactored...

            ArraySegment<byte> segment = new ArraySegment<byte>(_buffer, _usedSize, size);
            _usedSize += size;
            return segment;
        }
    }
}

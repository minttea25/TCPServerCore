using System;

namespace ServerCoreTCP
{
    /// <summary>
    /// The buffer for Receive using byte ArraySegment
    /// </summary>
    public class RecvBuffer
    {
        readonly ArraySegment<byte> _buffer;
        /// <summary>
        /// Pointer of reading in the buffer
        /// </summary>
        int _readPtr;
        /// <summary>
        /// Pointer of writing in the buffer
        /// </summary>
        int _writePtr;

        /// <summary>
        /// Get size of written data.
        /// </summary>
        public int DataSize { get { return _writePtr - _readPtr; } }
        /// <summary>
        /// Get writable size of the buffer now.
        /// </summary>
        public int FreeSize { get { return _buffer.Count - _writePtr; } }

        public RecvBuffer(int bufferSize)
        {
            _buffer = new ArraySegment<byte>(new byte[bufferSize], 0, bufferSize);
        }

        /// <summary>
        /// Return the byte segment of the written data.
        /// </summary>
        public ArraySegment<byte> DataSegment
        {
            get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _readPtr, DataSize); }
        }

        /// <summary>
        /// Return the writable byte segment.
        /// </summary>
        public ArraySegment<byte> WriteSegment
        {
            get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _writePtr, FreeSize); }
        }

        /// <summary>
        /// Pulls the data to the front of the buffer and cleans it up. (Free up space of buffer)
        /// </summary>
        public void CleanUp()
        {
            int dataSize = DataSize;

            // If no more data, reset the pointers
            if (dataSize == 0)
            {
                _readPtr = _writePtr = 0;
            }
            // If there is left data, copy the data at index=0
            else
            {
                Array.Copy(_buffer.Array, _buffer.Offset + _readPtr,
                    _buffer.Array, _buffer.Offset, dataSize);

                // reset read pointer to 0
                _readPtr = 0;
                // reset write pointer after the position of the data
                _writePtr = dataSize;
            }
        }

        /// <summary>
        /// Reset the buffer.
        /// </summary>
        public void ClearBuffer()
        {
            _readPtr = _writePtr = 0;
        }

        /// <summary>
        /// Move the read pointer of the recvbuffer.
        /// </summary>
        /// <param name="numOfBytes">The length of bytes to check the data.</param>
        /// <returns>False if the numOfBytes is bigger than current data size; True, otherwise.</returns>
        public bool OnRead(int numOfBytes)
        {
            // If numOfBytes are bigger than DataSize, it's wrong numOfBytes => return false
            if (numOfBytes > DataSize) return false;

            // Move the pointer as much as the read data length
            _readPtr += numOfBytes;
            return true;
        }

        /// <summary>
        /// Move the write pointer of the recvbuffer.
        /// </summary>
        /// <param name="numOfBytes">The length of bytes to check the data.</param>
        /// <returns>False if the numOfBytes is bigger than current-writable size; True, otherwise.</returns>
        public bool OnWrite(int numOfBytes)
        {
            // If there is not enough length to write in buffer, return false
            if (numOfBytes > FreeSize) return false;

            // Move the pointer as much as the written data length
            _writePtr += numOfBytes;
            return true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCoreTCP
{
    /// <summary>
    /// The buffer for Receive using byte Memory
    /// </summary>
    public class MRecvBuffer
    {
        readonly Memory<byte> buffer;
        /// <summary>
        /// Pointer of reading in the buffer
        /// </summary>
        int readPtr;
        /// <summary>
        /// Pointer of writing in the buffer
        /// </summary>
        int writePtr;

        /// <summary>
        /// Get size of written data.
        /// </summary>
        public int DataSize { get { return writePtr - readPtr; } }
        /// <summary>
        /// Get writable size of the buffer now.
        /// </summary>
        public int FreeSize { get { return buffer.Length - writePtr; } }

        public MRecvBuffer(int bufferSize)
        {
            buffer = new byte[bufferSize];
        }

        /// <summary>
        /// Return the byte Memory of the written data.
        /// </summary>
        public Memory<byte> DataMemory
        {
            get { return buffer.Slice(readPtr, DataSize); }
        }

        /// <summary>
        /// Return the writable byte Memory
        /// </summary>
        public Memory<byte> WriteMemory
        {
            get { return buffer[writePtr..]; }
        }

        /// <summary>
        /// Pulls the data to the front of the buffer and cleans it up. (Free up space of buffer)
        /// </summary>
        public void CleanUp()
        {
            int dataSize = DataSize;

            // If no more data, reset the pointers
            if (DataSize == 0)
            {
                readPtr = writePtr = 0;
            }
            // If there is left data, copy the data at index=0
            else
            {
                buffer.Span.Slice(readPtr, dataSize).CopyTo(buffer.Span);

                // reset read pointer to 0
                readPtr = 0;
                // reset write pointer after the position of the data
                writePtr = dataSize;
            }
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
            readPtr += numOfBytes;
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
            writePtr += numOfBytes;
            return true;
        }
    }
}

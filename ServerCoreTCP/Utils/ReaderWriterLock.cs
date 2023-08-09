using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCoreTCP.Utils
{
    /* ReaderWriterLock
     * 
     * This class provides a lock mechanism that allows only one thread to write while multiple threads can read concurrently. 
     * It is recommended to use this class when there is less contention among threads for writing, 
     * and multiple threads are attempting to read. 
     * However, if contention increases, it might lead to performance issues.
     * For better performance, consider using System.Threading.ReaderWriterLockSlim provided by C#.
     * 
     * The _lockFlag is composed as follows:
     * It uses an unsigned 8-byte uint type, not using msb as a sign bit.
     * [writer_thread_lock, 4][reader_thread_count, 4]
     *
     * The writer_thread_lock is assigned to the ID of the thread with write permission (Thread.CurrentThread.ManagedThreadId).
     * The reader_thread_count holds the number of threads currently reading.
     * 
     * The following policies are allowed:
     * 1. Read by another thread while a read is in progress.
     *
     * The following policies are not allowed:
     * 1. Write by another thread while a read is in progress.
     * 2. Read by another thread while a write is in progress.
     * 3. Write by another thread while a write is in progress.
     *
     * Note: Thread.CurrentThread.ManagedThreadId can return negative values. 
     * Here, we assume that this value is always positive and cast it to uint for use.
     * (If threads are managed by ThreadManager with different unique numbers in the future, this will be adjusted accordingly.)
     *
     */

    public class ReaderWriterLock
    {
        public uint WriteCount => _writeCount;

        const uint TIMEOUT_TICK = 10000;
        const uint MAX_SPIN_COUNT = 5000;

        const uint EMPTY_FLAG = 0x00000000;
        const uint WRITE_MASK = 0xFFFF0000;
        const uint READ_COUNT_MASK = 0x0000FFFF;

        uint _writeCount = 0;
        uint _lockFlag = EMPTY_FLAG;

        uint GetLockThreadId => (_lockFlag & WRITE_MASK) >> 16;

        public void WriteLock()
        {
            uint threadId = (uint)Thread.CurrentThread.ManagedThreadId;
            uint lockThreadId = GetLockThreadId;

            if (threadId == lockThreadId)
            {
                _writeCount++;
                return;
            }

            long tick = Environment.TickCount64;

            uint desired = ((threadId << 16) & WRITE_MASK);
            while (true)
            {
                for (uint spinCount = 0; spinCount < MAX_SPIN_COUNT; spinCount++)
                {
                    uint expected = EMPTY_FLAG;
                    if (Interlocked.CompareExchange(ref _lockFlag, desired, expected) == expected)
                    {
                        _writeCount++;
                        return;
                    }
                }

                if (Environment.TickCount64 - tick >= TIMEOUT_TICK)
                {
                    throw new Exception($"WriteLock TIMEOUT: {TIMEOUT_TICK}");
                }

                Thread.Yield();
            }
        }

        public void WriteUnlock()
        {
            if ((_lockFlag & READ_COUNT_MASK) != 0) throw new Exception("Invalid WriteUnlock. WriteUnlock called before WriteLock.");

            if (--_writeCount == 0)
            {
                Interlocked.Exchange(ref _lockFlag, EMPTY_FLAG);
            }
        }

        public void ReadLock()
        {
            uint threadId = (uint)Thread.CurrentThread.ManagedThreadId;
            uint lockThreadId = GetLockThreadId;

            if (threadId == lockThreadId)
            {
                Interlocked.Increment(ref _lockFlag);
                return;
            }

            long tick = Environment.TickCount64;

            while (true)
            {
                for (uint spinCount = 0; spinCount < MAX_SPIN_COUNT; spinCount++)
                {
                    uint expected = _lockFlag & READ_COUNT_MASK;
                    if (Interlocked.CompareExchange(ref _lockFlag, expected + 1, expected) == expected)
                    {
                        return;
                    }
                }

                if (Environment.TickCount64 - tick >= TIMEOUT_TICK)
                {
                    throw new Exception($"ReadLock TIMEOUT: {TIMEOUT_TICK}");
                }

                Thread.Yield();
            }
        }

        public void ReadUnlock()
        {
            // 0 - 1 => 0xFFFFFFFF
            if (Interlocked.Decrement(ref _lockFlag) < 0)
            {
                throw new Exception("Can not ReadUnlock. Any thread is not reading.");
            }
        }
    }
}

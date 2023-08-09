using System.Threading;

namespace ServerCoreTCP.Utils
{
    public class SpinLock
    {
        int _lockFlag = 0;

        public void Lock()
        {
            SpinWait spinWait = new();
            while (Interlocked.CompareExchange(ref _lockFlag, 1, 0) != 0)
            {
                spinWait.SpinOnce();
            }
        }

        public void Lock(ushort maxRetriesBeforeYield = 10000)
        {
            ushort retries = 0;
            SpinWait spinWait = new();

            while (true)
            {
                if (Interlocked.CompareExchange(ref _lockFlag, 1, 0) == 0)
                    return;

                retries++;
                if (retries >= maxRetriesBeforeYield)
                {
                    spinWait.SpinOnce();
                    retries = 0;
                }
            }
        }

        public void UnLock()
        {
            if (_lockFlag == 0) throw new System.Exception("Invalid Order - SpinLock");

            Interlocked.Exchange(ref _lockFlag, 0);
        }
    }
}

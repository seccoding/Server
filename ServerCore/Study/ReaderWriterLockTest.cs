using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore.Study
{
    class ReaderWriterLockTest
    {
        // LockFreeProgramming 와 유사
        volatile int count = 0;
        ReaderWriterLock.RecursiveLock _lock = new ReaderWriterLock.RecursiveLock();

        public void run()
        {
            Task t1 = new Task(() =>
            {
                for(int i = 0; i < 100000; i++)
                {
                    _lock.WriteLock();
                    count++;
                    _lock.WriteUnlock();
                }
            });

            Task t2 = new Task(() =>
            {
                for (int i = 0; i < 100000; i++)
                {
                    _lock.WriteLock();
                    count--;
                    _lock.WriteUnlock();
                }
            });

            t1.Start();
            t2.Start();

            Task.WaitAll(t1, t2);

            Console.WriteLine(count);
        }
    }
}

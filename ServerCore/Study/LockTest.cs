using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore.Study
{
    class LockTest
    {
        object _obj = new object();
        int lockNumber = 0;

        public void run()
        {
            Task t1 = new Task(ThreadLock_1);
            Task t2 = new Task(ThreadLock_2);
            t1.Start();
            t2.Start();

            Task.WaitAll(t1, t2);

            Console.WriteLine(lockNumber);
        }

        private void ThreadLock_1()
        {
            // 10만번 더하기
            for (int i = 0; i < 100000; i++)
            {
                // MonitorLock 보다 더 직관적이고 간편하다.
                // DeadLock 발생할 확률이 줄어준다.
                lock (_obj)
                {
                    lockNumber++;
                }
            }
        }
        private void ThreadLock_2()
        {
            // 10만번 빼기
            for (int i = 0; i < 100000; i++)
            {
                // MonitorLock 보다 더 직관적이고 간편하다.
                // DeadLock 발생할 확률이 줄어준다.
                lock (_obj)
                {
                    lockNumber--;
                }
            }
        }
    }
}

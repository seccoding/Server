using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore.Study
{
    class MutexTest
    {
        int _num = 0;

        // 커널(OS)을 이용한 커널(OS) 동기화 객체. 대표적인 객체
        Mutex _lock = new Mutex();

        // AutoResetEvent 와 유사함.
            // AutoResetEvent는 bool(열림, 닫힘) 정보를 가짐.
        // Mutex는 AutoResetEvent보다 더 많은 정보를 가지고 있음
            // Lock 잡은 횟수를 가지고 있음 그 횟수만큼 Release해야 함/
            // Thread ID를 가지고 있음. 다른 Thread가 Release 못하도록 함.
            // 등등 때문에 추가적인 비용이 많이 듬. => 따라서 AutoResetEvent가 더 효율적임.


        /*
         * 면접에서 자주 나옴
         * Lock이 풀릴 때 까지 대기
         */
        public void run()
        {
            Task t1 = new Task(ThreadSpinLock_1);
            Task t2 = new Task(ThreadSpinLock_2);
            t1.Start();
            t2.Start();

            Task.WaitAll(t1, t2);

            // 왜 0이 안나오나?
            Console.WriteLine(_num);
        }

        void ThreadSpinLock_1()
        {
            for (int i = 0; i < 10000; i++)
            {
                // 입장시도 후 문 닫기
                _lock.WaitOne();
                _num++;
                // 문 열기
                _lock.ReleaseMutex();
            }
        }

        void ThreadSpinLock_2()
        {
            for (int i = 0; i < 10000; i++)
            {
                _lock.WaitOne();
                _num--;
                _lock.ReleaseMutex();
            }
        }
    }
}

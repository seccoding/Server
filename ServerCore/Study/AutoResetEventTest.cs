using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore.Study
{

    class Lock
    {
        // true => 접근 가능
        // false => 접근 불가능
        // 엄청나게 느리다.
        AutoResetEvent _available = new AutoResetEvent(true); // 톨게이트 : 자동으로 닫힘
        //ManualResetEvent _available = new ManualResetEvent(true); // 방 문 : 수동으로 닫아야 함

        public void Acquire()
        {
            // 입장시도 -> 문을 닫음.
            // true -> false
            // false 일 때엔 접근이 불가능
            _available.WaitOne();

            // true를 false로 바꾼다.
            //_available.Reset(); -> AutoResetEvent.WaitOne()에서 자동으로 실행됨.
        }

        public void Release()
        {
            // false를 true로 바꾼다.
            _available.Set();
        }
    }

    class AutoResetEventTest
    {
        int _num = 0;
        Lock _lock = new Lock();

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
                _lock.Acquire();
                _num++;
                _lock.Release();
            }
        }

        void ThreadSpinLock_2()
        {
            for (int i = 0; i < 10000; i++)
            {
                _lock.Acquire();
                _num--;
                _lock.Release();
            }
        }
       
    }
}

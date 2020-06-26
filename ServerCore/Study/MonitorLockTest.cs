using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore.Study
{
    class MonitorLockTest
    {
        int lockNumber = 0;
        object _obj = new object();

        public void run()
        {
            Task t1 = new Task(ThreadMonitorLock_1);
            Task t2 = new Task(ThreadMonitorLock_2);
            t1.Start();
            t2.Start();

            Task.WaitAll(t1, t2);

            Console.WriteLine(lockNumber);
        }

        private void ThreadMonitorLock_1()
        {
            // 10만번 더하기
            for (int i = 0; i < 100000; i++)
            {
                // 상호배제 : Mutual Exclusive
                // Synchronized Start -> 동기처리 시작 (동일 오브젝트에 대해 잠금 아래 코드는 대기한다)
                Monitor.Enter(_obj);
                // Single Thread 처럼 동작
                lockNumber++;

                // ★★ Exit 전에 return을 하게 되면 잠금이 풀리지 않는다. (Dead Lock)
                //if (lockNumber == 10000)
                //{
                //    Monitor.Exit(_obj);
                //    return;
                //}
                // Uncaught RuntimeException이 발생하면????????? DeadLock 발생
                // > try/finally 로 처리해야함 -> 번거로움

                // Synchronized End -> 동기처리 종료 (다른 오브젝트가 시작될 수 있다.)
                Monitor.Exit(_obj);

                // ★★ Monitor Enter / Exit의 범위가 커지면 문제가 생긴다.
                // ★★ Exit 전에 return을 하게 되면 잠금이 풀리지 않는다. (Dead Lock)
            }
        }
        private void ThreadMonitorLock_2()
        {
            // 10만번 빼기
            for (int i = 0; i < 100000; i++)
            {
                // 상호배제 : Mutual Exclusive
                // Synchronized Start -> 동기처리 시작 (동일 오브젝트에 대해 잠금 아래 코드는 대기한다)
                Monitor.Enter(_obj);
                // Single Thread 처럼 동작
                lockNumber--;
                // Synchronized End -> 동기처리 종료 (다른 오브젝트가 시작될 수 있다.)
                Monitor.Exit(_obj);

                // ★★ Monitor Enter / Exit의 범위가 커지면 문제가 생긴다.
                // ★★ Exit 전에 return을 하게 되면 잠금이 풀리지 않는다. (Dead Lock)
            }
        }
    }
}

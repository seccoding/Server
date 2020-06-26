using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore.Study
{
    class SessionManager
    {
        static object _lock = new object();
        public static void TestSession()
        {
            lock (_lock)
            {
                Console.WriteLine("TestSession");
            }
        }

        public static void Test()
        {
            lock (_lock)
            {
                Console.WriteLine("SessionManager.Test");
                UserManager.TestUser();
            }
        }
    }

    class UserManager
    {
        static object _lock = new object();

        public static void Test()
        {
            lock (_lock)
            {
                Console.WriteLine("UserManager.Test");
                SessionManager.TestSession();
            }
        }

        public static void TestUser()
        {
            lock (_lock)
            {
                Console.WriteLine("TestUser");
            }
        }
    }

    class DeadLockTest
    {
        
        public void run()
        {
            Task t1 = new Task(ThreadDeadLock_1);
            Task t2 = new Task(ThreadDeadLock_2);
            t1.Start();

            // 인스턴스 간의 종속적인 메소드 호출이 존재할 경우 DeadLock이 발생할 확률이 높다.
            // Thread 시작시 각 Thread 실행 시점을 다르게 주면 DeadLock이 발생할 확률이 낮아진다.
            Thread.Sleep(100);

            t2.Start();

            Task.WaitAll(t1, t2);

            Console.WriteLine("종료됨");
        }

        private static void ThreadDeadLock_1()
        {
            for (int i = 0; i < 50; i++)
            {
                SessionManager.Test();
            }
        }
        private static void ThreadDeadLock_2()
        {
            for (int i = 0; i < 50; i++)
            {
                UserManager.Test();
            }
        }
    }
}

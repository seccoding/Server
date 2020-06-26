using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore.Study
{
    class ThreadLocalStorageTest
    {
        // Thread 자신의 고유 전역 공간 생성
        static ThreadLocal<string> ThreadName = new ThreadLocal<string>(() => $"My name is {Thread.CurrentThread.ManagedThreadId}");

        void WhoAmI()
        {
            bool repeat = ThreadName.IsValueCreated;
            if(repeat)
                Console.WriteLine(ThreadName.Value + " (repeat)");
            else
                Console.WriteLine(ThreadName.Value);
        }

        public void run()
        {
            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(6, 6);
            // 할당된 Method들을 병행 수행시킨다.
            Parallel.Invoke(WhoAmI, WhoAmI, WhoAmI, WhoAmI, WhoAmI, WhoAmI, WhoAmI, WhoAmI);

            // 필요가 없어지면 날린다.
            ThreadName.Dispose();
        }
    }
}

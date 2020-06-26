using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore.Study
{

    class ThreadTest
    {

        void MainThread(object state)
        {
            for (int i = 0; i < 5; i++)
                Console.WriteLine($"Hello Thread! {state}");
        }

        public void RunThread()
        {
            // 쓰레드가 처리할 작업 단위
            // 쓰레드 풀에서 관리된다.
            // TaskCreationOptions.LongRunning : 엄청 오래 걸린다는 것을 알려준다.
            //      > ThreadPool 에서 별도 관리되어 Pool Size의 제한을 받지 않는다.
            //      > 지정되지 않으면 ThreadPool에서 Pool Size로 관리한다.
            Task t = new Task(() => { while (true) { } }, TaskCreationOptions.LongRunning);
            t.Start();

            /*
             * ThreadPool : 잠깐 사용하고 버린다. 쓰레드 풀로 관리된다. 부하제어가 가능하다.
             *              작업기간이 짧을 떄 사용한다.
             * Thread : 계속 사용할 수 있다. 개수를 늘릴때마다 부하가 증가한다. 제어할 방법이 어렵다.
             *          작업기간이 길 때 사용한다.
             */
            // 쓰레드 최소 개수
            ThreadPool.SetMinThreads(1, 1); // workerThreads = 작업을 처리하는 수
            // 쓰레드 최대 개수 = 쓰레드 풀에 2개 이상의 쓰레드가 존재할 경우 다음 쓰레드는 실행되지 않는다.
            ThreadPool.SetMaxThreads(2, 2); // completionPortThreads = ??

            // 잠깐 사용하고 버릴 쓰레드
            ThreadPool.QueueUserWorkItem(MainThread);
            ThreadPool.QueueUserWorkItem(MainThread, "파라미터1");
            ThreadPool.QueueUserWorkItem(MainThread, "파라미터2");
            ThreadPool.QueueUserWorkItem(MainThread, "파라미터3");
            ThreadPool.QueueUserWorkItem(MainThread, "파라미터4");
            ThreadPool.QueueUserWorkItem(MainThread, "파라미터5", true);

            //Thread t = new Thread(MainThread);
            //t.Name = "Test Thread";
            //t.IsBackground = true; // Background 실행일 경우 Main 이 종료되면 thread도 함께 종료된다.
            //t.Start();
            //Console.WriteLine("Waiting for foreground");

            //t.Join(); // t Thread 작업이 끝나길 기다린다.
            //Console.WriteLine("Hello World!");
        }

    }

    class BasicThread
    {
        public void run()
        {
            ThreadTest t = new ThreadTest();
            t.RunThread();

            while (true)
            {
            }
        }
    }
}

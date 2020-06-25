using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{

    class ThreadTest
    {

        void MainThread(object state)
        {
            for(int i = 0; i < 5; i++)
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

    class Program
    {
        
        static void Main(string[] args)
        {
            //ThreadTestRun_01();
            //TaskRun_02();
            //CacheTest_03();
            HardwareOptimization_04();
        }

        private static void ThreadTestRun_01()
        {
            ThreadTest t = new ThreadTest();
            t.RunThread();

            while (true)
            {
            }
        }

        // 모든 스레드가 동시 접근이 가능하다.
        // volatile : 데이터가 변경되면 즉시 모든 스레드에게 반영된다. => 컴파일러 최적화 제외시킴
        //      > 이거 쓰지마. 안좋대
        private static volatile bool _stop = false;

        private static void ThreadMain()
        {
            Console.WriteLine("스레드 시작!");

            // Debug 모드에서는 정상작동 됨.
            // Release 모드에서는 컴파일러가 코드를 최적화 시켜 오동작 할 경우가 생김.
            while (_stop == false)
            {
                // 누군가가  stop 신호를 해주기를 기다린다.
                //Console.WriteLine(_stop);
            }

            Console.WriteLine("스레드 종료!");
        }

        private static void TaskRun_02()
        {
            Task t = new Task(ThreadMain);
            t.Start();

            Thread.Sleep(1000);
            _stop = true;

            Console.WriteLine("Stop 호출");
            Console.WriteLine("종료 대기중");
            t.Wait(); // 끝날 때까지 기다린다.
            Console.WriteLine("종료 성공");
        }

        /** CPU Cache 테스트 */
        private static void CacheTest_03()
        {
            int[,] arr = new int[10000, 10000];

            {
                // 더 빨리 처리됨 => 순차처리 (Temporal Locality)
                // 순차로 처리되기 때문에 이후 배열 인덱스를 캐시로 가지고 있는다.
                // 결국 캐시 값을 업데이트 하게 된다.
                long now = DateTime.Now.Ticks;
                for (int y = 0; y < 10000; y++)
                    for (int x = 0; x < 10000; x++)
                        arr[y, x] = 1;
                long end = DateTime.Now.Ticks;
                Console.WriteLine($"(y, x) 순서 걸린시간 {(end-now) / 1000}");
            }

            {
                // 더 늦게 처리됨. => 비순차처리 (Temporal Locality, Special Locality 모두 아님)
                // 순차가 아니기 때문에 캐시에 넣어 놓더라도 활용할 수가 없다.
                long now = DateTime.Now.Ticks;
                for (int y = 0; y < 10000; y++)
                    for (int x = 0; x < 10000; x++)
                        arr[x, y] = 1;
                long end = DateTime.Now.Ticks;
                Console.WriteLine($"(x, y) 순서 걸린시간 {(end - now) / 1000}");
            }
        }
        
        static int x = 0;
        static int y = 0;
        static int r1 = 0;
        static int r2 = 0;
        
        /*
         * 메모리 배리어
         * 1) 코드의 재배치 억체
         * 2) 가시성
         * 
         * ASM : 어셈블리어
         * 1) Full Memory Barrier (ASM MFENCE, C# : Thread.MemoryBarrier) : Store(할당)/Load(참조) 둘다 막는다.
         * 2) Store Memory Barrier (ASM SFENCE) : Store만 막는다.
         * 3) Load Memory Barrier (ASM LFENCE) : Load만 막는다.
         */
        private static void HardwareOptimization_04()
        {
            int count = 0;
            
            while (true)
            {
                count += 1;
                x = y = r1 = r2 = 0;

                Task t1 = new Task(HardwareOptimizationThread_1);
                Task t2 = new Task(HardwareOptimizationThread_2);
                t1.Start();
                t2.Start();

                Task.WaitAll(t1, t2);

                /*
                 * r1, r2 가 어떻게 0이 되나?
                 * CPU가 나열된 코드들간의 연관성이 없다고 판단한경우 속도 향상을 위해 코드의 순서를 뒤바꿔버린다.
                 */
                if (r1 == 0 && r2 == 0)
                    break;
            }
            Console.WriteLine($"{count}번 만에 빠져나옴!");
        }

        private static void HardwareOptimizationThread_1()
        {
            y = 1; // Store y
            Thread.MemoryBarrier(); // 코드의 재배치 억제 -> 메모리 Commit (Cache -> Memory)
            r1 = x; // Load x
        }
        private static void HardwareOptimizationThread_2()
        {
            x = 1; // Store x
            Thread.MemoryBarrier(); // 코드의 재배치 억제 -> 메모리 Commit (Cache -> Memory)
            r2 = y; // Load y
        }


    }
}

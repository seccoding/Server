using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore.Study
{
    class HardwareOptimization
    {

        int x = 0;
        int y = 0;
        int r1 = 0;
        int r2 = 0;

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
        public void run()
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

        private void HardwareOptimizationThread_1()
        {
            y = 1; // Store y
            Thread.MemoryBarrier(); // 코드의 재배치 억제 -> 메모리 Commit (Cache -> Memory)
            r1 = x; // Load x
        }

        private void HardwareOptimizationThread_2()
        {
            x = 1; // Store x
            Thread.MemoryBarrier(); // 코드의 재배치 억제 -> 메모리 Commit (Cache -> Memory)
            r2 = y; // Load y
        }

    }
}

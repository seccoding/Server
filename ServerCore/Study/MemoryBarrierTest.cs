using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore.Study
{
    class MemoryBarrierTest
    {

        int _answer;
        bool _complate;

        public void run()
        {
            Task t1 = new Task(MemoryBarrierA);
            Task t2 = new Task(MemoryBarrierB);
            t1.Start();
            t2.Start();

            Task.WaitAll(t1, t2);

            Console.WriteLine("종료됨");
        }

        private void MemoryBarrierA()
        {
            _answer = 123; // Store
            // Store Barrier;
            Thread.MemoryBarrier(); // Barrier 1
            _complate = true; // Store
            // Store Barrier;
            Thread.MemoryBarrier(); // Barrier 2
        }

        private void MemoryBarrierB()
        {
            // Load Barrier
            Thread.MemoryBarrier(); // Barrier 3
            if (_complate)
            {
                // Load Barrier
                Thread.MemoryBarrier(); // Barrier 4
                Console.WriteLine(_answer);
            }
        }

    }
}

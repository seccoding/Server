using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore.Study
{
    class InterlockedTest
    {
        int number = 0;

        /**
         * Race Condition (경합조건)
         * Thread 들이 경합(선점)을 하며 공유된 자원을 각자 처리하게 되는 현상
         *  Thread 우선순위에 따라 결과가 달라질 수 있다.
         */
        public void run()
        {
            Task t1 = new Task(Thread_1);
            Task t2 = new Task(Thread_2);
            t1.Start();
            t2.Start();

            Task.WaitAll(t1, t2);

            /*
            왜 0이 안나오지?
            number++;
                > Compiler 가 이해하는 코드 -> 여러 Thread가 동시 처리 할 경우 문제가 발생된다. -> Atomic 결여
                > int temp = number;
                > temp += 1;
                > number = temp;
                >> Interlocked.Increment(ref number);
                >> Interlocked.Decrement(ref number);
                >> 이제 0이 나온다.

                Interlocked 이 실행 중이면 다른 Interlocked는 실행이 대기상태로 된다.
                A(Interlocked) : 실행중 -> B(Interlocked) : 대기중
                A(Interlocked) : 종료됨 -> B(Interlocked) : 실행중
            */
            Console.WriteLine(number);

        }

        private void Thread_1()
        {
            // 10만번 더하기
            for (int i = 0; i < 100000; i++)
            {
                // number++;
                int afterValue = Interlocked.Increment(ref number); // 원자성을 보장한채로 1증가, 연산 비용이 비싸다
                //int temp = number;
                //temp += 1;
                //number = temp;
            }
        }
        private void Thread_2()
        {
            // 10만번 빼기
            for (int i = 0; i < 100000; i++)
            {
                //number--;
                int afterValue = Interlocked.Decrement(ref number); // 원자성을 보장한채로 1감소, 연산 비용이 비싸다
                //int temp = number;
                //temp -= 1;
                //number = temp;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore.Study
{

    class SpinLockTest
    {
        class SpinLock
        {
            // Lock 상태
            int _locked = 0;

            // Lock
            public void Acquire()
            {
                while (true)
                {
                    // 잠김이 풀리기를 기다린다.
                    /*
                     * int original = _locked;
                     * locked = 1;
                     * return original;
                     * 
                     * 잘 안씀 -> 현재 상태에 대한 값 비교가 없음.
                     *           강제적인 변경이 필요할 때 사용.
                     */
                    //int original = Interlocked.Exchange(ref _locked, 1);

                    /*
                     * CAS (Compare-And-Swap)
                     * 
                     * CompareExchange를 많이 쓴다.
                     * 
                     * int original = _locked;
                     * if (_locked == 0)
                     *     locked = 1;
                     * return original;
                     */
                    int expected = 0;
                    int desired = 1;
                    if (Interlocked.CompareExchange(ref _locked, desired, expected) == expected)
                        break;

                    // 휴식을 가지지 않으면 SpinLock.
                    // 휴식을 가지면 Context Switching?

                    // 무조건 기다리는것 X
                    // 적절히 휴식 O
                    /*
                    Thread.Sleep(1); // 무조건 휴식, 1ms 쉼.
                    Thread.Sleep(0); // 조건부 휴식, 현재 쓰레드보다 우선순위가 같거나 높은 쓰레드에게 양보. 
                                     // 없다면 다시 가져옴
                                     // [Thread Instance].Priority = ThreadPriority.Highest;
                                     // ThreadPriority
                                         // Highest	    4 Thread는 다른 우선 순위가 할당된 스레드 앞에 예약할 수 있습니다.
                                         // AboveNormal	3 Thread는 우선 순위가 Highest인 스레드 뒤와 우선 순위가 Normal인 스레드 앞에 예약할 수 있습니다.
                                         // Normal	    2 Thread는 우선 순위가 AboveNormal인 스레드 뒤와 우선 순위가 BelowNormal인 스레드 앞에 예약할 수 있습니다. 
                                                       // 스레드에는 기본적으로 Normal 우선 순위가 할당됩니다.
                                         // BelowNormal	1 Thread는 우선 순위가 Normal인 스레드 뒤와 우선 순위가 Lowest인 스레드 앞에 예약할 수 있습니다.
                                         // Lowest	    0 Thread는 다른 우선 순위가 할당된 스레드 뒤에 예약할 수 있습니다.
                    */
                    Thread.Yield(); // 관대한 양보. 현재 실행가능한 쓰레드가 있다면 실행. 없다면, 남은 시간 소진
                }

            }

            // Release
            public void Release()
            {
                _locked = 0;
            }
        }

        int _num = 0;
        SpinLock _lock = new SpinLock();

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
            for (int i = 0; i < 100000; i++)
            {
                _lock.Acquire();
                _num++;
                _lock.Release();
            }
        }
        void ThreadSpinLock_2()
        {
            for (int i = 0; i < 100000; i++)
            {
                _lock.Acquire();
                _num--;
                _lock.Release();
            }
        }

    }
}

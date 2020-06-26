using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore.Study
{
    class BasicTask
    {

        // 모든 스레드가 동시 접근이 가능하다.
        // volatile : 데이터가 변경되면 즉시 모든 스레드에게 반영된다. => 컴파일러 최적화 제외시킴
        //      > 이거 쓰지마. 안좋대
        private volatile bool _stop = false;

        private void ThreadMain()
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

        public void run()
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

    }
}

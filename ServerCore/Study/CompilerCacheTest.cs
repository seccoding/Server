using System;
using System.Collections.Generic;
using System.Text;

namespace ServerCore.Study
{
    class CompilerCacheTest
    {

        /** CPU Cache 테스트 */
        public void run()
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
                Console.WriteLine($"(y, x) 순서 걸린시간 {(end - now) / 1000}");
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

    }
}

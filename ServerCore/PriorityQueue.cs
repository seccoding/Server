using System;
using System.Collections.Generic;
using System.Text;

namespace ServerCore
{
    public class PriorityQueue<T> where T : IComparable<T>
    {
        // 2진 힙트리
        List<T> _heap = new List<T>();

        public int Count { get { return _heap.Count; } }

        public void Push(T data)
        {
            // 힙의 맨 끝에 새로운 데이터를 삽입한다.
            _heap.Add(data);

            int now = _heap.Count - 1;
            int next;
            T temp;
            // 정렬
            while (now > 0)
            {
                next = (now - 1) / 2; // 부모노드
                if (_heap[now].CompareTo(_heap[next]) < 0) // 부모노드가 더 크면 정렬 중단.
                    break;

                // 두 값을 교체
                temp = _heap[now];
                _heap[now] = _heap[next];
                _heap[next] = temp;

                // 검사 위치를 이동한다.
                now = next;
            }
        }

        public T Pop()
        {
            // 반환 데이터를 따로 저장
            T ret = _heap[0];

            // 마지막 데이터를 루트로 이동한다.
            int lastIndex = _heap.Count - 1;
            _heap[0] = _heap[lastIndex];

            // 마지막 노드 제거
            _heap.RemoveAt(lastIndex);
            lastIndex--;

            // 역으로 내려가는 정렬 시작
            int now = 0;
            int left, right, next;
            T temp;
            while (true)
            {
                left = (2 * now) + 1;
                right = (2 * now) + 2;

                next = now;
                // 왼쪽값이 현재 값 보다 크면, 왼쪽으로 이동
                if (left <= lastIndex && _heap[next].CompareTo(_heap[left]) < 0)
                    next = left;

                // 오른쪽값이 현재 값 보다 크면, 오른쪽으로 이동
                if (right <= lastIndex && _heap[next].CompareTo(_heap[right]) < 0)
                    next = right;

                // 왼쪽/오른쪽 모두 현재 값 보다 작으면 종료
                if (next == now)
                    break;

                temp = _heap[now];
                _heap[now] = _heap[next];
                _heap[next] = temp;

                // 검사 위치 이동
                now = next;
            }

            return ret;
        }

        public T Peek()
        {
            if (_heap.Count == 0)
                return default(T);
            return _heap[0];
        }
    }
}

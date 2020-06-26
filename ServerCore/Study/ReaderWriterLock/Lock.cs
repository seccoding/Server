using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ServerCore.Study.ReaderWriterLock
{
    // Deny Recursive Lock
    // SpinLock Policy (Count 5000 -> Yield)
    class Lock
    {
        const int EMPTY_FLAG = 0x00000000; // 0000 0000 0000 0000 0000 0000 0000 0000
        const int WRITE_MASK = 0x7FFF0000; // 0111 1111 1111 1111 0000 0000 0000 0000
        const int READ_MASK = 0x0000FFFF;  // 0000 0000 0000 0000 1111 1111 1111 1111
        const int MAX_SPIN_COUNT = 5000;

        // [Unused(1)] 
        // [WriterThreadId(15)] Lock을 건 ThreadId를 기록
        // [ReadCount(16)] Read한 Count를 기록
        int _flag = EMPTY_FLAG;

        public void WriteLock()
        {
            // ManagedThreadId 는 1부터 증가하는 값.
            // WriteMask 영역에 넣기 위해 16비트만큼 Right Shift.
            // Unused(1) 영역을 뒤집기 위해 WRITE_MASK And Operation
            int desired = (Thread.CurrentThread.ManagedThreadId << 16) & WRITE_MASK;
            // 아무도 WriteLock or ReadLock을 획득하고 있지 않을 때 경합해서 소유권을 얻는다.
            while (true)
            {
                for (int i = 0; i < MAX_SPIN_COUNT; i++)
                {
                    //시도를 해서 성공하면 return
                    if (Interlocked.CompareExchange(ref _flag, desired, EMPTY_FLAG) == EMPTY_FLAG)
                        return;
                }

                // 소유권을 얻는데 실패했다면 소유권을 양보한다.
                Thread.Yield();
            }
        }

        // WriteLock 을 했던 Thread만 Release 할 수 있다.
        public void WriteUnlock()
        {
            Interlocked.Exchange(ref _flag, EMPTY_FLAG);
        }

        // 여러 Thread가 ReadLock을 잡는다.
        public void ReadLock()
        {
            // 아무도 WriteLock을 갖고 있지 않다면 ReadCount를 1 늘린다.
            while (true)
            {
                for (int i = 0; i < MAX_SPIN_COUNT; i++)
                {
                    int expected = (_flag & READ_MASK);
                    if (Interlocked.CompareExchange(ref _flag, expected + 1, expected) == expected)
                        return;
                }

                // 소유권을 얻지 못한다면 양보한다.
                Thread.Yield();
            }
        }

        public void ReadUnlock()
        {
            Interlocked.Decrement(ref _flag);
        }

    }

    /*
     * WriteLock -> WriteLock OK
     * WriteLock -> ReadLock OK
     * ReadLock -> WriteLock NO
     */
    class RecursiveLock
    {
        const int EMPTY_FLAG = 0x00000000; // 0000 0000 0000 0000 0000 0000 0000 0000
        const int WRITE_MASK = 0x7FFF0000; // 0111 1111 1111 1111 0000 0000 0000 0000
        const int READ_MASK = 0x0000FFFF;  // 0000 0000 0000 0000 1111 1111 1111 1111
        const int MAX_SPIN_COUNT = 5000;

        // [Unused(1)] 
        // [WriterThreadId(15)] Lock을 건 ThreadId를 기록
        // [ReadCount(16)] Read한 Count를 기록
        int _flag = EMPTY_FLAG;

        int _writeCount = 0;

        public void WriteLock()
        {
            // 동일 쓰레드가 WriteLock을 이미 획득하고 있는지 확인
            int lockThreadId = (_flag & WRITE_MASK) >> 16;

            if ( Thread.CurrentThread.ManagedThreadId == lockThreadId )
            {
                _writeCount++;
                return;
            }

            int desired = (Thread.CurrentThread.ManagedThreadId << 16) & WRITE_MASK;
            while (true)
            {
                for (int i = 0; i < MAX_SPIN_COUNT; i++)
                {
                    if (Interlocked.CompareExchange(ref _flag, desired, EMPTY_FLAG) == EMPTY_FLAG)
                    {
                        _writeCount = 1;
                        return;
                    }
                }

                Thread.Yield();
            }
        }

        public void WriteUnlock()
        {
            int lockCount = --_writeCount;
            if (lockCount == 0)
                Interlocked.Exchange(ref _flag, EMPTY_FLAG);
        }

        public void ReadLock()
        {
            int lockThreadId = (_flag & WRITE_MASK) >> 16;

            if (Thread.CurrentThread.ManagedThreadId == lockThreadId)
            {
                Interlocked.Increment(ref _flag);
                return;
            }

            while (true)
            {
                for (int i = 0; i < MAX_SPIN_COUNT; i++)
                {
                    int expected = (_flag & READ_MASK);
                    if (Interlocked.CompareExchange(ref _flag, expected + 1, expected) == expected)
                        return;
                }

                Thread.Yield();
            }
        }

        public void ReadUnlock()
        {
            Interlocked.Decrement(ref _flag);
        }
    }
}

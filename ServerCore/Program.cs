using System;

namespace ServerCore
{

    
    class Program
    {

        static void Main(string[] args)
        {
            //new Study.BasicThread().run();
            //new Study.BasicTask().run();
            //new Study.CompilerCacheTest().run();
            //new Study.HardwareOptimization().run();
            //new Study.MemoryBarrierTest().run();
            //new Study.InterlockedTest().run();
            //new Study.MonitorLockTest().run();
            //new Study.LockTest().run();
            //new Study.DeadLockTest().run();
            //new Study.SpinLockTest().run();
            //new Study.AutoResetEventTest().run();
            //new Study.MutexTest().run();
            new Study.ReaderWriterLockTest().run();
        }



    }
}

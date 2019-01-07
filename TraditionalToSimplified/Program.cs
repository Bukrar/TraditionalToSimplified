using System;
using System.Diagnostics;

namespace TraditionalToSimplified
{
    class Program
    {
        static void Main(string[] args)
        {
            TraditionalToSimplified traditionalToSimplified = new TraditionalToSimplified();
            //計時
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            //執行修改繁體資料庫資料至簡體資料庫(繁體BIG5轉GB18030)
            traditionalToSimplified.DbDataHandle();
           
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                                 ts.Hours, ts.Minutes, ts.Seconds,
                                 ts.Milliseconds / 10);
            Console.WriteLine("RunTime " + elapsedTime);
            Console.ReadLine();
        }
    }
}



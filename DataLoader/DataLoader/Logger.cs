using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StockAnalyzer
{
    public class Logger
    {
        static ReaderWriterLock locker = new ReaderWriterLock();
        public static void LogError(string message)
        {
            try
            {
                locker.AcquireWriterLock(int.MaxValue);
                File.AppendAllText("StockAnalysisError.log", message);
            }
            finally
            {
                locker.ReleaseWriterLock();
            }
        }
    }
}

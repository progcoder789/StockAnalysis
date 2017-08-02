using System.IO;
using System.Threading;

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

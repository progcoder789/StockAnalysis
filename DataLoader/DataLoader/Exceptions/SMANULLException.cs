using System;

namespace StockAnalyzer.Exceptions
{
    public class SMANULLException : Exception
    {
        public override string Message
        {
            get
            {
                return "SMA is Null";
            }
        }
    }
}

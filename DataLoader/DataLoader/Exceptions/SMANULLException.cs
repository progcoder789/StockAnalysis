using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalyzer
{
    public class StockData
    {
        public int date;
        public decimal? open;
        public decimal? high;
        public decimal? low;
        public decimal? close;
        public decimal? unadjclose;
        public int? volume;
    }

    public class SymbolData
    {
        public int Id;
        public string Symbol;
        public string Name;
        public decimal? Volume;
    }
}

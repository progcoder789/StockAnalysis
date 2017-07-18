using System;
using System.Data;

namespace StockAnalyzer.CandleStick
{
    public class HammerHang : AnalyzeMethods
    {
        private string _name = "HammerHangCS";
        private string _description = "";
        private AnalysisCommon.TrendPeriod _trendPeriod = AnalysisCommon.TrendPeriod.Short;
        private decimal multiplier1 = 2.2m;
        private decimal multiplier2 = 0.002m;

        public override string Description
        {
            get
            {
                return _description;
            }
        }

        public override string Name
        {
            get
            {
                return _name;
            }
        }

        public override AnalysisCommon.TrendPeriod TrendPeriod
        {
            get
            {
                return _trendPeriod;
            }
        }

        public override bool Qualified(DataRowCollection rows, int index)
        {
            decimal open, close, high, low;
            if (!TryGetPriceValues(rows[index], out open, out close, out high, out low))
                return false;

            var top = close;
            var bottom = open;

            if (close < open)
            {
                bottom = close;
                top = open;
            }

            //下影线必须超过实体的2.2倍
            if (Math.Abs(bottom - low) < Math.Abs(top - bottom) * multiplier1)
            {
                return false;
            }

            //上影线必须很短
            if (high - top > top * multiplier2)
                return false;

            return true;
        }

        public override bool Valid(DataRowCollection rows, int index)
        {
            return base.TurnOver(rows, index);
        }
    }
}

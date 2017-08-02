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

        public override bool Qualified(DataRowCollection rows, int index, StockData currentPrice, StockData beforePrice, StockData afterPrice)
        {
            if (currentPrice == null)
                return false;

            var top = currentPrice.close.Value;
            var bottom = currentPrice.open.Value;

            if (currentPrice.close < currentPrice.open)
            {
                bottom = currentPrice.close.Value;
                top = currentPrice.open.Value;
            }

            //下影线必须超过实体的2.2倍
            if (Math.Abs(bottom - currentPrice.low.Value) < Math.Abs(top - bottom) * multiplier1 ||
                Math.Abs(bottom - currentPrice.low.Value) == 0 ||
                Math.Abs(top - bottom) == 0)
            {
                return false;
            }

            //上影线必须很短
            if (currentPrice.high.Value - top > top * multiplier2)
                return false;

            return true;
        }

        public override bool Valid(DataRowCollection rows, int index)
        {
            return base.TurnOver(rows, index);
        }
    }
}

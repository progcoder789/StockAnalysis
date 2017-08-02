using System.Data;

namespace StockAnalyzer.CandleStick
{
    public class Cover : AnalyzeMethods
    {
        private string _name = "CoverCS";
        private string _description = "";
        private AnalysisCommon.TrendPeriod _trendPeriod = AnalysisCommon.TrendPeriod.Short;

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
            if (currentPrice == null || beforePrice == null)
                return false;

            if (!BeforeHasTrend(rows, index))
                return false;

            var previousTop = beforePrice.close > beforePrice.open ? beforePrice.close : beforePrice.open;
            var previousBottom = beforePrice.close > beforePrice.open ? beforePrice.open : beforePrice.close;

            var currentTop = currentPrice.close > currentPrice.open ? currentPrice.close : currentPrice.open;
            var currentBottom = beforePrice.close > beforePrice.open ? beforePrice.open : beforePrice.close;

            //current top must be greater than previous top
            if (currentTop < previousTop)
                return false;
            //current bottom must be less than previous bottom
            if (currentBottom > previousBottom)
                return false;

            //current price direction must be opposite to previous
            if ((currentPrice.open - currentPrice.close) * (beforePrice.open - beforePrice.close) > 0)
                return false;

            return true;
        }

        public override bool Valid(DataRowCollection rows, int index)
        {
            return base.TurnOver(rows, index);
        }
    }
}

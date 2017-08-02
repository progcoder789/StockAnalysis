using System.Data;

namespace StockAnalyzer.CandleStick
{
    public class Star : AnalyzeMethods
    {
        private string _name = "Star";
        private string _description = "Used to identify ascending turn to descending";
        private AnalysisCommon.TrendPeriod _trendPeriod = AnalysisCommon.TrendPeriod.Long;
        private decimal multiplier = 0.005m;
        private decimal multiplier2 = 0.6m;

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

            //Before trend must be Down
            if (AnalysisCommon.CheckBeforeTrendDirection(rows, index, _trendPeriod) != AnalysisCommon.TrendDirection.Down)
                return false;

            //Previous day must be down
            if (beforePrice.open < beforePrice.close)
                return false;
            //Current day must be up
            if (currentPrice.open > currentPrice.close)
                return false;

            //Current Open must be less than previous close
            if (currentPrice.open > beforePrice.close)
                return false;

            //Current close must be greater than previous close 
            if (currentPrice.close < beforePrice.close)
                return false;

            //Shadow line must be very short for previous
            if ((beforePrice.high - beforePrice.close) > (beforePrice.high - beforePrice.low) * multiplier)
                return false;

            if ((beforePrice.open - beforePrice.low) > (beforePrice.high - beforePrice.low) * multiplier)
                return false;

            //Shadow line must be very short for current
            if ((currentPrice.high - currentPrice.open) > (currentPrice.high - currentPrice.low) * multiplier)
                return false;

            if ((currentPrice.close - currentPrice.low) > (currentPrice.high - currentPrice.low) * multiplier)
                return false;

            //current close must be in body of previous bar for more than 60%
            if ((currentPrice.close - beforePrice.close) < (beforePrice.open - beforePrice.close) * multiplier2)
                return false;

            return true;
        }

        public override bool Valid(DataRowCollection rows, int index)
        {
            if (AnalysisCommon.CheckAfterTrendDirection(rows, index, AnalysisCommon.TrendPeriod.Short) != AnalysisCommon.TrendDirection.Up)
                return false;

            return true;
        }
    }
}

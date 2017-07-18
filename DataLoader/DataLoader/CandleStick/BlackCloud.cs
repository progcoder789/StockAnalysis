using System;
using System.Data;

namespace StockAnalyzer.CandleStick
{
    public class BlackCloud : AnalyzeMethods
    {
        private string _name = "BlackCloudCS";
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

        public override bool Qualified(DataRowCollection rows, int index)
        {
            if (!BeforeHasTrend(rows, index))
                return false;

            //Before trend must be UP
            if (AnalysisCommon.CheckBeforeTrendDirection(rows, index, _trendPeriod) != AnalysisCommon.TrendDirection.Up)
                return false;

            decimal currentOpen, currentClose, previousOpen, previousClose;
            decimal currentHigh, currentLow, previousHigh, previousLow;

            if (!TryGetPriceValues(rows[index], out currentOpen, out currentClose, out currentHigh, out currentLow))
                return false;

            if (!TryGetPriceValues(rows[index - 1], out previousOpen, out previousClose, out previousHigh, out previousLow))
                return false;

            //Previous day must be up
            if (previousClose < previousOpen)
                return false;
            //Current day must be down
            if (currentClose > currentOpen)
                return false;

            //Current Open must be greater than previous close
            if (currentOpen < previousClose)
                return false;

            //Current close must be greater than previous open 
            if (currentClose < previousOpen)
                return false;

            //Shadow line must be very short for previous
            if ((previousHigh - previousClose) > (previousHigh - previousLow) * multiplier)
                return false;

            if ((previousOpen - previousLow) > (previousHigh - previousLow) * multiplier)
                return false;

            //Shadow line must be very short for current
            if ((currentHigh - currentOpen) > (currentHigh - currentLow) * multiplier)
                return false;

            if ((currentClose - currentLow) > (currentHigh - currentLow) * multiplier)
                return false;

            //current close must be in body of previous bar for more than 60%
            if ((previousClose - currentClose) < (previousClose - previousOpen) * multiplier2)
                return false;

            return true;
        }

        public override bool Valid(DataRowCollection rows, int index)
        {
            if (AnalysisCommon.CheckAfterTrendDirection(rows, index, AnalysisCommon.TrendPeriod.Short) != AnalysisCommon.TrendDirection.Down)
                return false;

            return true;
        }
    }
}

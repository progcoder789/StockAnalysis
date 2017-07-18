using System.Data;

namespace StockAnalyzer.CandleStick
{
    public class Penetration : AnalyzeMethods
    {
        private string _name = "PenetrationCS";
        private string _description = "Used to identify descending turn to ascending, opposite to blackcloud.";
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

            //Before trend must be Down
            if (AnalysisCommon.CheckBeforeTrendDirection(rows, index, _trendPeriod) != AnalysisCommon.TrendDirection.Down)
                return false;

            decimal currentOpen, currentClose, previousOpen, previousClose;
            decimal currentHigh, currentLow, previousHigh, previousLow;

            if (!TryGetPriceValues(rows[index], out currentOpen, out currentClose, out currentHigh, out currentLow))
                return false;

            if (!TryGetPriceValues(rows[index - 1], out previousOpen, out previousClose, out previousHigh, out previousLow))
                return false;

            //Previous day must be down
            if (previousOpen < previousClose)
                return false;
            //Current day must be up
            if (currentOpen > currentClose)
                return false;

            //Current Open must be less than previous close
            if (currentOpen > previousClose)
                return false;

            //Current close must be greater than previous close 
            if (currentClose < previousClose)
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
            if ((currentClose - previousClose) < (previousOpen - previousClose) * multiplier2)
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

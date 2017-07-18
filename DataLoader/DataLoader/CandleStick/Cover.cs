using System;
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

        public override bool Qualified(DataRowCollection rows, int index)
        {
            if (!BeforeHasTrend(rows, index))
                return false;

            decimal currentOpen, currentClose, previousOpen, previousClose;
            decimal currentHigh, currentLow, previousHigh, previousLow;

            if (!TryGetPriceValues(rows[index], out currentOpen, out currentClose, out currentHigh, out currentLow))
                return false;

            if (!TryGetPriceValues(rows[index - 1], out previousOpen, out previousClose, out previousHigh, out previousLow))
                return false;

            var previousTop = previousClose > previousOpen ? previousClose : previousOpen;
            var previousBottom = previousClose > previousOpen ? previousOpen : previousClose;

            var currentTop = currentClose > currentOpen ? currentClose : currentOpen;
            var currentBottom = previousClose > previousOpen ? previousOpen : previousClose;

            //current top must be greater than previous top
            if (currentTop < previousTop)
                return false;
            //current bottom must be less than previous bottom
            if (currentBottom > previousBottom)
                return false;

            //current price direct must be opposite to previous
            if ((currentOpen - currentClose) * (previousOpen - previousClose) > 0)
                return false;

            return true;
        }

        public override bool Valid(DataRowCollection rows, int index)
        {
            return base.TurnOver(rows, index);
        }
    }
}

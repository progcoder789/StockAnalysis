using System;
using System.Data;

namespace StockAnalyzer.CandleStick
{
    public class BlackCloud : AnalyzeMethods
    {
        private string _name = "CoverCS";
        private string _description = "";

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

        private bool hasTrend(DataRowCollection rows, int index)
        {
            if (!AnalysisCommon.CheckTrendPeriod(index, rows.Count))
                return false;
            var trendDirect = AnalysisCommon.CheckTrendDirection(rows, index - AnalysisCommon.TrendShortPeriod, index);
            if (trendDirect == AnalysisCommon.TrendDirection.None)
                return false;

            return true;
        }

        public override bool Qualified(DataRowCollection rows, int index)
        {
            if (!hasTrend(rows, index))
                return false;
            decimal currentOpen, currentClose, previousOpen, previousClose;

            try
            {
                currentOpen = Convert.ToDecimal(rows[index][Common.OpenColumn]);
                currentClose = Convert.ToDecimal(rows[index][Common.CloseColumn]);
                previousOpen = Convert.ToDecimal(rows[index - 1][Common.OpenColumn]);
                previousClose = Convert.ToDecimal(rows[index][Common.CloseColumn]);
            }
            catch (InvalidCastException ex)
            {
                // one of the data is not decimal(could be DBNull)
                return false;
            }
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

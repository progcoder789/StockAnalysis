using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalyzer.CandleStick
{
    public class HammerHang : AnalyzeMethods
    {
        private string _name = "HammerHangCS";
        private string _description = "";
        private decimal multiplier1 = 1.9m;
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
        public override bool Qualified(DataRowCollection rows, int index)
        {
            var dr = rows[index];
            if (dr[Common.CloseColumn] == DBNull.Value ||
               dr[Common.OpenColumn] == DBNull.Value ||
                dr[Common.LowColumn] == DBNull.Value ||
                dr[Common.HighColumn] == DBNull.Value)
            {
                return false;
            }

            var close = Convert.ToDecimal(dr[Common.CloseColumn]);
            var open = Convert.ToDecimal(dr[Common.OpenColumn]);
            var low = Convert.ToDecimal(dr[Common.LowColumn]);
            var high = Convert.ToDecimal(dr[Common.HighColumn]);

            var top = close;
            var bottom = open;

            if (close < open)
            {
                bottom = close;
                top = open;
            }

            //下影线必须超过实体的1.8倍
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

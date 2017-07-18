﻿using StockAnalyzer.Exceptions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalyzer.CandleStick
{
    public abstract class AnalyzeMethods
    {
        public abstract string Name { get; }
        public abstract string Description { get; }

        public abstract AnalysisCommon.TrendPeriod TrendPeriod { get; }

        public abstract bool Qualified(DataRowCollection rows, int index);
        public abstract bool Valid(DataRowCollection rows, int index);

        public virtual async Task<bool> Analyze(DataTable dtPrices, DataTable rootTable)
        {

            var rows = dtPrices.Rows;
            if (rows.Count == 0)
                return false;

            string symbolId = rows[0][Common.SymbolIdColumn].ToString();
            var table = AnalysisCommon.MakeAnalysisResultsTable();

            Console.WriteLine(string.Format("Analyzing Symbol {0} with method {1}", symbolId, Name));

            for (int i = 0; i < rows.Count; i++)
            {
                bool isQualified = false;
                bool isValid = false;
                if (Qualified(rows, i))
                {
                    isQualified = true;
                    isValid = Valid(rows, i);
                    var newRow = table.NewRow();
                    newRow[Common.SymbolIdColumn] = symbolId;
                    newRow[Common.MethodNameColumn] = Name;
                    newRow[Common.DateColumn] = rows[i][Common.DateColumn].ToString();
                    newRow[Common.QualifiedColumn] = Convert.ToInt32(isQualified).ToString();
                    newRow[Common.ValidColumn] = Convert.ToInt32(isValid).ToString();
                    table.Rows.Add(newRow);
                }
            }

            if (table.Rows.Count > 0)
            {
                Console.WriteLine(string.Format("Write {0} rows to DB for symbol {1} and method {2}", table.Rows.Count, symbolId, Name));
                var ret = await SqlExecutor.BulkCopy(table);
            }

            //AnalysisCommon.MergeAnalysisResultTable(rootTable, table);
            return true;
        }

        public virtual bool TurnOver(DataRowCollection rows, int index)
        {
            if (!AnalysisCommon.CheckTrendPeriod(index, rows.Count, TrendPeriod))
                return false;

            AnalysisCommon.TrendDirection before = AnalysisCommon.TrendDirection.None;
            AnalysisCommon.TrendDirection after = AnalysisCommon.TrendDirection.None;

            try
            {
                before = AnalysisCommon.CheckBeforeTrendDirection(rows, index, TrendPeriod);
                after = AnalysisCommon.CheckAfterTrendDirection(rows, index, TrendPeriod);
            }
            catch (Exception ex)
            {
                if (ex is SMANULLException)
                    return false;
            }

           if((before == AnalysisCommon.TrendDirection.Down && after == AnalysisCommon.TrendDirection.Up)
               || (before == AnalysisCommon.TrendDirection.Up && after == AnalysisCommon.TrendDirection.Down))
            {
                return true;
            }

            return false;
        }

        public virtual bool TryGetPriceValues(DataRow dr, out decimal open, out decimal close, out decimal high, out decimal low)
        {
            open = close = high = low = 0;
            try
            {
                open = Convert.ToDecimal(dr[Common.OpenColumn]);
                close = Convert.ToDecimal(dr[Common.CloseColumn]);
                high = Convert.ToDecimal(dr[Common.HighColumn]);
                low = Convert.ToDecimal(dr[Common.LowColumn]);
            }
            catch (InvalidCastException ex)
            {
                // one of the data is not decimal(could be DBNull)
                return false;
            }

            return true;
        }

        public virtual bool BeforeHasTrend(DataRowCollection rows, int index)
        {
            if (!AnalysisCommon.CheckTrendPeriod(index, rows.Count, TrendPeriod))
                return false;

            var trendDirect = AnalysisCommon.CheckBeforeTrendDirection(rows, index, TrendPeriod);
            if (trendDirect == AnalysisCommon.TrendDirection.None)
                return false;

            return true;
        }
    }
}

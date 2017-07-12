﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalyzer.CandleStick
{
    public class HammerHang : IAnalyzerMethods
    {
        private string _name = "HammerHangCandleStick";
        private string _description = "";
        private decimal multiplier1 = 2.5m;
        private decimal multiplier2 = 0.002m;
        private bool Qualified(DataRow dr)
        {
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

            if (close < open)
                top = open;

            //下影线必须超过实体的2.5倍
            if (Math.Abs(high - low) < Math.Abs(open - close) * multiplier1)
            {
                return false;
            }

            //上影线必须很短
            if (high - top > top * multiplier2)
                return false;

            return true;
        }

        private bool Valid(DataRowCollection rows, int index)
        {
            if (!AnalysisCommon.CheckTrend(index, rows.Count))
                return false;

            bool isBeforeUp = false;
            bool isAfterUp = false;


            try
            {
                AnalysisCommon.CheckTrendBeforeAfter(index, rows, out isBeforeUp, out isAfterUp);
            }
            catch (Exception ex)
            {
                if (ex.Message == "SMA is null")
                    return false;
            }

            return (isBeforeUp ^ isAfterUp);
        }

        public string Description
        {
            get
            {
                return _description;
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
        }

        public async Task<bool> Run(DataTable dtPrices, DataTable rootTable)
        {
            //string pathToScriptFile = @"DBScripts\AnalysisTables\InsertAnalysisResults.sql";
            //string insertAnalysisResultsSqlScript = File.ReadAllText(Common.GetScriptPath(pathToScriptFile));

            string pathToScriptFile = @"DBScripts\AnalysisTables\InsertAnalysisStatistic.sql";
            string insertAnalysisStatisticSqlScript = File.ReadAllText(Common.GetScriptPath(pathToScriptFile));

            int TotalQualified, TotalValid, Last1YearQualified, Last1YearValid,
                Last3YearQualified, Last3YearValid, Last6YearQualified, Last6YearValid,
                Last10YearQualified, Last10YearValid;

            TotalQualified = TotalValid = Last1YearQualified = Last1YearValid =
            Last3YearQualified = Last3YearValid = Last6YearQualified = Last6YearValid =
            Last10YearQualified = Last10YearValid = 0;

            var rows = dtPrices.Rows;
            if (rows.Count == 0)
                return false;

            string symbolId = rows[0][Common.SymbolIdColumn].ToString();
            var table = AnalysisCommon.MakeAnalysisResultsTable();
            Console.WriteLine(string.Format("Analyzing Symbol {0} with method {1}", symbolId, _name));
            for (int i = 0; i < rows.Count; i++)
            {
                bool isQualified = false;
                bool isValid = false;
                if (Qualified(rows[i]))
                {
                    TotalQualified++;
                    isQualified = true;

                    if (Valid(rows, i))
                    {
                        isValid = true;
                        TotalValid++;
                    }

                    AnalysisCommon.CheckValidPeriod(isValid, isQualified, Convert.ToDateTime(rows[i][Common.DateColumn]),
                                                    ref Last1YearQualified, ref Last1YearValid,
                                                    ref Last3YearQualified, ref Last3YearValid,
                                                    ref Last6YearQualified, ref Last6YearValid,
                                                    ref Last10YearQualified, ref Last10YearValid);
                    var newRow = table.NewRow();
                    newRow[Common.SymbolIdColumn] = symbolId;
                    newRow[Common.MethodNameColumn] = _name;
                    newRow[Common.DateColumn] = rows[i][Common.DateColumn].ToString();
                    newRow[Common.QualifiedColumn] = Convert.ToInt32(isQualified).ToString();
                    newRow[Common.ValidColumn] = Convert.ToInt32(isValid).ToString();
                    table.Rows.Add(newRow);
                }
            }

            var parames = new List<KeyValuePair<string, string>>();
            parames.Add(new KeyValuePair<string, string>(Common.MethodNameColumn, _name));
            parames.Add(new KeyValuePair<string, string>(Common.SymbolIdColumn, symbolId));
            parames.Add(new KeyValuePair<string, string>(Common.TotalValidColumn, TotalValid.ToString()));
            parames.Add(new KeyValuePair<string, string>(Common.TotalQualifiedColumn, TotalQualified.ToString()));
            parames.Add(new KeyValuePair<string, string>(Common.Last1YearValidColumn, Last1YearValid.ToString()));
            parames.Add(new KeyValuePair<string, string>(Common.Last1YearQualifiedColumn, Last1YearQualified.ToString()));
            parames.Add(new KeyValuePair<string, string>(Common.Last3YearValidColumn, Last3YearValid.ToString()));
            parames.Add(new KeyValuePair<string, string>(Common.Last3YearQualifiedColumn, Last3YearQualified.ToString()));
            parames.Add(new KeyValuePair<string, string>(Common.Last6YearValidColumn, Last6YearValid.ToString()));
            parames.Add(new KeyValuePair<string, string>(Common.Last6YearQualifiedColumn, Last6YearQualified.ToString()));
            parames.Add(new KeyValuePair<string, string>(Common.Last10YearQualifiedColumn, Last10YearQualified.ToString()));
            parames.Add(new KeyValuePair<string, string>(Common.Last10YearValidColumn, Last10YearValid.ToString()));

            SqlExecutor.ExecuteQuery(insertAnalysisStatisticSqlScript, parames);

            var ret = await SqlExecutor.BulkCopy(table);
            //AnalysisCommon.MergeAnalysisResultTable(rootTable, table);
            return true;
        }
    }
}
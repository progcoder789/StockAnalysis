using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;

namespace StockAnalyzer
{
    public class SMACalculator
    {
        public static void CalculateSMA(int period, int index, DataRow[] rows)
        {
            string smaColumnName = Common.SMAColumnPrefix + period.ToString();

            decimal sum = 0;
            if (index + 2 > period)
            {
                for (int i = index - period + 1; i < index + 1; i++)
                {
                    var value = rows[i][Common.CloseColumn];
                    if (value == DBNull.Value)
                        return;
                    sum = sum + Convert.ToDecimal(rows[i][Common.CloseColumn]);
                }
                rows[index][smaColumnName] = sum / period;
            }
        }

        public static async Task<bool> Run()
        {
            var symbols = SqlExecutor.GetSymbols();

            var pathToScriptFile = @"DBScripts\StockDataTables\SelectStockBySymbol.sql";
            var sqlScript = File.ReadAllText(Common.GetScriptPath(pathToScriptFile));
            DataTable dt;
            foreach (var symbol in symbols)
            {
                var paramCollection = new List<KeyValuePair<string, string>>();
                paramCollection.Add(new KeyValuePair<string, string>(Common.SymbolIdColumn, symbol.Id.ToString()));
                sqlScript = sqlScript.Replace(Common.TableNameOld, symbol.Symbol[0].ToString() + Common.TableNameSuffix);
                dt = StockDataLoader.MakeStockTable(symbol.Symbol[0].ToString() + Common.TableNameSuffix);
                SqlExecutor.ExecuteQueryFillDataTable(sqlScript, paramCollection, dt);
                Console.WriteLine(string.Format("Calculate SMA for Symbol:{0}", symbol.Symbol));

                ////for (int i = 0; i < dt.Rows.Count; i++)
                ////{
                ////    CalculateSMA(5, i, dt);
                ////    CalculateSMA(10, i, dt);
                ////    CalculateSMA(30, i, dt);
                ////    CalculateSMA(60, i, dt);
                ////}
                dt.AcceptChanges();
                var ret = await SqlExecutor.BulkCopy(dt);
            }
            return true;
        }
    }
}

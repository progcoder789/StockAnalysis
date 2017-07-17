using StockAnalyzer.CandleStick;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StockAnalyzer
{
    public class AnalyzerCoordinator
    {
        private static DataTable rootTable = AnalysisCommon.MakeAnalysisResultsTable();
        private static Type parentType = typeof(AnalyzeMethods);
        private static string pathToStockBySymbolScriptFile = @"DBScripts\AnalysisTables\SelectStockBySymbol.sql";
        private static string selectStockBySymbolSqlScript = File.ReadAllText(Common.GetScriptPath(pathToStockBySymbolScriptFile));

        private static string pathToStatisticScriptFile = @"DBScripts\AnalysisTables\SelectStatistic.sql";
        private static string selectStatisticSqlScript = File.ReadAllText(Common.GetScriptPath(pathToStatisticScriptFile));

        private static string pathToInsertAnalysisStatisticScriptFile = @"DBScripts\AnalysisTables\InsertAnalysisStatistic.sql";
        private static string insertAnalysisStatisticSqlScript = File.ReadAllText(Common.GetScriptPath(pathToInsertAnalysisStatisticScriptFile));

        private static Type[] GetAllTypes()
        {
            var types = (from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
                         from assemblyType in domainAssembly.GetTypes()
                         where typeof(AnalyzeMethods).IsAssignableFrom(assemblyType)
                         select assemblyType).ToArray();

            return types;
        }

        private static DataTable GetPrices(int symbolId, string symbol, int updatePeriod)
        {
            var date = Convert.ToDateTime(ConfigurationManager.AppSettings["StartTime"]);
            if (updatePeriod > 0)
            {
                date = DateTime.Now.Subtract(TimeSpan.FromDays(updatePeriod + 1));
            }
            var paramCollection = new List<KeyValuePair<string, string>>();

            paramCollection.Add(new KeyValuePair<string, string>(Common.SymbolIdColumn, symbolId.ToString()));
            paramCollection.Add(new KeyValuePair<string, string>(Common.DateColumn, date.ToString()));

            var newSqlScript = selectStockBySymbolSqlScript.Replace(Common.TableNameOld, symbol[0].ToString() + Common.TableNameSuffix);
            var dt = StockDataLoader.MakeStockTable(symbol[0].ToString() + Common.TableNameSuffix);

            SqlExecutor.ExecuteQueryFillDataTable(newSqlScript, paramCollection, dt);
            return dt;
        }

        private static void Analyze(IEnumerable<Type> allAnalyzers, List<SymbolData> symbols, int updatePeriod)
        {
            List<Task> tasks = new List<Task>();

            foreach (var symbol in symbols)
            {
                var prices = GetPrices(symbol.Id, symbol.Symbol, updatePeriod);

                foreach (var myType in allAnalyzers)
                {
                    if (myType != parentType)
                    {
                        var obj = (AnalyzeMethods)Activator.CreateInstance(myType);
                        Task t = Task.Run(async () => await obj.Analyze(prices, rootTable));
                        tasks.Add(t);
                    }
                }
            }

            if (tasks.Count > 0)
                Task.WaitAll(tasks.ToArray());

            //var ret1 = SqlExecutor.BulkCopy(rootTable).Result;
            var ret2 = SqlExecutor.ExecuteStoredProcedure("TransferAnalysisResults").Result;
        }

        private static DataTable GetResults(int symbolId, string methodName)
        {
            var paramCollection = new List<KeyValuePair<string, string>>();

            paramCollection.Add(new KeyValuePair<string, string>(Common.SymbolIdColumn, symbolId.ToString()));
            paramCollection.Add(new KeyValuePair<string, string>(Common.MethodNameColumn, methodName));

            var dt = AnalysisCommon.MakeAnalysisResultsTable();

            SqlExecutor.ExecuteQueryFillDataTable(selectStatisticSqlScript, paramCollection, dt);
            return dt;
        }

        private static async Task<bool> CalculateStatistic(int symbolId, string methodName)
        {
            int TotalQualified, TotalValid, Last1YearQualified, Last1YearValid,
       Last3YearQualified, Last3YearValid, Last6YearQualified, Last6YearValid,
       Last10YearQualified, Last10YearValid;

            TotalQualified = TotalValid = Last1YearQualified = Last1YearValid =
            Last3YearQualified = Last3YearValid = Last6YearQualified = Last6YearValid =
            Last10YearQualified = Last10YearValid = 0;
            //var statisticTable = AnalysisCommon.MakeAnalysisStatisticTable();

            var resultTable = GetResults(symbolId, methodName);
            if (resultTable.Rows.Count == 0)
                return false;

            Console.WriteLine("Calculate Statistic for Symbol {0} and Method {1}", symbolId, methodName);
            int count = resultTable.Rows.Count;

            for (int i = 0; i < count; i++)
            {
                bool isValid = false;
                bool isQualified = false;

                if (Convert.ToBoolean(resultTable.Rows[i][Common.ValidColumn]))
                {
                    isValid = true;
                    TotalValid++;
                }

                if (Convert.ToBoolean(resultTable.Rows[i][Common.QualifiedColumn]))
                {
                    isQualified = true;
                    TotalQualified++;
                }

                AnalysisCommon.CheckValidPeriod(isValid, isQualified, Convert.ToDateTime(resultTable.Rows[i][Common.DateColumn]),
                                    ref Last1YearQualified, ref Last1YearValid,
                                    ref Last3YearQualified, ref Last3YearValid,
                                    ref Last6YearQualified, ref Last6YearValid,
                                    ref Last10YearQualified, ref Last10YearValid);

            }

            var parames = new List<KeyValuePair<string, string>>();
            parames.Add(new KeyValuePair<string, string>(Common.MethodNameColumn, methodName));
            parames.Add(new KeyValuePair<string, string>(Common.SymbolIdColumn, symbolId.ToString()));
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

            return true;
        }

        private static void Consolidate(List<SymbolData> symbols, IEnumerable<Type> allAnalyzers)
        {
            List<Task> tasks = new List<Task>();

            foreach (var symbol in symbols)
            {
                foreach (var analyzerType in allAnalyzers)
                {
                    if (analyzerType != parentType)
                    {
                        var obj = (AnalyzeMethods)Activator.CreateInstance(analyzerType);
                        Task t = Task.Run(async () => await CalculateStatistic(symbol.Id, obj.Name));
                        tasks.Add(t);
                    }
                }

            }

            if (tasks.Count > 0)
                Task.WaitAll(tasks.ToArray());
        }

        public static void RunAnalysis(int updatePeriod = 0)
        {
            var types = GetAllTypes();
            var symbols = SqlExecutor.GetSymbols();

            Analyze(types, symbols, updatePeriod);

            Consolidate(symbols, types);
        }
    }
}

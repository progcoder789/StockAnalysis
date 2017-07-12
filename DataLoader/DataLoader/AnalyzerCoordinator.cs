using StockAnalyzer.CandleStick;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalyzer
{
    public class AnalyzerCoordinator
    {
        private static DataTable rootTable = AnalysisCommon.MakeAnalysisResultsTable();
        public static void RunAnalysis()
        {
            var type = typeof(IAnalyzerMethods);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p));
            var symbols = SqlExecutor.GetSymbols();

            List<Task> tasks = new List<Task>();

            foreach (var symbol in symbols)
            {
                var prices = AnalysisCommon.GetPrices(symbol.Id, symbol.Symbol);

                foreach (var myType in types)
                {
                    if (myType != type)
                    {
                        var obj = (IAnalyzerMethods)Activator.CreateInstance(myType);
                        Task t = Task.Run(async () => await obj.Run(prices, rootTable));
                        tasks.Add(t);
                    }
                }
            }

            if (tasks.Count > 0)
                Task.WaitAll(tasks.ToArray());

            //var ret1 = SqlExecutor.BulkCopy(rootTable).Result;
            var ret2 = SqlExecutor.ExecuteStoredProcedure("TransferAnalysisResults").Result;
        }
    }
}

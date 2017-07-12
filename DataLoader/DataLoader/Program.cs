using CommandLine;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalyzer
{
    class Options
    {
        // omitting long name, default --verbose
        [Option('r', DefaultValue = false,
          HelpText = "re-run all models")]
        public bool ReRun { get; set; }

        // omitting long name, default --verbose
        [Option('u', DefaultValue = false,
          HelpText = "Update Data")]
        public bool Update { get; set; }

        // omitting long name, default --verbose
        [Option('l', DefaultValue = false,
          HelpText = "Load Symbols and Prices only")]
        public bool LoadStockData { get; set; }

        // omitting long name, default --verbose
        [Option('y', DefaultValue = false,
          HelpText = "Analysis Stock")]
        public bool Analysis { get; set; }

        // omitting long name, default --verbose
        [Option('t', DefaultValue = false,
          HelpText = "Send Alerts")]
        public bool Alert { get; set; }

    }

    class Program
    {
        static void Main(string[] args)
        {
            DateTime now = DateTime.Now;
            Console.WriteLine(string.Format("Start at: {0}", now.ToString()));
            var options = new Options();
            var parser = new Parser();
            var updatePeriod = Convert.ToInt32(ConfigurationManager.AppSettings["UpdatePeriod"]);

            if (parser.ParseArguments(args, options))
            {
                if (options.ReRun)
                {
                    //asdasdas
                    DBInitializer.DropAllTables();
                    DBInitializer.CreateAllTables();
                    SymbolLoader.LoadSymbols();
                    var ret = StockDataLoader.LoadStockPrice(0).Result;
                    AnalyzerCoordinator.RunAnalysis();
                }

                if (options.Update)
                {
                    DBInitializer.InitialzeForLoadOnly();
                    SymbolLoader.LoadSymbols();
                    var ret = StockDataLoader.LoadStockPrice(updatePeriod).Result;
                    AnalyzerCoordinator.RunAnalysis();
                }

                if (options.LoadStockData)
                {
                    DBInitializer.InitialzeForLoadOnly();
                    var ret = StockDataLoader.LoadStockPrice(0).Result;
                }

                if (options.Analysis )
                {
                    DBInitializer.InitialzeForAnalysisOnly();
                    AnalyzerCoordinator.RunAnalysis();
                }

                if (options.Alert)
                {

                }
            }

            double totalMins = DateTime.Now.Subtract(now).TotalMinutes;
            Console.WriteLine(string.Format("End at: {0}. Total Minutes {1}", DateTime.Now.ToString(), totalMins.ToString()));
            Console.ReadKey();
        }
    }
}

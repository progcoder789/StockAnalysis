using System.IO;

namespace StockAnalyzer
{
    public class DBInitializer
    {
        public static void CreateAllTables()
        {
            string pathToScriptFile, sqlScript;
            //create symbol table and staging symbol table
            SqlExecutor.ExecuteScriptFile(@"DBScripts\StockDataTables\DropStagingSymbolTable.sql");
            SqlExecutor.ExecuteScriptFile(@"DBScripts\StockDataTables\CreateStockSymbolsTable.sql");
            InitialzeForLoadOnly();
            //create stock table for each letter
            pathToScriptFile = @"DBScripts\StockDataTables\CreateStocksTable.sql";
            sqlScript = File.ReadAllText(Common.GetScriptPath(pathToScriptFile));

            for (int i = 0; i < 26; i++)
            {
                int stockFirstChar = 'A' + i;
                var tableName = char.ConvertFromUtf32(stockFirstChar) + Common.TableNameSuffix;
                var newSqlScript = sqlScript.Replace(Common.TableNameOld, tableName);
                SqlExecutor.ExecuteQuery(newSqlScript);
            }


            //Create Analysis alerts table
            SqlExecutor.ExecuteScriptFile(@"DBScripts\AnalysisTables\CreateAnalysisAlertsTable.sql");

            InitialzeForAnalysisOnly();


        }

        public static void DropAllTables()
        {
            SqlExecutor.ExecuteScriptFile(@"DBScripts\StockDataTables\DropAllTables.sql");
        }

        public static void InitialzeForLoadOnly()
        {
            //recreate staging table for all stocks
            //Drop all stocks and staging symbol tables
            SqlExecutor.ExecuteScriptFile(@"DBScripts\StockDataTables\DropAllStocksTable.sql");
            //Create transfer data stored procedure
            SqlExecutor.ExecuteScriptFile(@"DBScripts\StockDataTables\CreateTransferDataSP.sql");
            SqlExecutor.ExecuteScriptFile(@"DBScripts\StockDataTables\CreateAllStocksTable.sql");
            return;
        }

        public static void InitialzeForAnalysisOnly()
        {
            //Drop all analysis and staging symbol tables
            SqlExecutor.ExecuteScriptFile(@"DBScripts\AnalysisTables\DropAnalysisTables.sql");
            //Create Analysis result table
            SqlExecutor.ExecuteScriptFile(@"DBScripts\AnalysisTables\CreateAnalysisResultsTable.sql");
            //Create Analysis statistic table
            SqlExecutor.ExecuteScriptFile(@"DBScripts\AnalysisTables\CreateAnalysisStatisticTable.sql");
            //Create Analysis result Transfer stored procedure
            SqlExecutor.ExecuteScriptFile(@"DBScripts\AnalysisTables\CreateTransferSP.sql");
            return;
        }
    }
}

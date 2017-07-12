using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using System.IO;
using System.Data;
using System.Threading.Tasks;

namespace StockAnalyzer
{
    public class StockDataLoader
    {
        private static string urlTemplate = "https://finance.yahoo.com/quote/{0}/history?period1={1}&period2={2}&interval=1d&filter=history&frequency=1d";
        //private static string urlTemplate2 = "https://query1.finance.yahoo.com/v7/finance/download/{0}?period1={1}&period2={2}&interval=1d&events=history&crumb=y2QxxlT3dco";
        private static string startSearch = "\"HistoricalPriceStore\":";
        private static DataTable rootTable = MakeStockTable("[dbo].[AllStocks]");
        public static DataTable MakeStockTable(string tableName)
        {
            DataTable table = new DataTable(tableName);
            Common.MakeIdentityColumn(Common.IdColumn, "System.Int32", table);
            Common.MakeNormalColumn(Common.SymbolIdColumn, "System.Int32", table);
            Common.MakeNormalColumn(Common.DateColumn, "System.DateTime", table);
            Common.MakeNormalColumn(Common.CloseColumn, "System.Decimal", table);
            Common.MakeNormalColumn(Common.OpenColumn, "System.Decimal", table);
            Common.MakeNormalColumn(Common.HighColumn, "System.Decimal", table);
            Common.MakeNormalColumn(Common.LowColumn, "System.Decimal", table);
            Common.MakeNormalColumn(Common.TradeVolumeColumn, "System.Int32", table);
            Common.MakeNormalColumn(Common.UnAdjustCloseColumn, "System.Decimal", table);
            Common.MakeNormalColumn(Common.SMA5Column, "System.Decimal", table);
            Common.MakeNormalColumn(Common.SMA10Column, "System.Decimal", table);
            Common.MakeNormalColumn(Common.SMA30Column, "System.Decimal", table);
            Common.MakeNormalColumn(Common.SMA60Column, "System.Decimal", table);
            Common.MakeNormalColumn(Common.TrendColumn, "System.Decimal", table);

            // Make the ID column the primary key column.
            DataColumn[] PrimaryKeyColumns = new DataColumn[1];
            PrimaryKeyColumns[0] = table.Columns[Common.IdColumn];
            table.PrimaryKey = PrimaryKeyColumns;

            UniqueConstraint custUnique = new UniqueConstraint(new DataColumn[] { table.Columns[Common.SymbolIdColumn],
                                           table.Columns[Common.DateColumn] });

            table.Constraints.Add(custUnique);
            return table;
        }

        private static async Task<bool> LoadData(string symbol, int symbolId, string startTime, string endTime)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                var url = string.Format(urlTemplate, symbol, startTime, endTime);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);

                Console.WriteLine("Send Request for Symbol {0}", symbol);
                var body = await DownloadPage(url);
                Console.WriteLine("Get Response Back for Symbol {0}", symbol);

                // get stock historical data
                int startIndex = body.IndexOf(startSearch);
                int endIndex = body.IndexOf("\"isPending\":");
                int notFoundIndex = body.IndexOf("Symbols similar to");
                if ((startIndex < 1 || endIndex < 1 || startIndex > endIndex) && notFoundIndex > 0)
                    return false;
                startIndex = startIndex + startSearch.Length + 10;
                var data = body.Substring(startIndex, endIndex - startIndex - 1);
                IEnumerable<StockData> prices = JsonConvert.DeserializeObject<IEnumerable<StockData>>(data);
                var priceList = prices.ToList();
                //sort the price based on date
                priceList.Sort((x, y) => x.date.CompareTo(y.date));

                var table = MakeStockTable("[dbo].[AllStocks]");
                int index = 0;
                foreach (var price in priceList)
                {
                    try
                    {

                        var row = table.NewRow();
                        var date = (new DateTime(1970, 1, 1)).AddSeconds(price.date);
                        row[Common.SymbolIdColumn] = symbolId;
                        row[Common.DateColumn] = date.Date;

                        if (price.open.HasValue)
                            row[Common.OpenColumn] = price.open;
                        else
                            row[Common.OpenColumn] = DBNull.Value;

                        if (price.close.HasValue)
                            row[Common.CloseColumn] = price.close;
                        else
                            row[Common.CloseColumn] = DBNull.Value;

                        if (price.high.HasValue)
                            row[Common.HighColumn] = price.high;
                        else
                            row[Common.HighColumn] = DBNull.Value;

                        if (price.low.HasValue)
                            row[Common.LowColumn] = price.low;
                        else
                            row[Common.LowColumn] = DBNull.Value;

                        if (price.unadjclose.HasValue)
                            row[Common.UnAdjustCloseColumn] = price.unadjclose;
                        else
                            row[Common.UnAdjustCloseColumn] = DBNull.Value;

                        if (price.volume.HasValue)
                            row[Common.TradeVolumeColumn] = price.volume;
                        else
                            row[Common.TradeVolumeColumn] = DBNull.Value;

                        table.Rows.Add(row);
                        var rows = table.Select("Close is not null");
                        SMACalculator.CalculateSMA(5, index, rows);
                        SMACalculator.CalculateSMA(10, index, rows);
                        SMACalculator.CalculateSMA(30, index, rows);
                        SMACalculator.CalculateSMA(60, index, rows);
                        index++;
                    }
                    catch (Exception ex)
                    {
                        //benign error 
                    }
                }
                await SqlExecutor.BulkCopy(table);
                //Common.MergeTable(rootTable, table);

            }
            catch (Exception ex)
            {
                Logger.LogError(string.Format("Symbol:{0}. \n\n\n Exception: {1}", symbol, ex.ToString()));
            }

            return true;
        }

        private static async Task<string> DownloadPage(string url)
        {
            using (var client = new HttpClient())
            {
                using (var r = await client.GetAsync(new Uri(url)))
                {
                    string result = await r.Content.ReadAsStringAsync();
                    r.EnsureSuccessStatusCode();
                    return result;
                }
            }
        }

        public static async Task<bool> LoadStockPrice(int startDays)
        {
            var symbols = SqlExecutor.GetSymbols();
            var start = Convert.ToDateTime(ConfigurationManager.AppSettings["StartTime"]);
            //override for update case
            if (startDays > 0)
                start = DateTime.Now.Subtract(TimeSpan.FromDays(startDays));

            var endTime = (DateTime.Now.Date.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            var startTime = (start.Date.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            symbols.Sort((x, y) => x.Symbol.CompareTo(y.Symbol));
            int batchSize = Convert.ToInt32(ConfigurationManager.AppSettings["BatchSize"]); ;
            int failedRequest = 0;

            for (int i = 0; i < symbols.Count; i = i + batchSize)
            //for(int i = 0; i < 200; i = i + batchSize)
            {
                List<Task<bool>> tasks = new List<Task<bool>>();

                for (int j = 0; j < batchSize; j++)
                {
                    if (i + j < symbols.Count)
                    {
                        try
                        {
                            var symbol = symbols[i + j].Symbol;
                            var symbolId = symbols[i + j].Id;
                            Task<bool> t = Task.Run(async () => await LoadData(symbol, symbolId, startTime.ToString(), endTime.ToString()));
                            tasks.Add(t);
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex.ToString());
                        }
                    }
                }

                if (tasks.Count > 0)
                    Task.WaitAll(tasks.ToArray());
                foreach (var task in tasks)
                {
                    if (!task.Result)
                        failedRequest++;
                }
            }

            //await SqlExecutor.BulkCopy(rootTable);
            await SqlExecutor.ExecuteStoredProcedure("TransferData");
            Console.WriteLine(string.Format("Failed Request {0}", failedRequest));
            return true;
        }
    }
}

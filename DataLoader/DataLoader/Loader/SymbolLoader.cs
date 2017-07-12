using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalyzer
{
    public class SymbolLoader
    {
        private static DataTable MakeSymbolTable()
        {
            DataTable table = new DataTable("[dbo].[StagingStockSymbols]");
            Common.MakeIdentityColumn(Common.IdColumn, "System.Int32", table);
            Common.MakeNormalColumn(Common.SymbolColumn, "System.String", table);
            Common.MakeNormalColumn(Common.NameColumn, "System.String", table);
            Common.MakeNormalColumn(Common.VolumeColumn, "System.Decimal", table);

            // Make the ID column the primary key column.
            DataColumn[] PrimaryKeyColumns = new DataColumn[1];
            PrimaryKeyColumns[0] = table.Columns[Common.IdColumn];
            table.PrimaryKey = PrimaryKeyColumns;

            return table;
        }

        public static void LoadSymbols()
        {
            var path = @"Input\companylist.csv";
            using (var reader = new StreamReader(Common.GetScriptPath(path)))
            {
                //read off the column row
                reader.ReadLine();

                decimal price = 1;
                decimal marketCap = 0;
                var table = MakeSymbolTable();

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine().Replace("\"","");
                    var values = line.Split(';');
                    //var parameters = new List<KeyValuePair<string, string>>();
                    //parameters.Add(new KeyValuePair<string, string>(Common.SymbolColumn, values[0].Trim()));
                    //parameters.Add(new KeyValuePair<string, string>(Common.NameColumn, string.Empty));

                    if (!Decimal.TryParse(values[2], out price))
                        price = 1;
                    if (!Decimal.TryParse(values[3], out marketCap))
                        marketCap = 0;

                    //parameters.Add(new KeyValuePair<string, string>(Common.VolumeColumn, string.Empty));
                    //SqlExecutor.ExecuteQuery(sqlScript, parameters);
                    var row = table.NewRow();
                    row[Common.SymbolColumn] = values[0].Trim();
                    row[Common.NameColumn] = values[1].Trim();
                    row[Common.VolumeColumn] = (marketCap / price).ToString();
                    table.Rows.Add(row);
                }

                var result = SqlExecutor.BulkCopy(table).Result;
                result = SqlExecutor.ExecuteStoredProcedure("TransferSymbols").Result;
            }
        }
    }
}

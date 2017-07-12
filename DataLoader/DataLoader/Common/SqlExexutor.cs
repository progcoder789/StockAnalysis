using System.Data.SqlClient;
using System.IO;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System.Collections.Generic;
using System;
using System.Data;
using System.Threading.Tasks;

namespace StockAnalyzer
{
    public class SqlExecutor
    {
        public static void ExecuteScriptFile(string filePath)
        {
            var sqlScript = File.ReadAllText(Common.GetScriptPath(filePath));
            SqlExecutor.ExecuteQuery(sqlScript);
        }

        public static void ExecuteQuery(string query)
        {
            using (var connection = new SqlConnection(Common.ConnectionString))
            {
                var server = new Server(new ServerConnection(connection));
                server.ConnectionContext.ExecuteNonQuery(query);
            }
        }

        public static void ExecuteQuery(string query, List<KeyValuePair<string, string>> parameters, DataTable dataTable)
        {
            using (var connection = new SqlConnection(Common.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue("@" + param.Key, param.Value);
                    }
                    connection.Open();
                    using (SqlDataAdapter da = new SqlDataAdapter(command))
                    {
                        // this will query your database and return the result to your datatable
                        da.Fill(dataTable);
                    }
                }
            }
        }

        public static void ExecuteQuery(string query, List<KeyValuePair<string, string>> parameters)
        {
            using (var connection = new SqlConnection(Common.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue("@" + param.Key, param.Value);
                    }
                    connection.Open();
                    int result = command.ExecuteNonQuery();
                }
            }
        }

        public static Dictionary<string, List<string>> ExecuteReader(string query)
        {
            var values = new Dictionary<string, List<string>>();

            using (var connection = new SqlConnection(Common.ConnectionString))
            {
                var server = new Server(new ServerConnection(connection));
                SqlDataReader reader = server.ConnectionContext.ExecuteReader(query);

                while (reader.Read())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        if (!values.ContainsKey(reader.GetName(i)))
                        {
                            values.Add(reader.GetName(i), new List<string>());
                        }

                        var valueList = values[reader.GetName(i)];
                        valueList.Add(reader[i].ToString());
                    }
                }
            }

            return values;
        }

        public static List<SymbolData> GetSymbols()
        {
            var pathToScriptFile = @"DBScripts\StockDataTables\SelectSymbols.sql";
            var sqlScript = File.ReadAllText(Common.GetScriptPath(pathToScriptFile));
            var symbols = SqlExecutor.ExecuteReader(sqlScript);

            var symbolList = symbols[Common.SymbolColumn];
            var symbolsObjList = new List<SymbolData>();
            for (int i = 0; i < symbolList.Count; i++)
            {
                var symbolObj = new SymbolData();
                symbolObj.Id = Convert.ToInt32(symbols[Common.IdColumn][i]);
                symbolObj.Symbol = symbols[Common.SymbolColumn][i];
                symbolObj.Name = symbols[Common.NameColumn][i];
                symbolObj.Volume = Convert.ToDecimal(symbols[Common.VolumeColumn][i]);
                symbolsObjList.Add(symbolObj);
            }

            return symbolsObjList;
        }

        public static async Task<bool> BulkCopy(DataTable dataTable)
        {
            bool done = false;
            int i = 0;

            while (!done)
            {
                try
                {
                    using (var bulkCopy = new SqlBulkCopy(Common.ConnectionString, SqlBulkCopyOptions.KeepNulls & SqlBulkCopyOptions.KeepIdentity))
                    {
                        // Set the timeout.
                        bulkCopy.BulkCopyTimeout = 60;
                        bulkCopy.BatchSize = dataTable.Rows.Count;
                        bulkCopy.DestinationTableName = dataTable.TableName;
                        await bulkCopy.WriteToServerAsync(dataTable);
                    }

                    done = true;
                }
                catch (Exception ex)
                {
                    i++;
                    if (i > 3)
                        throw ex;
                }
            }
            return true;
        }

        public static async Task<bool> ExecuteStoredProcedure(string storedProcedureName)
        {
            using (var conn = new SqlConnection(Common.ConnectionString))
            {
                using (var command = new SqlCommand(storedProcedureName, conn))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = 600;
                    conn.Open();
                    await command.ExecuteNonQueryAsync();
                }
            }
            return true;
        }
    }
}

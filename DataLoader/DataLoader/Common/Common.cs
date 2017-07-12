using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StockAnalyzer
{
    public class Common
    {
        private static ReaderWriterLock stockTableLocker = new ReaderWriterLock();

        public static readonly string TableNameOld = "AStocks";
        public static readonly string ConnectionString = ConfigurationManager.AppSettings["ConnectionString"];
        public static string TableNameSuffix = "Stocks";
        public static readonly string SymbolColumn = "Symbol";
        public static readonly string NameColumn = "Name";
        public static readonly string VolumeColumn = "Volume";
        public static readonly string IdColumn = "Id";

        public static readonly string SymbolIdColumn = "SymbolId";
        public static readonly string DateColumn = "Date";
        public static readonly string OpenColumn = "Open";
        public static readonly string CloseColumn = "Close";
        public static readonly string LowColumn = "Low";
        public static readonly string TradeVolumeColumn = "TradeVolume";
        public static readonly string HighColumn = "High";
        public static readonly string UnAdjustCloseColumn = "UnAdjustClose";

        public static readonly string SMA5Column = "SMA5";
        public static readonly string SMA10Column = "SMA10";
        public static readonly string SMA30Column = "SMA30";
        public static readonly string SMA60Column = "SMA60";
        public static readonly string SMAColumnPrefix = "SMA";
        public static readonly string TrendColumn = "Trend";

        public static readonly string MethodNameColumn = "MethodName";
        public static readonly string QualifiedColumn = "Qualified";
        public static readonly string ValidColumn = "Valid";

        public static readonly string TotalQualifiedColumn = "TotalQualified";
        public static readonly string TotalValidColumn = "TotalValid";
        public static readonly string Last1YearValidColumn = "Last1YearValid";
        public static readonly string Last1YearQualifiedColumn = "Last1YearQualified";
        public static readonly string Last3YearValidColumn = "Last3YearValid";
        public static readonly string Last3YearQualifiedColumn = "Last3YearQualified";
        public static readonly string Last6YearValidColumn = "Last6YearValid";
        public static readonly string Last6YearQualifiedColumn = "Last6YearQualified";
        public static readonly string Last10YearValidColumn = "Last10YearValid";
        public static readonly string Last10YearQualifiedColumn = "Last10YearQualified";


        private static void MakeColumn(string columnName, string dataType, DataTable dataTable, bool isIdentity)
        {
            var column = new DataColumn();
            column.DataType = Type.GetType(dataType);
            column.ColumnName = columnName;
            column.AutoIncrement = isIdentity;
            column.Caption = columnName;
            column.ReadOnly = isIdentity;
            column.Unique = isIdentity;
            dataTable.Columns.Add(column);
        }

        public static string GetScriptPath(string scriptPath)
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            if (!scriptPath.StartsWith("\\"))
                scriptPath = "\\" + scriptPath;

            return Directory.GetParent(Directory.GetParent(Path.GetDirectoryName(path)).FullName).FullName + scriptPath;
        }

        public static void MakeNormalColumn(string columnName, string dataType, DataTable dataTable)
        {
            MakeColumn(columnName, dataType, dataTable, false);
        }

        public static void MakeIdentityColumn(string columnName, string dataType, DataTable dataTable)
        {
            MakeColumn(columnName, dataType, dataTable, true);
        }


        public static void MergeTable(DataTable rootTable, DataTable targetTable)
        {
            try
            {
                stockTableLocker.AcquireWriterLock(int.MaxValue);
                rootTable.Merge(targetTable);
            }
            finally
            {
                stockTableLocker.ReleaseWriterLock();
            }
        }
    }
}

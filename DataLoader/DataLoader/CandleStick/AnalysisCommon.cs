using System;
using System.Configuration;
using System.Data;
using System.Threading;

namespace StockAnalyzer.CandleStick
{
    public class AnalysisCommon
    {
        private static ReaderWriterLock analysisResultLocker = new ReaderWriterLock();

        public static readonly int TrendPeriod = Convert.ToInt32(ConfigurationManager.AppSettings["TrendPeriod"]);

        public static bool CheckTrend(int index, int count)
        {
            //unable to verify
            if (index - TrendPeriod < 0 || index + TrendPeriod > count)
                return false;

            return true;
        }

        public static void CheckTrendBeforeAfter(int index, DataRowCollection rows, out bool isBeforeUp, out bool isAfterUp)
        {
            isBeforeUp = true;
            isAfterUp = true;

            decimal last = 0;

            for (int i = index - TrendPeriod; i < index; i++)
            {
                if (rows[i][Common.SMA5Column] == DBNull.Value)
                    throw new Exception("SMA is null");

                var current = Convert.ToDecimal(rows[i][Common.SMA5Column]);
                if (current > last)
                {
                    last = current;
                }
                else
                {
                    isBeforeUp = false;
                    break;
                }
            }

            last = 0;
            for (int i = index + 1; i < index + TrendPeriod + 1; i++)
            {
                var current = Convert.ToDecimal(rows[i][Common.SMA5Column]);
                if (current > last)
                {
                    last = current;
                }
                else
                {
                    isAfterUp = false;
                    break;
                }
            }
        }

        public static void CheckValidPeriod(bool isValid, bool isQualified, DateTime date,
                                            ref int Last1YearQualified, ref int Last1YearValid,
                                            ref int Last3YearQualified, ref int Last3YearValid,
                                            ref int Last6YearQualified, ref int Last6YearValid,
                                            ref int Last10YearQualified, ref int Last10YearValid)
        {
            if (isQualified)
            {
                //last year
                if (DateTime.Now.Subtract(date).TotalDays <= 365)
                {

                    Last1YearQualified++;
                    if (isValid)
                        Last1YearValid++;
                }
                //last 3 years
                else if (DateTime.Now.Subtract(date).TotalDays <= 365 * 3)
                {
                    Last3YearQualified++;
                    if (isValid)
                        Last3YearValid++;
                }
                //last 6 years
                else if (DateTime.Now.Subtract(date).TotalDays <= 365 * 6)
                {
                    Last6YearQualified++;
                    if (isValid)
                        Last6YearValid++;
                }
                //last 10 years
                else if (DateTime.Now.Subtract(date).TotalDays <= 365 * 10)
                {
                    Last10YearQualified++;
                    if (isValid)
                        Last10YearValid++;
                }
            }
        }

        public static DataTable MakeAnalysisResultsTable()
        {
            DataTable table = new DataTable("[dbo].[StagingAnalysisResults]");
            // Declare variables for DataColumn and DataRow objects.
            Common.MakeIdentityColumn(Common.IdColumn, "System.Int32", table);
            Common.MakeNormalColumn(Common.MethodNameColumn, "System.String", table);
            Common.MakeNormalColumn(Common.SymbolIdColumn, "System.Int32", table);
            Common.MakeNormalColumn(Common.DateColumn, "System.DateTime", table);
            Common.MakeNormalColumn(Common.QualifiedColumn, "System.Int32", table);
            Common.MakeNormalColumn(Common.ValidColumn, "System.Int32", table);

            // Make the ID column the primary key column.
            DataColumn[] PrimaryKeyColumns = new DataColumn[1];
            PrimaryKeyColumns[0] = table.Columns[Common.IdColumn];
            table.PrimaryKey = PrimaryKeyColumns;

            return table;
        }

        public static DataTable MakeAnalysisStatisticTable()
        {
            DataTable table = new DataTable("[dbo].[AnalysisStatistic]");
            // Declare variables for DataColumn and DataRow objects.
            Common.MakeIdentityColumn(Common.IdColumn, "System.Int32", table);
            Common.MakeNormalColumn(Common.MethodNameColumn, "System.String", table);
            Common.MakeNormalColumn(Common.SymbolIdColumn, "System.Int32", table);
            Common.MakeNormalColumn(Common.TotalQualifiedColumn, "System.Decimal", table);
            Common.MakeNormalColumn(Common.TotalValidColumn, "System.Decimal", table);
            Common.MakeNormalColumn(Common.Last1YearQualifiedColumn, "System.Decimal", table);
            Common.MakeNormalColumn(Common.Last1YearValidColumn, "System.Decimal", table);
            Common.MakeNormalColumn(Common.Last3YearQualifiedColumn, "System.Decimal", table);
            Common.MakeNormalColumn(Common.Last3YearValidColumn, "System.Decimal", table);
            Common.MakeNormalColumn(Common.Last6YearQualifiedColumn, "System.Decimal", table);
            Common.MakeNormalColumn(Common.Last6YearValidColumn, "System.Decimal", table);
            Common.MakeNormalColumn(Common.Last10YearQualifiedColumn, "System.Decimal", table);
            Common.MakeNormalColumn(Common.Last10YearValidColumn, "System.Decimal", table);

            // Make the ID column the primary key column.
            DataColumn[] PrimaryKeyColumns = new DataColumn[1];
            PrimaryKeyColumns[0] = table.Columns[Common.IdColumn];
            table.PrimaryKey = PrimaryKeyColumns;

            return table;
        }

        public static void MergeAnalysisResultTable(DataTable rootTable, DataTable targetTable)
        {
            try
            {
                analysisResultLocker.AcquireWriterLock(int.MaxValue);
                rootTable.Merge(targetTable);
            }
            finally
            {
                analysisResultLocker.ReleaseWriterLock();
            }
        }
    }
}

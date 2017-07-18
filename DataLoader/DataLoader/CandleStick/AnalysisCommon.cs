using StockAnalyzer.Exceptions;
using System;
using System.Configuration;
using System.Data;
using System.Threading;

namespace StockAnalyzer.CandleStick
{
    public class AnalysisCommon
    {
        private static ReaderWriterLock analysisResultLocker = new ReaderWriterLock();
        public static readonly int TrendShortPeriod = Convert.ToInt32(ConfigurationManager.AppSettings["TrendShortPeriod"]);
        public static readonly int TrendMediumPeriod = Convert.ToInt32(ConfigurationManager.AppSettings["TrendMediumPeriod"]);
        public static readonly int TrendLongPeriod = Convert.ToInt32(ConfigurationManager.AppSettings["TrendLongPeriod"]);

        /// <summary>
        /// used for checking if trend is down
        /// </summary>
        private static string lessOperator = "<";
        /// <summary>
        /// used for checking if trend is up
        /// </summary>
        private static string greatOperator = ">";
        public enum TrendDirection { Up, Down, None };

        public enum TrendPeriod { Short, Medium, Long };

        private static bool CheckTrendDirectionHelper(DataRowCollection rows, int start, int end, string operatorString)
        {
            if (rows[start][Common.SMA5Column] == DBNull.Value)
                throw new SMANULLException();

            decimal last = Convert.ToDecimal(rows[start][Common.SMA5Column]);

            for (int i = start; i < end; i++)
            {
                if (rows[i][Common.SMA5Column] == DBNull.Value)
                    throw new SMANULLException();

                var current = Convert.ToDecimal(rows[i][Common.SMA5Column]);

                if (operatorString == lessOperator)
                {
                    if (current <= last)
                    {
                        last = current;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (operatorString == greatOperator)
                {
                    if (current >= last)
                    {
                        last = current;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static int GetTrendPeriod(TrendPeriod trendPeriod)
        {
            int nTrendPeriod = 0;
            switch (trendPeriod)
            {
                case TrendPeriod.Short:
                    nTrendPeriod = TrendShortPeriod;
                    break;
                case TrendPeriod.Medium:
                    nTrendPeriod = TrendMediumPeriod;
                    break;
                case TrendPeriod.Long:
                    nTrendPeriod = TrendLongPeriod;
                    break;
                default:
                    break;
            }

            return nTrendPeriod;
        }

        private static TrendDirection CheckTrendDirection(DataRowCollection rows, int start, int end)
        {
            var direction = TrendDirection.None;
            bool isDown = false;
            bool isUp = false;

            try
            {
                isDown = CheckTrendDirectionHelper(rows, start, end, lessOperator);
                isUp = CheckTrendDirectionHelper(rows, start, end, greatOperator);
            }
            catch (SMANULLException ex)
            {
                direction = TrendDirection.None;
            }

            if (isDown)
                direction = TrendDirection.Down;
            if (isUp)
                direction = TrendDirection.Up;

            return direction;
        }

        /// <summary>
        /// Check if the point has enough data for checking before and after trend
        /// </summary>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static bool CheckTrendPeriod(int index, int count, TrendPeriod trendPeriod)
        {
            int nTrendPeriod = GetTrendPeriod(trendPeriod);

            //unable to verify
            if (index - nTrendPeriod < 0 || index + nTrendPeriod > count)
                return false;

            return true;
        }
    
        public static TrendDirection CheckBeforeTrendDirection(DataRowCollection rows, int index, TrendPeriod trendPeriod)
        {
            int nTrendPeriod = GetTrendPeriod(trendPeriod);
            return CheckTrendDirection(rows, index - nTrendPeriod, index);
        }

        public static TrendDirection CheckAfterTrendDirection(DataRowCollection rows, int index, TrendPeriod trendPeriod)
        {
            int nTrendPeriod = GetTrendPeriod(trendPeriod);
            return CheckTrendDirection(rows, index + 1, index + nTrendPeriod + 1);
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

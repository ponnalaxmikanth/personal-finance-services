using System;
namespace BusinessAccess.Services
{
    public class Conversions
    {
        public static int ToInt(object val, int defaultValue)
        {
            return (val == null || val == DBNull.Value ? defaultValue : Convert.ToInt32(val));
        }

        public static int ToInt(double val)
        {
            return Convert.ToInt32(val);
        }

        public static decimal ToDecimal(object val, decimal defaultValue)
        {
            return (val == null || val == DBNull.Value ? defaultValue : Convert.ToDecimal(val));
        }

        public static double ToDouble(object val, double defaultValue)
        {
            return (val == null || val == DBNull.Value ? defaultValue : Convert.ToDouble(val));
        }

        public static long ToLong(object val, long defaultValue)
        {
            return (val == null || val == DBNull.Value ? defaultValue : Convert.ToInt64(val));
        }

        public static string ToString(object val, string defaultValue)
        {
            return (val == null || val == DBNull.Value ? defaultValue : val.ToString());
        }

        public static DateTime ToDateTime(object val, DateTime defaultValue)
        {
            return (val == null || val == DBNull.Value ? defaultValue : Convert.ToDateTime(val));
        }

        public static int GetFinancialYear(DateTime transactionDate)
        {
            return transactionDate.Month < 4 ? transactionDate.Year - 1 : transactionDate.Year;
        }

        public static bool ToBoolean(object val, bool defaultValue)
        {
            return (val == null || val == DBNull.Value ? defaultValue : Convert.ToBoolean(val));
        }
    }
}

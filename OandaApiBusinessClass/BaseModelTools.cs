using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Data.SqlClient;
using System.Data;
using MySql.Data.MySqlClient;

namespace OandaApiBusinessClass
{
    public class BaseModelTools
    {
        public static string CultureName = "cs-CZ";
        public static TimeZoneInfo TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time");

        public static string BoolToText(bool b)
        {
            return b ? "Ano" : "Ne";
        }

        public static string DDLGetEmptyItemName()
        {
            return "---";
        }

        public static int DDLGetEmptyItemId()
        {
            return -1;
        }

        public static string FilterDiacritics(string s)
        {
            return new String(s.Normalize(NormalizationForm.FormKD).Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray());
        }

        public static bool ContainSubstring(string search, string what)
        {
            if (string.IsNullOrEmpty(what))
                return true;
            return FilterDiacritics(search).IndexOf(FilterDiacritics(what), StringComparison.OrdinalIgnoreCase) != -1;
        }

        public static void AddWhereOrAnd(ref string where)
        {
            if (string.IsNullOrEmpty(where))
                where += @"  WHERE ";
            else
                where += @"  AND ";
        }

        public static string NullToStr(string s)
        {
            if (s == null)
                return String.Empty;
            else
                return s;
        }

        public static string ToTimeSpan(TimeSpan ts)
        {
            string s = string.Empty;
            int days = (int)Math.Ceiling(ts.TotalDays) - 1;
            if (ts.Ticks < 0)
                s += "-";
            if (days >= 1)
            {
                s += days.ToString();
                if (days == 1)
                    s += " den ";
                else if (days >= 2 && days <= 4)
                    s += " dny ";
                else if (days >= 5)
                    s += " dnů ";
            }            
            s += ts.ToString("h\\:mm\\:ss", CultureInfo.CreateSpecificCulture(CultureName));
            return s;
        }

        public static string ToString(object value)
        {
            if (value == null)
                return string.Empty;
            else
                return value.ToString();
        }

        public static DateTime ToDateTime(object value)
        {
            return Convert.ToDateTime(value, CultureInfo.CreateSpecificCulture(CultureName));
        }

        public static string ToDateTime(DateTime dt)
        {
            return dt.ToString("G", CultureInfo.CreateSpecificCulture(CultureName));
        }

        public static string ToDate(DateTime dt)
        {
            return dt.ToString("d", CultureInfo.CreateSpecificCulture(CultureName));
        }

        public static decimal ToDecimal(object value)
        {
            return Convert.ToDecimal(value, CultureInfo.CreateSpecificCulture(CultureName));
        }

        public static string ToDecimal(decimal d, int decimalPlaces = Int32.MaxValue)
        {
            if (decimalPlaces == Int32.MaxValue)
                return d.ToString(CultureInfo.CreateSpecificCulture(CultureName));
            else
                return d.ToString("F" + decimalPlaces.ToString(), CultureInfo.CreateSpecificCulture(CultureName));
        }

        public static Nullable<int> ToInteger(object value)
        {
            if (value == null || (value is string && string.IsNullOrEmpty(Convert.ToString(value))))
                return null;
            else 
                return Convert.ToInt32(value, CultureInfo.CreateSpecificCulture(CultureName));
        }

        public static float ToFloat(object value)
        {
            return (float)Convert.ToDouble(value, CultureInfo.CreateSpecificCulture(CultureName));
        }

        public static double ToDouble(object value)
        {
            return (double)Convert.ToDecimal(value, CultureInfo.CreateSpecificCulture(CultureName));
        }

        public static DateTime ToLocalDateTime(DateTime dt)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(dt.ToUniversalTime(), TimeZone);
        }

        public static string ToDouble(Nullable<double> d)
        {
            if (d == null)
                return string.Empty;
            else
                return d.Value.ToString(CultureInfo.CreateSpecificCulture(CultureName));
        }

        public static string ToFloat(Nullable<float> f)
        {
            if (f == null)
                return string.Empty;
            else
                return f.Value.ToString(CultureInfo.CreateSpecificCulture(CultureName));
        }

        public static string ToFloat(float f)
        {
            return f.ToString(CultureInfo.CreateSpecificCulture(CultureName));
        }

        public static Nullable<bool> GetParameterNullableBool(MySqlDataReader r, string name)
        {
            if (r[name] == DBNull.Value)
                return null;
            else
                return Convert.ToBoolean(r[name]);
        }

        public static Nullable<int> GetParameterNullableInt(MySqlDataReader r, string name)
        {
            if (r[name] == DBNull.Value)
                return null;
            else
                return Convert.ToInt32(r[name]);
        }

        public static object GetParameterNullableEnum<T>(MySqlDataReader r, string name)
        {
            if (r[name] == DBNull.Value)
                return null;
            else
            {
                string data = Convert.ToString(r[name]);
                T result = default(T);
                result = (T)Enum.Parse(typeof(T), data, true);
                return result;
            }
        }

        public static string GetParameterNullableString(MySqlDataReader r, string name)
        {
            if (r[name] == DBNull.Value)
                return null;
            else
                return Convert.ToString(r[name]);
        }

        public static Nullable<float> GetParameterNullableFloat(MySqlDataReader r, string name)
        {
            if (r[name] == DBNull.Value)
                return null;
            else
                return (float)ToFloat(r[name]);
        }

        public static Nullable<decimal> GetParameterNullableDecimal(MySqlDataReader r, string name)
        {
            if (r[name] == DBNull.Value)
                return null;
            else
                return ToDecimal(r[name]);
        }

        public static Nullable<double> GetParameterNullableDouble(MySqlDataReader r, string name)
        {
            if (r[name] == DBNull.Value)
                return null;
            else
                return (double)ToDecimal(r[name]);
        }

        public static Nullable<TimeSpan> GetParameterNullableTimeSpan(MySqlDataReader r, string name)
        {
            if (r[name] == DBNull.Value)
                return null;
            else
                return (TimeSpan)r[name];
        }

        public static Nullable<DateTime> GetParameterNullableDateTime(MySqlDataReader r, string name)
        {
            if (r[name] == DBNull.Value)
                return null;
            else
                return (DateTime)r[name];
        }

        public static string TransformFromTextToHTML(string s)
        {
            if (string.IsNullOrEmpty(s))
                return null;
            s = s.Replace("\n", "<br/>");
            s = s.Replace("\r", "");
            s = s.Replace(" ", "&nbsp;");            
            return s;
        }

        public static string TransformFromHTMLToText(string s)
        {
            if (string.IsNullOrEmpty(s))
                return null;
            s = s.Replace("<br/>", "\n");
            s = s.Replace("&nbsp;", " ");
            return s;
        }

        public static void AddParameterNullableInt(MySqlCommand cmd, string name, Nullable<int> parameter)
        {
            if (parameter == null)
                cmd.Parameters.Add(new MySqlParameter(name, DBNull.Value));
            else
                cmd.Parameters.Add(new MySqlParameter(name, parameter));
        }

        public static void AddParameterNullableFloat(MySqlCommand cmd, string name, Nullable<float> parameter)
        {
            if (parameter == null)
                cmd.Parameters.Add(new MySqlParameter(name, DBNull.Value));
            else
                cmd.Parameters.Add(new MySqlParameter(name, parameter));
        }

        public static void AddParameterNullableDouble(MySqlCommand cmd, string name, Nullable<double> parameter)
        {
            if (parameter == null)
                cmd.Parameters.Add(new MySqlParameter(name, DBNull.Value));
            else
                cmd.Parameters.Add(new MySqlParameter(name, parameter));
        }

        public static void AddParameterNullableEnum(MySqlCommand cmd, string name, object parameter)
        {
            if (parameter == null)
                cmd.Parameters.Add(new MySqlParameter(name, DBNull.Value));
            else
                cmd.Parameters.Add(new MySqlParameter(name, parameter.ToString()));
        }

        public static void AddParameterNullableString(MySqlCommand cmd, string name, string parameter)
        {
            if (string.IsNullOrEmpty(parameter))
                cmd.Parameters.Add(new MySqlParameter(name, DBNull.Value));
            else
                cmd.Parameters.Add(new MySqlParameter(name, parameter));
        }

        public static void AddParameterNullableDecimal(MySqlCommand cmd, string name, Nullable<decimal> parameter)
        {
            if (parameter == null)
                cmd.Parameters.Add(new MySqlParameter(name, DBNull.Value));
            else
                cmd.Parameters.Add(new MySqlParameter(name, parameter));
        }

        public static void AddParameterNullableTimeSpan(MySqlCommand cmd, string name, Nullable<TimeSpan> parameter)
        {
            if (parameter == null)
                cmd.Parameters.Add(new MySqlParameter(name, DBNull.Value));
            else
                cmd.Parameters.Add(new MySqlParameter(name, parameter));
        }

        public static void AddParameterNullableDateTime(MySqlCommand cmd, string name, Nullable<DateTime> parameter)
        {
            if (parameter == null)
                cmd.Parameters.Add(new MySqlParameter(name, DBNull.Value));
            else
                cmd.Parameters.Add(new MySqlParameter(name, parameter));
        }

        public static bool ColumnExists(IDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).ToUpper() == columnName.ToUpper())
                {
                    return true;
                }
            }

            return false;
        }

        public static string GetOrParameters(string parameterName, int itemsCount)
        {
            string sql = string.Empty;
            for(int index = 0; index < itemsCount; index++)
            {
                if (index != 0)
                    sql += " OR ";
                sql += string.Format(@"{0} = @{0}_{1}", parameterName, index);
            }
            return sql;
        }

    }
}

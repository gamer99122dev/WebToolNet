using System;
using System.Data;
using System.Linq;
using System.Text;

namespace WebToolNet.Extensions
{
    public static class DataRowTool
    {
        public static string pCol(this DataRow r, int colIndex)
        {
            if (r == null || r[colIndex] == null)
                return "";

            return (r[colIndex] ?? "").ToString().pNullOrTrim();
        }

        public static string pGetColumn(this DataRow r, string ColumnName)
        {
            // tocheck: r.Table.Columns[ColumnName] == null 表示給錯欄位名，如果濾掉錯誤將不易發現
            if (r == null || r[ColumnName] == null)
                return "";

            return (r[ColumnName] ?? "").ToString().pNullOrTrim();
        }

        // pCol = pGetColumn
        public static string pCol(this DataRow r, string ColumnName)
        {
            return pGetColumn(r, ColumnName);
        }

        public static int pGetColumnInt(this DataRow r, string ColumnName)
        {
            String s = pGetColumn(r, ColumnName);
            return Convert.ToInt32(s.Length > 0 ? s : "0");
        }

        public static long pGetColumnLong(this DataRow r, string ColumnName)
        {
            String s = pGetColumn(r, ColumnName);
            return Convert.ToInt64(s.Length > 0 ? s : "0");
        }

        public static double pGetColumnDouble(this DataRow r, string ColumnName, double defaultVal = 0)
        {
            String s = pGetColumn(r, ColumnName);
            double val = defaultVal;
            try
            {
                val = Convert.ToDouble(s);
            }
            catch (Exception)
            {
                val = defaultVal;
            }
            return val;
        }

        // 以欄位substring取值
        //   例如, 欄位為: "身分證號(A12)"
        //   r.pColSub("A12")即可
        public static string pColSub(this DataRow r, string ColumnSubName)
        {
            if (r == null || r.Table == null)
            {
                Exception e1 = new Exception("DataRow 錯誤");
                throw e1;
            }

            foreach (DataColumn c in r.Table.Columns)
            {
                if (c.ColumnName.Contains(ColumnSubName))
                {
                    string s = r.pCol(c.ColumnName);
                    return s;
                }
            }

            Exception e2 = new Exception($"無相關欄位[{ColumnSubName}]");
            throw e2;
        }

        public static string pToString(this DataRow row)
        {
            if (row == null)
                return "DataRow NULL";

            DataTable dt = row.Table;

            var output = new StringBuilder();

            var columnsWidths = new int[dt.Columns.Count];

            // Get column widths
            //foreach (DataRow row in dt.Rows)
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    var length = row[i].ToString().Length;
                    if (columnsWidths[i] < length)
                        columnsWidths[i] = length;
                }
            }

            // Get Column Titles
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                var length = dt.Columns[i].ColumnName.Length;
                if (columnsWidths[i] < length)
                    columnsWidths[i] = length;
            }

            // Write Column titles
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                var text = dt.Columns[i].ColumnName;
                output.Append("|" + PadCenter(text, columnsWidths[i] + 2));
            }
            output.Append("|\n" + new string('=', output.Length) + "\n");

            // Write Rows
            //foreach (DataRow row in dt.Rows)
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    var text = row[i].ToString();
                    output.Append("|" + PadCenter(text, columnsWidths[i] + 2));
                }
                output.Append("|\n");
            }
            return output.ToString();
        }


        public static bool pContains(this DataRow row, string search)
        {
            foreach (DataColumn c in row.Table.Columns)
            {
                if (row[c].ToString().Contains(search))
                {
                    return true;
                }
            }
            return false;
        }

        private static string PadCenter(string text, int maxLength)
        {
            int diff = maxLength - text.Length;
            return new string(' ', diff / 2) + text + new string(' ', (int)(diff / 2.0 + 0.5));
        }

    }
}

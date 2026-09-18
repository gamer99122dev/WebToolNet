using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Text;
using WebToolNet.Data;

namespace WebToolNet.Extensions
{
    public static class DataTableUtil
    {
        // https://stackoverflow.com/questions/1104121/how-to-convert-a-datatable-to-a-string-in-c

        //public static string pToString(this DataTable dt)
        //{
        //    return string.Join(Environment.NewLine, dt.Rows.OfType<DataRow>().Select(x => string.Join(" ; ", x.ItemArray)));
        //}

        public static string pToString(this DataTable dt)
        {
            if (dt == null)
                return "DataTable NULL";

            var output = new StringBuilder();

            var columnsWidths = new int[dt.Columns.Count];

            // Get column widths
            foreach (DataRow row in dt.Rows)
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
            foreach (DataRow row in dt.Rows)
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

        private static string PadCenter(string text, int maxLength)
        {
            int diff = maxLength - text.Length;
            return new string(' ', diff / 2) + text + new string(' ', (int)(diff / 2.0 + 0.5));
        }

        public static bool pEmpty(this DataTable dt)
        {
            return !pAny(dt);
        }

        public static bool pAny(this DataTable dt)
        {
            bool hasAnyRow = (dt != null && dt.Rows.Count > 0);
            return hasAnyRow;
        }


        public static DataTable pOrderByToDataTable(this DataTable dt, string columnName, SortOrder sort = SortOrder.Ascending)
        {

            if (dt.pEmpty())
            {
                return dt;
            }

            // 用 DataView 排序：依欄位型別比較(數字欄位就是數字排序)，舊版 LINQ 是全部轉字串再排
            DataView dv = new DataView(dt);
            if (sort == SortOrder.Ascending)
            {
                dv.Sort = "[" + columnName + "] ASC";
            }
            else if (sort == SortOrder.Descending)
            {
                dv.Sort = "[" + columnName + "] DESC";
            }
            else
            {
                return dt.Clone();
            }

            return dv.ToTable();
        }


        public static int pCount(this DataTable dt)
        {
            if (dt == null)
                return 0;

            return dt.Rows.Count;
        }

        public static string pRowCol(this DataTable dt, string ColumnName, int RowIndex = 0)
        {
            // RowIndex大於RowCount時讓它Crash
            string s = dt.Rows[RowIndex].pCol(ColumnName);
            return s;
        }

        // Trim所有String資料
        public static DataTable pTrim(this DataTable dt)
        {
            foreach (DataRow r in dt.Rows)
            {
                foreach (DataColumn c in dt.Columns)
                {
                    if (c.DataType == typeof(String))
                    {
                        bool rw = c.ReadOnly;
                        c.ReadOnly = false;
                        r[c.ColumnName] = r.pCol(c.ColumnName);
                        c.ReadOnly = rw;
                    }
                }
            }

            return dt;
        }

        public static int pMax(this DataTable dt, string column)
        {
            int max = int.MinValue;

            foreach (DataRow r in dt.Rows)
            {
                int val = r.pGetColumnInt(column);
                max = Math.Max(max, val);
            }

            return max;
        }

        public static int pMin(this DataTable dt, string column)
        {
            int min = int.MaxValue;

            foreach (DataRow r in dt.Rows)
            {
                int val = r.pGetColumnInt(column);
                min = Math.Min(min, val);
            }

            return min;
        }

        // 取代DataTable Where(), 查詢為空值時, 回傳DataTable clone
        public static DataTable pWhere(this DataTable dt, Func<DataRow, bool> predicate)
        {
            if (dt == null)
            {
                return null;
            }
            var q = dt.AsEnumerable().Where(predicate);
            var r = q.Any() ? q.CopyToDataTable() : dt.Clone();

            return r;
        }

        public static string ConvertObjectToString(object obj)
        {
            return obj.ToString() ?? string.Empty;
        }

        public static DataTable pAddCols(this DataTable dt, params object[] args)
        {
            string[] cols = Array.ConvertAll<object, string>(args, ConvertObjectToString);
            foreach (string col in cols)
            {
                dt.pAddCol(col, Length: 100, ReadOnly: false);
            }

            return dt;
        }


        public static DataTable pAddCol(this DataTable dt, string ColName, int Length = 100, bool ReadOnly = false)
        {
            if (dt.Columns.Contains(ColName) == false)
            {
                dt.Columns.Add(ColName);
                dt.Columns[ColName].ReadOnly = ReadOnly;
                dt.Columns[ColName].MaxLength = Length;
            }

            return dt;
        }

        public static DataTable pSetColWritable(this DataTable dt, string ColName, int Length = 100)
        {
            if (dt.Columns.Contains(ColName))
            {
                dt.Columns[ColName].ReadOnly = false;
                dt.Columns[ColName].MaxLength = Length;
            }

            return dt;
        }

        public static DataTable pAddColInt(this DataTable dt, string ColName, bool ReadOnly = false)
        {
            dt.Columns.Add(ColName, typeof(int));
            dt.Columns[ColName].ReadOnly = ReadOnly;

            return dt;
        }

        public static DataTable pAddColDouble(this DataTable dt, string ColName, bool ReadOnly = false)
        {
            dt.Columns.Add(ColName, typeof(double));
            dt.Columns[ColName].ReadOnly = ReadOnly;

            return dt;
        }

        public static string pSQLInsert(this DataTable dt, string dbName = "DB_OPD")
        {
            string insertSQL = "";
            if (dt.pAny())
            {
                DataTableTool dtt = new DataTableTool();
                insertSQL = dtt.DataTableToSingleInsertSQL(dt, dbName);
            }
            return insertSQL;
        }

        public static string pSQLInsert(this DataTable dt)
        {
            string insertSQL = dt.pSQLInsert("DB_OPD");

            return insertSQL;
        }
        public static string pMIDDLESQLInsert(this DataTable dt)
        {
            string insertSQL = "";
            if (dt.pAny())
            {
                DataTableTool dtt = new DataTableTool();
                insertSQL = dtt.DataTableToSingleInsertMIDDLESQL(dt);
            }
            return insertSQL;
        }


        public static string[] pColumnToArray(this DataTable dt, string ColName)
        {
            string[] strArray = { };
            if (dt.pEmpty())
            {
                return strArray;
            }


            strArray = new string[dt.Rows.Count];
            for (int i = 0; i < dt.Rows.Count; i++)
                strArray[i] = dt.Rows[i].pGetColumn(ColName);

            return strArray;
        }

        // 欄位加總(int 4byte)
        public static int pSumInt(this DataTable dt, string ColName)
        {
            int total = 0;
            foreach (DataRow row in dt.Rows)
                total = checked(total + row.pGetColumnInt(ColName));

            return total;
        }

        // 欄位加總(long 8byte)
        public static long pSumLong(this DataTable dt, string ColName)
        {
            long total = 0;
            foreach (DataRow row in dt.Rows)
                total = checked(total + row.pGetColumnLong(ColName));

            return total;
        }

        // 欄位加總(double)
        public static double pSumDouble(this DataTable dt, string ColName)
        {
            double total = 0;
            foreach (DataRow row in dt.Rows)
                total += row.pGetColumnDouble(ColName);

            return total;
        }

    }
}

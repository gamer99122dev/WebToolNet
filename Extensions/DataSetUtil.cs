using System;
using System.Data;
using System.Linq;
using System.Text;

namespace WebToolNet.Extensions
{
    public static class DataSetUtil
    {
        public static bool pAny(this DataSet ds)
        {
            bool hasAnyTables = (ds != null && ds.Tables.Count > 0);
            return hasAnyTables;
        }


        public static string pToString(this DataSet ds)
        {
            var output = new StringBuilder();

            for (int i = 0; i < ds.Tables.Count; i++)
            {
                output.Append($"Table:{ds.Tables[i].TableName}\n");
                output.AppendLine(ds.Tables[i].pToString());
            }

            return output.ToString();
        }

        public static DataSet pAdd(this DataSet ds, DataTable dt, string tableName = null)
        {

            if (ds.Tables.Contains(dt.TableName))
            {
                ds.Tables.Remove(dt.TableName);
            }

            ds.Tables.Add(dt);
            return ds;
        }
    }

}

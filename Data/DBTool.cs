using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebToolNet.UtilExtension;

namespace WebToolNet.Data
{
    public class DBTool
    {
        SqlEscape injection;

        public DBTool()
        {
            injection = new SqlEscape();
        }

        public string ISGenerator(object o)
        {
            string result = "";

            // 原本 o == null 時會在 o.GetType() 炸 NullReferenceException
            if (o == null || o == DBNull.Value)
                return "null";

            if (o.GetType() == typeof(string))
            {

                result = "'" + injection.SQLValidator(o.ToString()) + "'";

            }
            else if (o.GetType() == typeof(DateTime))
            {

                DateTime dt = Convert.ToDateTime(o);
                result = "'" + dt.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'";

            }
            else if (o.GetType() == typeof(double) || o.GetType() == typeof(decimal))
            {

                result = o.ToString();

            }
            else if (o.GetType() == typeof(int))
            {

                result = o.ToString();

            }
            else if (o.GetType() == typeof(Int16) || o.GetType() == typeof(Single))
            {
                result = o.ToString();
            }
            else if (o.GetType() == typeof(Boolean))
            {

                if ((bool)o == true)
                {

                    result = "'1'";

                }
                else
                {

                    result = "'0'";
                }




            }

            return result;

        }

        public string DictionarytoInsertSQL(Dictionary<string, object> row, string TableName)
        {
            string SQL = "Insert Into " + TableName + "(";

            foreach (string key in row.Keys)
            {
                SQL += key + ", ";
            }

            SQL = SQL.Substring(0, SQL.Length - 2) + ")\n";

            SQL += " values (";

            foreach (object o in row.Values)
            {
                SQL += ISGenerator(o) + ", ";
            }

            SQL = SQL.Substring(0, SQL.Length - 2) + ");\n";
            return SQL;
        }
    }
}

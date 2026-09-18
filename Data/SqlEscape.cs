using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace WebToolNet.Data
{
    public class SqlEscape
    {
        public string SQLValidator(string SQL)
        {
            if (SQL == null)
                return "";

            //string[] Str =  { "'", "//", "--", "/*", "*/" };
            string[] Str = { "'"};

            for (int i = 0; i < Str.Length; i++)
            {

                SQL = SQL.Replace(Str[i], "''");
            }


            return SQL;
        }

        public string RowfilterValidator(string Rowfilter)
        {

            string[] Str = { "!", "@", "#", "$", "%", "^", "&", "*", "{", "}", "?", "[", "]", "'", ",", "~", "/", "\\", ":", ";", "<", ">" };

            for (int i = 0; i < Str.Length; i++)
            {
                Rowfilter = Rowfilter.Replace(Str[i], "["+ Str[i] + "]");
            }


            return Rowfilter;
        }

        //目前可以跳脫的符號 =>  *、%、 [、]、'
        public string EscapeRowfilter(string valueWithoutWildcards)
        {

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < valueWithoutWildcards.Length; i++)
            {
                char c = valueWithoutWildcards[i];
                if (c == '*' || c == '%' || c == '[' || c == ']')
                    sb.Append("[").Append(c).Append("]");
                else if (c == '\'')
                    sb.Append("''");
                else
                    sb.Append(c);
            }
            return sb.ToString();

        }


    }
}

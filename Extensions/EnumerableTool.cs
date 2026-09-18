using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace WebToolNet.Extensions
{
    public static class EnumerableTool
    {
        public static void pForeach<T>(this IEnumerable<T> _this, Action<T> action)
        {
            foreach (var cur in _this)
            {
                action(cur);
            }
        }

        public static IEnumerable<TResult> pForeach<T, TResult>(this IEnumerable<T> _this, Func<T, TResult> action)
        {
            foreach (var cur in _this)
            {
                yield return action(cur);
            }
        }

        public static void pForeachWithIndex<T>(this IEnumerable<T> _this, Action<T, int> action)
        {
            int idx = 0;
            foreach (var cur in _this)
            {
                action(cur, idx++);
            }
        }

        public static IEnumerable<TResult> pForeachWithIndex<T, TResult>(this IEnumerable<T> _this, Func<T, int, TResult> action)
        {
            int idx = 0;
            foreach (var cur in _this)
            {
                yield return action(cur, idx++);
            }
        }

        public static IEnumerable<TSource> pDistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> knownKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (knownKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }
        /// <summary>
        /// 泛型集合轉DataTable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="dtName"></param>
        /// <returns></returns>
        public static DataTable pToDataTable<T>(this IEnumerable<T> obj, string dtName = "")
        {
            DataTable dt = new DataTable() { TableName = dtName };
            PropertyInfo[] props = null;
            foreach (T item in obj)
            {
                if (props == null) //尚未初始化
                {
                    Type t = item.GetType();
                    props = t.GetProperties();
                    foreach (PropertyInfo pi in props)
                    {
                        Type colType = pi.PropertyType;
                        //針對Nullable<>特別處理
                        if (colType.IsGenericType
                            && colType.GetGenericTypeDefinition() == typeof(Nullable<>))
                            colType = colType.GetGenericArguments()[0];
                        //建立欄位
                        dt.Columns.Add(pi.Name, colType);
                    }
                }
                DataRow row = dt.NewRow();
                foreach (PropertyInfo pi in props)
                    row[pi.Name] = pi.GetValue(item, null) ?? DBNull.Value;
                dt.Rows.Add(row);
            }
            return dt;
        }

    }
}

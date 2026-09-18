using System;
using System.Collections.Generic;
using System.Linq;

namespace WebToolNet.Extensions
{
    public static class ListUtil
    {
        public static void pAddMany<T>(this List<T> list, params T[] elements)
        {
            list.AddRange(elements);
        }

        public static string pJoin<T>(this List<T> list, string delimiter = ",")
        {
            return list.ToArray<T>().pJoin(delimiter);
        }

        public static string pJoinWithQuote<T>(this List<T> list, string delimiter = ",", string quote = "'")
        {
            return list.ToArray<T>().pJoinWithQuote(delimiter, quote);
        }

        // 非空的項目才加入
        public static void pAddNonEmpty<T>(this List<string> list, string item)
        {
            if (item.pEmpty())
            {
                return;
            }
            list.Add(item);
        }


        public static bool pAny<T>(this List<T> list)
        {
            return list.Count > 0;
        }

        public static bool pEmpty<T>(this List<T> list)
        {
            return list.Count == 0;
        }

    }
}

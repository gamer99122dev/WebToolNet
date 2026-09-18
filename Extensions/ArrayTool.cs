using System;

namespace WebToolNet.Extensions
{
    public static class ArrayTool
    {
        /// <summary>
        ///  Array [1,2,3] or ["1","2","3"] 回傳字串 "1,2,3"
        /// </summary>
        public static string pJoin<T>(this T[] array, string delimiter = ",")
        {
            // string.Join<T> 遇到 null 元素本來就會當成空字串
            return string.Join(delimiter, array);
        }

        // 03, B1 Join 回傳 '03','B1'
        public static string pJoinWithQuote<T>(this T[] array, string delimiter = ",", string quote = "'")
        {
            string[] parts = new string[array.Length];
            for (int i = 0; i < array.Length; i++)
                parts[i] = array[i] == null ? "" : quote + array[i] + quote;

            return string.Join(delimiter, parts);
        }
    }
}

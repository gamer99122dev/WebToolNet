using System;
using System.Collections.Generic;

namespace WebToolNet.Extensions
{
    public static class DictionaryTool
    {
        /// <summary>
        /// 搜尋字典物件，如果沒有則New一個空的回傳
        /// </summary>
        // https://stackoverflow.com/questions/16192906/net-dictionary-get-or-create-new
        public static TValue pGetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
        where TValue : new()
        {
            TValue val;

            if (!dict.TryGetValue(key, out val))
            {
                val = new TValue();
                dict.Add(key, val);
            }

            return val;
        }

        public static TValue pCol<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, bool NullOnEmpty = false)
        {
            TValue value = dict.pGetColumn(key, NullOnEmpty);
            return value;
        }

        public static TValue pGetColumn<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, bool NullOnEmpty = false)
        {
            if (NullOnEmpty && dict.ContainsKey(key) == false)
                return default(TValue);

            return dict[key];
        }

        // 取值, key不存在回傳空字串, 限定String類別
        public static string pColNoNull<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
        {
            if (dict.ContainsKey(key) == false)
            {
                return "";
            }

            return dict[key].ToString();
        }

        public static string pToString<TKey, TValue>(this IDictionary<TKey, TValue> dict, string separator = "\n")
        {
            List<string> parts = new List<string>();
            foreach (KeyValuePair<TKey, TValue> kv in dict)
                parts.Add(kv.Key + "=" + kv.Value);

            return string.Join(separator, parts);
        }
    }
}

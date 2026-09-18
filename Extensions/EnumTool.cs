using System;
using System.ComponentModel;
using System.Reflection;

namespace WebToolNet.Extensions
{
    public static class EnumTool
    {
        /// <summary>
        /// 傳入列舉，回傳Description 的值
        /// </summary>
        /// <param name="value">列舉</param>
        /// <returns></returns>
        public static string pToDescription(this IConvertible value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute),
                false);

            if (attributes != null &&
                attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }


        public static bool pIn(this IConvertible sSource, params object[] args)
        {
            if (sSource == null)
                return false;

            string source = sSource.ToString();
            bool contained = source.pIn(args);

            return contained;
        }

        public static bool pNotIn(this IConvertible sSource, params object[] args)
        {
            bool IsIn = sSource.pIn(args);
            return !IsIn;
        }
    }
}

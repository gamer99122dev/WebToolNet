using System;

namespace WebToolNet.Extensions
{
    public static class IntTool
    {
        public static bool pBetween(this int num, int low, int high)
        {
            return num >= low && num <= high;
        }

        public static bool pNotBetween(this int num, int low, int high)
        {
            return !num.pBetween(low, high);
        }

        // 回傳不超過min,max的值
        // int val = 10;
        // val = val.pClamp(5, 20), 此時val為原本10
        // val = val.pClamp(20, 30), 此時val為min 20
        // 比較好的設計為擴充Math Class, 暫時先放這裡
        public static T pClamp<T>(this T value, T min, T max) where T : IComparable<T>
        {
            if (value.CompareTo(min) < 0)
                return min;
            if (value.CompareTo(max) > 0)
                return max;

            return value;
        }

        // 將數字格式化成顯示數字 1234567 => 1,234,567
        public static string pToIntN(this int num)
        {
            return num.ToString("N0");
        }
    }
}

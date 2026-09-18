using System;

namespace WebToolNet.Extensions
{
    public static class DoubleTool
    {
        public static bool pBetween(this double num, double low, double high)
        {
            return num >= low && num <= high;
        }

        public static bool pNotBetween(this double num, double low, double high)
        {
            return !num.pBetween(low, high);
        }

        public static int pToIntRound(this double num, MidpointRounding Rounding = MidpointRounding.AwayFromZero)
        {
            num = Math.Round(num, Rounding);
            int i = Convert.ToInt32(num);
            return i;
        }

    }
}

using System;

namespace WebToolNet.Extensions
{
    public static class BoolTool
    {
        public static bool pEqualsAll(this bool sSource, params object[] args)
        {
            foreach (bool s in Array.ConvertAll<object, bool>(args, ConvertObjectToBool))
            {
                if (sSource != s)
                    return false;
            }

            return true;
        }

        public static bool ConvertObjectToBool(object obj)
        {
            //return obj.ToString() ?? string.Empty;
            return (bool)obj;
        }
    }

}

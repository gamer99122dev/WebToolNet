using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WebToolNet.Validation
{
    public class CheckDate
    {
        public bool isTWDate(string twd)
        {
            int y;
            DateTime dt;
            string acDate;

            if (twd.Length < 7)
            {
                Debug.Print("日期不足7碼");
                return false;
            }


            /*驗證日期格式 YYYMMDD, 範圍0010101~9991231 回傳不符合格式*/
            //Regex regex = new Regex("(^000|001|999|[0-9]{3})+(0[1-9]|1[0-2])+(0[1-9]|[12][0-9]|3[01])$");
            if (twd.Length == 7)
            {
                //if (twd.Substring(0, 1) != "-")
                //{
                Regex regex7 = new Regex("(^[0-9]{3}|^-[0-9]{2})+(0[1-9]|1[0-2])+(0[1-9]|[12][0-9]|3[01])$");
                if (!regex7.IsMatch(twd))
                    return false;
                //}
            }

            //HH
            if (twd.Length >= 9)
            {
                try
                {
                    Int16 t = Int16.Parse(twd.Substring(7, 2));

                    if (t < 0 || t > 23)
                        return false;

                }
                catch (Exception)
                {
                    return false;
                }
            }

            //MM
            if (twd.Length >= 11)
            {
                try
                {
                    Int16 t = Int16.Parse(twd.Substring(9, 2));
                    if (t < 0 || t > 59)
                        return false;

                }
                catch (Exception)
                {
                    return false;
                }
            }

            //SS
            if (twd.Length >= 13)
            {
                try
                {

                    Int16 t = Int16.Parse(twd.Substring(11, 2));
                    if (t < 0 || t > 59)
                        return false;

                }
                catch (Exception)
                {
                    return false;
                }
            }

            if (twd.Substring(0, 1) == "-")
                y = 1910 + Int16.Parse(twd.Substring(0, 3));
            else
                y = 1911 + Int16.Parse(twd.Substring(0, 3));

            acDate = y.ToString() + twd.Substring(3, 4);

            IFormatProvider ifp = new CultureInfo("zh-TW", true);

            return DateTime.TryParseExact(acDate, "yyyyMMdd", ifp, DateTimeStyles.None, out dt);

        }

        //驗證西元日期格式
        public bool isDate(string strDate)
        {
            DateTime dt;

            if (strDate.Length < 8)
            {
                Debug.Print("日期不足8碼");
                return false;
            }


            /*驗證日期格式 YYYYMMDD, 範圍00010101~99991231 回傳不符合格式*/
            if (strDate.Length == 8)
            {
                //if (twd.Substring(0, 1) != "-")
                //{
                Regex regex8 = new Regex("(^[0-9]{4}|^-[0-9]{2})+(0[1-9]|1[0-2])+(0[1-9]|[12][0-9]|3[01])$");
                if (!regex8.IsMatch(strDate))
                    return false;
                //}
            }

            IFormatProvider ifp = new CultureInfo("en-US", true);

            return DateTime.TryParseExact(strDate, "yyyyMMdd", ifp, DateTimeStyles.None, out dt);
        }


        //驗證時間格式,範圍從000000~235959,若格式符合,傳回True
        public bool isTime(string time)
        {
            if (time.Length != 6)
            {
                Debug.Print("時間需等於6碼");
                return false;
            }

            Regex regex = new Regex(@"(([0-1][0-9])|([2][0-3]))+([0-5][0-9])+([0-5][0-9])");
            if (regex.IsMatch(time))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool isTime4(string time)
        {
            if (time.Length != 4)
            {
                Debug.Print("時間需等於6碼");
                return false;
            }

            Regex regex = new Regex(@"(([0-1][0-9])|([2][0-3]))+([0-5][0-9])");
            if (regex.IsMatch(time))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}

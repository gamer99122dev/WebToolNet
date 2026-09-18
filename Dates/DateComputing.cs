using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebToolNet.Extensions;
using WebToolNet.Validation;

namespace WebToolNet.Dates
{
    public class DateComputing
    {
        private DateTime _dt;
        private CheckDate _cdate;
        private StringCut _strcut = new StringCut();

        private const double _YearOfDays = 365.25;

        public DateComputing()
        {
            _cdate = new CheckDate();
        }

        public DateTime DT
        {
            get { return _dt; }
            set { _dt = value; }
        }

        // 民國年月日(時分秒可有可無) 轉 西元年月日(時分秒可有可無)
        // 傳回字串格式
        public string TWD2AC_STR(string twd)
        {
            return "";
        }



        //傳入民國年月日，傳回西元 DateTime 物件
        public DateTime TWD2DateTime(string twd, bool WellFormCheck = true)
        {

            if (WellFormCheck && !_cdate.isTWDate(twd))
                return DateTime.Now;

            DateTime dt;
            int y, m, d;

            if (twd.Substring(0, 1) == "-")
                y = 1912 + Int16.Parse(twd.Substring(0, 3));
            else
                y = 1911 + Int16.Parse(twd.Substring(0, 3));
            m = Int16.Parse(twd.Substring(3, 2));
            d = Int16.Parse(twd.Substring(5, 2));

            dt = new DateTime(y, m, d, 0, 0, 0);
            return dt;
        }

        //傳入民國年月日，傳回西元 DateTime 物件
        public string TWD2ChineseTWD(string twd)
        {
            string y, m, d;
            y = _strcut.Left(twd, 3);
            m = twd.Substring(3, 2);
            d = twd.Substring(5, 2);

            if (y.IndexOf("-") > 0)
                return "民國前" + y.Replace("-", "") + "年" + m + "月" + d + "日";
            else
                return "民國" + y + "年" + m + "月" + d + "日";
        }


        //傳入民國年月日時分秒，傳回西元 DateTime 物件
        public DateTime TWDHMS2DateTime(string twd, bool WellFormCheck = true)
        {

            if (WellFormCheck && !_cdate.isTWDate(twd))
                return DateTime.Now;

            DateTime dt;
            int y, m, d, hh = 0, mm = 0, ss = 0;
            if (twd.Substring(0, 1) == "-")
                y = 1912 + Int16.Parse(twd.Substring(0, 3));
            else
                y = 1911 + Int16.Parse(twd.Substring(0, 3));

            m = Int16.Parse(twd.Substring(3, 2));
            d = Int16.Parse(twd.Substring(5, 2));

            hh = Int16.Parse(twd.Substring(7, 2));
            mm = Int16.Parse(twd.Substring(9, 2));
            ss = Int16.Parse(twd.Substring(11, 2));

            dt = new DateTime(y, m, d, hh, mm, ss);
            return dt;
        }

        // 輸入民國年月日(時分秒可有可無) 計算出整數年齡
        // 傳回字串格式
        // twd: 出生日
        public string TWDsAge(string twd, bool WellFormCheck = true)
        {
            if (WellFormCheck && !_cdate.isTWDate(twd))
                return "0";

            DateTime birthday = this.TWD2DateTime(twd, WellFormCheck);
            DateTime today = DateTime.Now;
            TimeSpan ts = new TimeSpan(today.Ticks - birthday.Ticks);
            int age = (int)(ts.Days / _YearOfDays);

            //if (birthday.Month == today.Month && birthday.Day > today.Day)
            //    age--;
            if (age < 0)
            {
                age = 0;
            }
            return age.ToString();
        }


        // 輸入民國年月日(時分秒可有可無) 計算出整數年齡
        // 傳回字串格式
        // twd:出生日、baseDate:基準日
        public string TWDsAge(string twd, string baseDate, bool WellFormCheck = true)
        {
            if (WellFormCheck && (!_cdate.isTWDate(twd) || !_cdate.isTWDate(baseDate)))
                return "0";

            DateTime DTbirthday = this.TWD2DateTime(twd);
            DateTime DTbaseDate = this.TWD2DateTime(baseDate);

            if (DTbaseDate < DTbirthday)
                return "0";

            TimeSpan ts = new TimeSpan(DTbaseDate.Ticks - DTbirthday.Ticks);
            int age = (int)(ts.Days / _YearOfDays);


            //if (DTbirthday.Month == DTbaseDate.Month && DTbirthday.Day > DTbaseDate.Day)
            //    age--;

            if (age < 0)
            {
                age = 0;
            }
            return age.ToString();
        }



        // 輸入民國年月日(時分秒可有可無) 計算出年齡 幾月(不含幾歲，只有月)
        // 傳回字串格式
        public string TWDsAgeMonth(string twd)
        {
            if (!_cdate.isTWDate(twd))
                return "0";

            DateTime birthday = this.TWD2DateTime(twd);
            TimeSpan ts = new TimeSpan(DateTime.Now.Ticks - birthday.Ticks);
            int result = (int)(ts.Days / _YearOfDays);

            //把年回扣掉。
            DateTime t = DateTime.Now;
            t = t.AddYears(result * -1);
            ts = new TimeSpan(t.Ticks - birthday.Ticks);
            result = ts.Days / 31;

            return result.ToString();
        }


        // 輸入民國年月日(時分秒可有可無) 計算出年齡 幾月(不含幾歲，只有月)
        // 傳回字串格式
        // twd:出生日、baseDate:基準日
        public string TWDsAgeMonth(string twd, string baseDate)
        {
            if (!_cdate.isTWDate(twd) || !_cdate.isTWDate(baseDate))
                return "0";

            DateTime DTbirthday = this.TWD2DateTime(twd);
            DateTime DTbaseDate = this.TWD2DateTime(baseDate);

            if (DTbaseDate < DTbirthday)
                return "0";

            TimeSpan ts = new TimeSpan(DTbaseDate.Ticks - DTbirthday.Ticks);
            int result = (int)(ts.Days / _YearOfDays);

            //把年回扣掉。
            DTbaseDate = DTbaseDate.AddYears(result * -1);
            ts = new TimeSpan(DTbaseDate.Ticks - DTbirthday.Ticks);
            result = ts.Days / 31;

            return result.ToString();
        }

        public int TWDsTodayDiff(string twd, bool WellFormCheck = true)
        {
            DateTime DTdiff = this.TWDHMS2DateTime(twd + "235959", WellFormCheck);
            TimeSpan ts1 = DTdiff - DateTime.Now;
            return ts1.Days;
        }

        public int TWDsDiff(string twde, string twds, bool WellFormCheck = true)
        {
            DateTime DTS = this.TWDHMS2DateTime(twds + "000000", WellFormCheck);
            DateTime DTE = this.TWDHMS2DateTime(twde + "235959", WellFormCheck);
            TimeSpan ts1 = DTE - DTS;
            return ts1.Days;
        }



        public string DT2Week(DateTime dt)
        {
            switch (dt.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    return "星期一";

                case DayOfWeek.Tuesday:
                    return "星期二";

                case DayOfWeek.Wednesday:
                    return "星期三";

                case DayOfWeek.Thursday:
                    return "星期四";

                case DayOfWeek.Friday:
                    return "星期五";

                case DayOfWeek.Saturday:
                    return "星期六";

                case DayOfWeek.Sunday:
                    return "星期日";

                default:
                    return "";
            }
        }

        public string DT2WeekDay(DateTime dt)
        {
            switch (dt.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    return "1";

                case DayOfWeek.Tuesday:
                    return "2";

                case DayOfWeek.Wednesday:
                    return "3";

                case DayOfWeek.Thursday:
                    return "4";

                case DayOfWeek.Friday:
                    return "5";

                case DayOfWeek.Saturday:
                    return "6";

                case DayOfWeek.Sunday:
                    return "7";
                default:
                    return "";
            }
        }

        public string WeekDaytoWeek(int day)
        {
            switch (day)
            {
                case 1:
                    return "星期一";

                case 2:
                    return "星期二";

                case 3:
                    return "星期三";

                case 4:
                    return "星期四";

                case 5:
                    return "星期五";

                case 6:
                    return "星期六";

                case 7:
                    return "星期日";

                default:
                    return "";
            }
        }

        public DateTime FirstDay(DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, 1);
        }

        public DateTime LastDay(DateTime dt)
        {
            return new DateTime(dt.AddMonths(1).Year, dt.AddMonths(1).Month, 1).AddDays(-1);
        }


        public string TWDsDateSubtract(string BDate, string EDate)
        {
            if (!_cdate.isTWDate(BDate) || !_cdate.isTWDate(EDate))
                return "0";

            DateTime Date1 = this.TWD2DateTime(BDate);
            DateTime Date2 = this.TWD2DateTime(EDate);

            TimeSpan ts = new TimeSpan();

            if (Date1 > Date2)
            {
                ts = Date1.Subtract(Date2);
            }
            else
            {
                ts = Date2.Subtract(Date1);
            }
            if (ts.Days == 0)
            {
                return "1";
            }
            else
            {
                return ts.Days.ToString();
            }

        }

        /// <summary>
        /// 傳入民國年的出生年月日，回傳目前共幾個月
        /// </summary>
        /// <param name="twd">民國年月日</param>
        /// <returns>字串格式的月份數</returns>
        public string TWDsMonths(string twd)
        {
            if (!_cdate.isTWDate(twd))
                return "0";

            DateTime birthday = this.TWD2DateTime(twd);
            TimeSpan ts = new TimeSpan(DateTime.Now.Ticks - birthday.Ticks);

            int y = (int)(ts.Days / _YearOfDays);
            DateTime t = DateTime.Now;
            t = t.AddYears(y * -1);
            ts = new TimeSpan(t.Ticks - birthday.Ticks);

            return (ts.Days / 31 + y * 12).ToString();
        }

        /// <summary>
        /// 取得現在時間點的時間序號
        /// </summary>
        /// <returns></returns>
        public Int64 DateTimeSerialNumber()
        {
            DateTime baseDateTime = new DateTime(1970, 1, 1, 0, 0, 0);
            TimeSpan ts = DateTime.Now.Subtract(baseDateTime);
            return Convert.ToInt64(ts.TotalSeconds);
        }

        /// <summary>
        /// 取得傳入時間的時間序號
        /// </summary>
        /// <param name="DTM">民國年月日時分秒字串:yyymmddHHMMSS</param>
        /// <returns></returns>
        public Int64 DateTimeSerialNumber(string DTM)
        {
            if (DTM.Length != 13 || !_cdate.isTWDate(DTM.Substring(0, 7)))
                return 0;

            DateTime baseDateTime = new DateTime(1970, 1, 1, 0, 0, 0);
            DateTime dt = new DateTime();
            //if (DTM.Length == 7) {
            //    dt = TWD2DateTime(DTM);
            //} else if (DTM.Length == 11) {
            //    dt = TWDHMS2DateTime(DTM + "00");
            //} else if (DTM.Length == 13) {
            dt = TWDHMS2DateTime(DTM);
            //}

            TimeSpan ts = dt.Subtract(baseDateTime);
            return Convert.ToInt64(ts.TotalSeconds);

        }


    }
}

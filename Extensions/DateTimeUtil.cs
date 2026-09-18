using System;
using WebToolNet.Dates;

namespace WebToolNet.Extensions
{
    public static class DateTimeUtil
    {
        public static bool pBetween(this DateTime src, DateTime Sdate, DateTime Edate)
        {
            return src >= Sdate && src <= Edate;
        }

        public static bool pNotBetween(this DateTime src, DateTime Sdate, DateTime Edate)
        {
            return !src.pBetween(Sdate, Edate);
        }


        public static string pRyyy(this DateTime src)
        {
            GenerateDateString gds = new GenerateDateString(src);
            return gds.Ryyy;
        }

        public static string pRyyymm(this DateTime src)
        {
            GenerateDateString gds = new GenerateDateString(src);
            return gds.Ryyymm;
        }

        public static string pRyyymmdd(this DateTime src)
        {
            GenerateDateString gds = new GenerateDateString(src);
            return gds.Ryyymmdd;
        }

        public static string pRyyymmddhhmm(this DateTime src)
        {
            GenerateDateString gds = new GenerateDateString(src);
            return gds.Ryyymmddhhmm;
        }

        public static string pRyyymmddhhmmss(this DateTime src)
        {
            GenerateDateString gds = new GenerateDateString(src);
            return gds.Ryyymmdd + gds.hhmmss;
        }


        public static string pRyyymmddhhMMssffff(this DateTime src)
        {
            GenerateDateString gds = new GenerateDateString(src);
            return gds.Ryyymmdd + gds.hhmmssffff;
        }



        public static string pSRyyymmdd(this DateTime src)
        {
            GenerateDateString gds = new GenerateDateString(src);

            return gds.SRyyymmdd;
        }

        public static string pSRyyymmddhhmm(this DateTime src)
        {
            GenerateDateString gds = new GenerateDateString(src);
            return gds.SRyyymmdd + " " + gds.Chhmm;
        }

        public static string pSRyyymmddhhmmss(this DateTime src)
        {
            GenerateDateString gds = new GenerateDateString(src);
            return gds.SRyyymmdd + " " + gds.Chhmmss;
        }

        // 客制化輸出民國年月日的 數字年後面的部分
        // dt.pToCString("年MM月dd日") 輸出如: 108年02月15日
        public static string pToCString(this DateTime dt, string format = "年MM月dd日")
        {
            string sYear = "000" + Convert.ToString(dt.AddYears(-1911).Year);
            sYear = sYear.Substring(sYear.Length - 3, 3);
            return sYear + dt.ToString(format);
        }


        public static string pyyyymmdd(this DateTime src)
        {
            GenerateDateString gds = new GenerateDateString(src);
            return gds.yyyymmdd;
        }

        // 西元年
        // 輸出如: 10/15/2021
        public static string pSmmddyyyy(this DateTime src)
        {
            GenerateDateString gds = new GenerateDateString(src);
            return gds.Smmddyyyy;
        }


        public static string pSyyyymmddhhmmss(this DateTime src)
        {
            GenerateDateString gds = new GenerateDateString(src);
            return gds.Syyyymmddhhmmss;
        }

        public static string pDyyyymmdd(this DateTime src)
        {
            GenerateDateString gds = new GenerateDateString(src);
            return gds.Dyyyymmdd;
        }

        // 時間經過了多久, baseDTM(Now) - src
        // 格式:  依TimeDiffType回傳double
        public static double pTimeDiff(this DateTime src, TimeDiffType type, DateTime baseDTM = default(DateTime))
        {
            if (baseDTM == default(DateTime))
            {
                baseDTM = DateTime.Now;
            }

            TimeSpan ts = baseDTM - src;

            if (type == TimeDiffType.Days)
                return ts.TotalDays;
            else if (type == TimeDiffType.Hours)
                return ts.TotalHours;
            else if (type == TimeDiffType.Minutes)
                return ts.TotalMinutes;
            else if (type == TimeDiffType.Seconds)
                return ts.TotalSeconds;
            else if (type == TimeDiffType.Milliseconds)
                return ts.TotalMilliseconds;
            else return -1;
        }

        // 時間經過了多久, baseDTM(Now) - src
        // 格式:  1:02:30:05
        public static string pTimeDiff2(this DateTime src, DateTime baseDTM = default(DateTime))
        {
            if (baseDTM == default(DateTime))
            {
                baseDTM = DateTime.Now;
            }

            TimeSpan ts = baseDTM - src;

            string elapsed = ts.ToString(@"d\:hh\:mm\:ss");
            return elapsed;
        }

        // 時間經過了多久, baseDTM(Now) - src
        // 格式:  2天3小時3分20秒
        // 格式:  4小時20分10秒
        public static string pTimeDiff3(this DateTime src, DateTime baseDTM = default(DateTime))
        {
            if (baseDTM == default(DateTime))
            {
                baseDTM = DateTime.Now;
            }

            TimeSpan ts = baseDTM - src;

            string dd = ts.ToString(@"dd");
            string hh = ts.ToString(@"hh");
            string mm = ts.ToString(@"mm");
            string ss = ts.ToString(@"ss");

            string elapsed = "";

            if (dd != "00")
            {
                elapsed = $"{dd.TrimStart('0')}天";
            }

            if (hh != "00")
            {
                elapsed = $"{elapsed}{hh.TrimStart('0')}小時";
            }

            if (mm != "00")
            {
                elapsed = $"{elapsed}{mm.TrimStart('0')}分";
            }

            if (ss != "00")
            {
                elapsed = $"{elapsed}{ss.TrimStart('0')}秒";
            }
            return elapsed;
        }

        public static string pHHmmss(this DateTime src)
        {
            return src.ToString("HH:mm:ss");
        }

        public static string pCHHmmss(this DateTime src)
        {
            return src.ToString("HH點mm分ss秒");
        }

        // 回傳一週的第一天, 預設以週一為開頭
        public static DateTime pStartOfWeek(this DateTime src, DayOfWeek startOfWeek = DayOfWeek.Monday)
        {
            int diff = (7 + (src.DayOfWeek - startOfWeek)) % 7;
            DateTime start = src.AddDays(-1 * diff).Date;
            return start;
        }

        // 回傳當月的第一天
        public static DateTime pStartOfMonth(this DateTime src, DayOfWeek startOfWeek = DayOfWeek.Monday)
        {
            DateTime start = (new DateTime(src.Year, src.Month, 1));
            return start;
        }

        public static string pToDayOfWeekCh(this DateTime dtm)
        {
            string[] WeekDays = { "日", "一", "二", "三", "四", "五", "六" };
            return WeekDays[((int)dtm.DayOfWeek)];
        }


        public static string pToDayOfWeek(this DateTime dtm)
        {
            int DayofWeek = 0;
            if (dtm.DayOfWeek == DayOfWeek.Sunday)
            {
                DayofWeek = 7;
            }
            else
            {
                DayofWeek = (int)dtm.DayOfWeek;
            }

            return DayofWeek.ToString();
        }



        public static bool pIsToday(this DateTime dtm)
        {
            return dtm.Date == DateTime.Today;
        }


    }
}

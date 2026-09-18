using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using WebToolNet.Data;
using WebToolNet.Dates;
using WebToolNet.Validation;

namespace WebToolNet.Extensions
{
    public enum TimeDiffType
    {
        Days,
        Hours,
        Minutes,
        Seconds,
        Milliseconds
    }

    #region String
    public static class StringTool
    {
        public static string ToAlignedString(this DataTable table)
        {
            StringBuilder sb = new StringBuilder();
            int maxWidth = 20;

            // Write column headers
            for (int i = 0; i < table.Columns.Count; i++)
            {
                sb.Append(table.Columns[i].ColumnName.PadRight(maxWidth));
                sb.Append(" || ");
            }
            sb.AppendLine();

            // Write rows
            foreach (DataRow row in table.Rows)
            {
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    string cell = row[i].ToString();
                    while (cell.Length > maxWidth)
                    {
                        sb.Append(cell.Substring(0, maxWidth));
                        sb.Append(" || ");
                        cell = cell.Substring(maxWidth);
                    }
                    sb.Append(cell.PadRight(maxWidth));
                    sb.Append(" || ");
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }
        // -----------------

        public static string pNullOrTrim(this String str)
        {

            if (str == null)
            {
                str = string.Empty;
            }

            str = str.Trim();

            return str;

        }

        public static string pNullOrTrimTwo(this object obj)
        {
            return (obj ?? "").ToString().Trim();
        }

        public static string pLeft(this String sSource, int iLength)
        {
            sSource = sSource.pNullOrTrim();
            return sSource.Substring(0, iLength > sSource.Length ? sSource.Length : iLength);
        }

        public static string pRight(this String sSource, int iLength)
        {
            sSource = sSource.pNullOrTrim();
            return sSource.Substring(iLength > sSource.Length ? 0 : sSource.Length - iLength);
        }

        public static string pMid(this String sSource, int iStart, int iLength)
        {
            sSource = sSource.pNullOrTrim();
            int iStartPoint = iStart > sSource.Length ? sSource.Length : iStart;
            return sSource.Substring(iStartPoint, iStartPoint + iLength > sSource.Length ? sSource.Length - iStartPoint : iLength);
        }

        public static int pToByteCount(this String sSource)
        {
            if (sSource == null)
                return 0;
            return StringCut.Big5.GetByteCount(sSource);
        }

        public static string pSubStringByte(this String sSource, int start, int end)
        {
            if (sSource == null)
                return string.Empty;

            StringCut stringCut = new StringCut();

            return stringCut.SubStrginByte(sSource, start, end);
        }


        public static string pSQLValidator(this String sSource)
        {
            SqlEscape ckSQL = new SqlEscape();
            return ckSQL.SQLValidator(sSource);
        }

        public static string pRowfilterValidator(this String sSource)
        {
            SqlEscape ckSQL = new SqlEscape();
            return ckSQL.RowfilterValidator(sSource);
        }


        /// <summary>
        /// 支援的跳脫字元，見EscapeRowfilter的定義。
        /// </summary>
        /// <param name="sSource"></param>
        /// <returns></returns>
        public static string pEscapeRowfilter(this String sSource)
        {
            SqlEscape ckSQL = new SqlEscape();
            return ckSQL.EscapeRowfilter(sSource);
        }

        public static string ConvertObjectToString(object obj)
        {
            if (obj == null)
            {
                return string.Empty;
            }

            return obj.ToString() ?? string.Empty;
        }

        // "AA".pIn("123", "AA", "ABC") : 斷判AA 是否包含在 123, AA, ABC 內
        public static bool pIn(this String sSource, params object[] args)
        {
            if (sSource == null)
                return false;
            string[] strs = Array.ConvertAll<object, string>(args, ConvertObjectToString);
            return strs.Contains(sSource);
        }

        public static bool pNotIn(this String sSource, params object[] args)
        {
            bool val = !sSource.pIn(args);
            return val;
        }

        public static string pAgeMonth(this string Birthday, string Base = null)
        {

            DateComputing dc = new DateComputing();

            if (Birthday.pIsTWDate() == false)
            {
                return string.Empty;
            }

            string AgeMonth = "";
            if (Base == null)
            {
                AgeMonth = dc.TWDsAgeMonth(Birthday);
            }
            else
            {
                AgeMonth = dc.TWDsAgeMonth(Birthday, Base);
            }

            return AgeMonth;

        }

        // "AB123".pContainsAny("1", "A", "C") : 斷判AB123 是否包含任一 1, A, C
        public static bool pContainsAny(this String sSource, params object[] args)
        {
            if (sSource == null)
                return false;

            foreach (string s in Array.ConvertAll<object, string>(args, ConvertObjectToString))
            {
                if (sSource.Contains(s))
                    return true;
            }

            return false;
        }

        // "AB123".pContainsAll("1", "A", "C") : 斷判AB123 是否包含全部 1, A, C
        public static bool pContainsAll(this String sSource, params object[] args)
        {
            if (sSource == null)
                return false;

            foreach (string s in Array.ConvertAll<object, string>(args, ConvertObjectToString))
            {
                if (!sSource.Contains(s))
                    return false;
            }

            return true;
        }

        // "23".pContainedBy("123", "456", "789") : 斷判23 是否被123,456,789任一所包含
        public static bool pContainedBy(this String sSource, params object[] args)
        {
            if (sSource == null)
                return false;

            foreach (string s in Array.ConvertAll<object, string>(args, ConvertObjectToString))
            {
                if (s.Contains(sSource))
                    return true;
            }

            return false;
        }

        // "A".pEqualsAny("1", "A", "C") : 斷判A 是否等於任一 1, A, C
        public static bool pEqualsAny(this String sSource, params object[] args)
        {
            if (sSource == null)
                return false;

            foreach (string s in Array.ConvertAll<object, string>(args, ConvertObjectToString))
            {
                if (sSource == s)
                    return true;
            }

            return false;
        }

        // "A".{("1", "A", "C") : 斷判A 是否等於全部 1, A, C
        public static bool pEqualsAll(this String sSource, params object[] args)
        {
            if (sSource == null)
                return false;

            foreach (string s in Array.ConvertAll<object, string>(args, ConvertObjectToString))
            {
                if (sSource != s)
                    return false;
            }

            return true;
        }

        // 字串取代, 傳入StringComparison.OrdinalIgnoreCase時, 可不分大小寫取代
        // 例: "123abc789".pReplace("ABC", "456", StringComparison.OrdinalIgnoreCase)
        //     會取代成 "123456789"
        public static string pReplace(this string source, string oldString, string newString, StringComparison comp)
        {
            bool MatchFound = false;
            do
            {
                int index = source.IndexOf(oldString, comp);

                // Determine if we found a match
                MatchFound = index >= 0;

                if (MatchFound)
                {
                    // Remove the old text
                    source = source.Remove(index, oldString.Length);

                    // Add the replacemenet text
                    source = source.Insert(index, newString);
                }
            } while (MatchFound);

            return source;
        }

        public static string pReplaceNewLine(this string source, string treat = "")
        {
            return source.Replace("\r\n", treat).Replace("\n", treat);
        }

        public static float pToFloat(this string source)
        {
            float dec = 0;
            float.TryParse(source, out dec);
            return dec;
        }
        public static decimal pToDecimal(this string source)
        {
            decimal dec = 0;
            decimal.TryParse(source, out dec);
            return dec;
        }


        public static int pToInt(this string source)
        {
            int i = 0;
            int.TryParse(source, out i);
            return i;
        }

        // 轉整數
        //   MidpointRounding.AwayFromZero : 正常的四捨五入 (預設)
        //     Math.Round(8.5, MidpointRounding.AwayFromZero); //結果為 9
        //
        //   MidpointRounding.ToEven       : 四捨六入五成雙 (奇進偶捨、銀行進位法)
        //     Math.Round(5.5, MidpointRounding.ToEven);       //結果為 6
        //     Math.Round(8.5, MidpointRounding.ToEven);       //結果為 8
        public static int pToIntRound(this string source, MidpointRounding Rounding = MidpointRounding.AwayFromZero)
        {
            double d = source.pToDouble();
            d = Math.Round(d, Rounding);
            int i = Convert.ToInt32(d);
            return i;
        }

        // 將數字格式化成顯示數字 1234567 => 1,234,567
        public static string pToIntN(this string source)
        {
            double d = 0;
            Double.TryParse(source, out d);
            return d.ToString("N0");
        }

        public static Double pToDouble(this string source)
        {
            Double d = 0;
            Double.TryParse(source, out d);
            return d;
        }


        /// <summary>
        /// 字串是否介於Low跟High之間 (從String Compare的角度)
        /// </summary>
        /// <param name="low">下限</param>
        /// <param name="high">上限</param>
        /// <returns></returns>
        public static bool pBetween(this string source, string low, string high)
        {
            return (low.Length > 0 && string.Compare(source, low) >= 0)
                && (high.Length > 0 && string.Compare(source, high) <= 0);
        }

        public static bool pNotBetween(this string source, string low, string high)
        {
            return !pBetween(source, low, high);
        }

        public static bool pGreaterThan(this string source, string compare, bool IncludeEuqal = true)
        {
            if (IncludeEuqal)
            {
                return (compare.Length > 0 && String.Compare(source, compare) >= 0);
            }

            return (compare.Length > 0 && String.Compare(source, compare) > 0);
        }

        public static bool pLessThan(this string source, string compare, bool IncludeEuqal = true)
        {
            if (IncludeEuqal)
            {
                return (compare.Length > 0 && String.Compare(source, compare) <= 0);
            }

            return (compare.Length > 0 && String.Compare(source, compare) < 0);
        }

        public static void pToConsole(this string source, bool ConsoleDebug = true)
        {
            if (ConsoleDebug)
                Console.WriteLine(source);
        }

        public static void pToConsole(this string source, string prefix, bool ConsoleDebug = true)
        {
            if (ConsoleDebug)
                Console.WriteLine(prefix + source);
        }

        // 以字串切string, ex: "ab cd ef".pSplit("cd") = ["ab","ef"]
        public static string[] pSplit(this string source, string separator, StringSplitOptions options = StringSplitOptions.None, bool Trim = true)
        {
            string[] arr = source.Split(new string[] { separator }, options);
            if (Trim)
            {
                for (int i = 0; i < arr.Length; i++)
                    arr[i] = arr[i].Trim();
            }
            return arr;
        }

        /// <summary>
        /// 字串轉半形
        /// </summary>
        /// <param name="source">任一字元串</param>
        /// <returns>半形字元串</returns>
        public static string pToNarrow(this string source)
        {
            char[] c = source.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] == 12288)
                {
                    c[i] = (char)32;
                    continue;
                }
                if (c[i] > 65280 && c[i] < 65375)
                    c[i] = (char)(c[i] - 65248);
            }
            return new string(c);
        }

        /// <summary>
        /// 取代系統造字。Big5 造字區的字進到 .NET 會落在 Unicode 私人使用區 (U+E000-U+F8FF)，
        /// 在沒裝造字檔的機器上是亂碼或空白，畫面顯示一律換成 ○
        /// </summary>
        public static string pReplaceEUDC(this string source, string replacement = "○")
        {
            if (source == null)
                return string.Empty;

            return Regex.Replace(source, "[\\uE000-\\uF8FF]", replacement);
        }

        /// <summary>
        /// 民國年月日, 長度支援7,11,13碼 ex:1070830, 10708301423, 1070830142305
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static DateTime pToDateTime(this string source, bool WellFormCheck = true)
        {
            DateComputing dc = new DateComputing();

            DateTime d = DateTime.Now;
            if (source.Length == 7)
            {
                d = dc.TWD2DateTime(source, WellFormCheck);
            }
            else if (source.Length == 11)
            {
                d = dc.TWDHMS2DateTime(source + "00", WellFormCheck);
            }
            else if (source.Length == 13)
            {
                d = dc.TWDHMS2DateTime(source, WellFormCheck);
            }

            return d;
        }


        /// <summary>
        /// 西元年月日 ex:2018/08/30
        /// </summary>
        /// <param name="source"></param>
        /// <param name="format">日期格式</param>
        /// <returns></returns>
        public static DateTime pParseDateTime(this string source, string format = "yyyy/MM/dd")
        {
            return DateTime.ParseExact(source, format, CultureInfo.InvariantCulture);
        }

        public static string pTWDateFormat(this string src, string delimiter = "/")
        {

            if (src.pIsTWDate() == false)
            {
                throw new Exception("非民國日期格式(yyy/MM/dd)");
            }

            return $"{src.pLeft(3)}{delimiter}{src.pMid(3, 2)}{delimiter}{src.pRight(2)}";


        }



        public static bool pIsTWDate(this string src, int expectLength = 0)
        {
            // Caller指定預期日期長度
            // 11104012為錯誤民國日期, 但過得了isTWDate()的檢核
            if (expectLength > 0 && src.Length != expectLength)
            {
                return false;
            }

            CheckDate _dateChecker = new CheckDate();
            bool isValid = _dateChecker.isTWDate(src);
            return isValid;
        }

        public static bool pIsTime4(this string src, int expectLength = 0)
        {
            CheckDate _dateChecker = new CheckDate();
            bool isValid = _dateChecker.isTime4(src);
            return isValid;
        }



        // 是否西元日期
        public static bool pIsDate(this string src)
        {
            CheckDate _dateChecker = new CheckDate();
            bool isValid = _dateChecker.isDate(src);
            return isValid;
        }

        public static string pNewLine(this string source)
        {
            return Regex.Replace(source, "(?<!\r)\n", "\r\n"); ;
        }

        public static string pAppend(this string source, string text, string separator = ",", bool AppendEmpty = true)
        {
            if (AppendEmpty == false && text.pEmpty())
            {
                return source;
            }

            return string.Format("{0}{1}{2}", source, source.Length > 0 ? separator : "", text);
        }

        public static bool pIsLetter(this string source)
        {
            return source.All(char.IsLetter);
        }

        public static bool pIsDigit(this string source)
        {
            return source.All(char.IsDigit);
        }

        // 是否數字, 考慮小數點
        public static bool pIsNumeric(this string source)
        {
            return double.TryParse(source, out double dummy);
        }

        // 取代特殊字元成檔名相容字元 (未驗證)
        public static string pToValidFileName(this string filename)
        {
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            {
                filename = filename.Replace(c, '_');
            }
            return filename;
        }

        public static string pAge(this string Birthday, string Base = null)
        {
            DateComputing dc = new DateComputing();
            string age = "";
            if (Base == null)
            {
                age = dc.TWDsAge(Birthday);
            }
            else
            {
                age = dc.TWDsAge(Birthday, Base);
            }
            return age;
        }

        public static string pIndent(this string src, string spaces = "    ")
        {
            string s = spaces + src.Replace("\n", "\n" + spaces);
            return s;
        }

        // 把字串上下左右填入空白, 例如放入ToolTip時會比較好閱讀
        // 注意Caller要指定Newline是\n或是\r\n
        public static string pSurroundSpace(this string src, string prefixSpace = "  ", string suffixSpace = "  ", int topLines = 1, int bottomLines = 1, string NewLine = "\n")
        {
            // 上方空的行數
            string top = "";
            for (int i = 0; i < topLines; i++)
            {
                top = top + "  " + NewLine;
            }

            // 下方空的行數
            string bottom = "";
            for (int i = 0; i < bottomLines; i++)
            {
                bottom = bottom + NewLine + "  ";
            }

            string s = top;

            s += prefixSpace;

            s += src.Replace(NewLine, prefixSpace + NewLine + suffixSpace);

            s += bottom;

            return s;
        }

        public static bool pEmpty(this string src)
        {
            bool empty = !src.pAny();
            return empty;
        }

        public static bool pAny(this string src)
        {
            bool any = (src != null && src.Trim() != "");
            return any;
        }

        // 傳入 ("0", "女", "1", "男")
        // Mapping: 0 => 女, 1=> 男
        public static string pMapValue(this string src, params string[] KeyValuePairArray)
        {
            if (KeyValuePairArray.Length % 2 != 0)
            {
                // error
                return "";
            }
            string key = src.pNullOrTrim();
            for (int i = 0; i < KeyValuePairArray.Length; i += 2)
            {
                if (key == KeyValuePairArray[i])
                    return KeyValuePairArray[i + 1];
            }
            return "";
        }

        public static string pMapValueWithUnknown(this string src, string unknownVal, params string[] KeyValuePairArray)
        {
            if (KeyValuePairArray.Length % 2 != 0)
            {
                // error
                return "";
            }
            string key = src.pNullOrTrim();
            for (int i = 0; i < KeyValuePairArray.Length; i += 2)
            {
                if (key == KeyValuePairArray[i])
                    return KeyValuePairArray[i + 1];
            }

            return unknownVal;
        }

        /*
         * ex: 
         *  public enum  Weeks
         *  {
         *    [Description("未知")]
         *    Unknown,
         *    [Description("星期一")]
         *    Monday,
         *    [Description("星期二")]
         *    Tuesday,
         *    [Description("星期三")]
         *    Wednesday
         *  }
         *  defaultValue:Weeks.Unknown
         *  傳入"星期一"字串，就對應Description中的文字，符合就回傳 "Monday" 
         *  若找不到列舉中Description，回傳"Unknown" 
         */
        //public static Enum pToEnum<T>(this string src,string defaultValue)
        public static Enum pToEnum<T>(this string src, Enum defaultValue)
        {

            Array values = Enum.GetValues(typeof(T));

            foreach (Enum val in values)
            {
                if (src == val.pToDescription())
                {
                    return val;
                }
            }

            return defaultValue;
        }


        public static double pTimeDiff(this string src, TimeDiffType type, string baseDTMstr = "")
        {
            DateTime baseDTM = DateTime.Now;
            if (baseDTMstr != "")
                baseDTM = baseDTMstr.pToDateTime(WellFormCheck: false);

            DateTime srcDTM = src.pToDateTime(WellFormCheck: false);
            double diff = srcDTM.pTimeDiff(type, baseDTM);
            return diff;
        }

        //字串切成指定byte 參數(byLen 指定int byte)
        public static string pGetByteString(this string st, int byLen)
        {
            string q = "";
            for (int i = 1; i <= byLen; i++)
            {
                if (StringCut.Big5.GetByteCount(st.pLeft(st.Length - i)) <= byLen)
                {
                    q = st.pLeft(st.Length - i);
                    break;
                }
            }
            return q;
        }

        public static string pLeftByte(this string st, int byLen)
        {
            if (st.pEmpty())
            {
                return st;
            }

            if (byLen == 0)
            {
                return string.Empty;
            }

            if (st.pToByteCount() < byLen)
            {
                return st;
            }

            bool isBreak = false;
            string q = string.Empty;
            for (int i = byLen; i > 0; i--)
            {
                q = st.pLeft(i);
                if (q.pToByteCount() <= byLen)
                {
                    isBreak = true;
                    break;
                }
            }

            if (isBreak)
            {
                return q;
            }
            else
            {
                return string.Empty;
            }
        }



        // 以Byte Count補字串長度
        public static string pBytePadRight(this string src, int length, char padChar = ' ')
        {
            int byteLen = src.pToByteCount();

            int pad = length - byteLen;
            if (pad > 0)
            {
                src += padChar.ToString().PadRight(pad, padChar);
            }
            return src;
        }

        // 傳回第一個非空的字串
        //   string result = str1.Or(str2).Or(str3).Or(str4);
        public static string pOr(this string src, string alternative)
        {
            return src.pAny() ? src : alternative;
        }

        // 民國日期的AddDays
        public static string pAddDays(this string src, int days)
        {
            DateTime date = src.pToDateTime();
            string d = date.AddDays(days).pRyyymmdd();
            return d;
        }

        // 民國日期的AddYears
        public static string pAddYears(this string src, int years)
        {
            DateTime date = src.pToDateTime();
            string d = date.AddYears(years).pRyyymmdd();
            return d;
        }

        public static bool pCheckPID(this string src)
        {
            CheckID check = new CheckID();

            return check.ckTWID(src) || check.ckForeignID(src);
        }

        public static bool pCheckPID(this string src, bool IsTW = true)
        {
            CheckID check = new CheckID();

            if (IsTW == true)
            {
                return check.ckTWID(src);
            }
            else
            {
                return check.ckForeignID(src);
            }
        }

        public static int pCountLines(this string src, bool CountFinalNewline = true)
        {
            string text = src;
            if (!CountFinalNewline)
            {
                text = src.TrimEnd();
            }
            int numLines = text.Split('\n').Length;
            return numLines;
        }

        // true:  "ABC123".pContains("123")   : 看ABC123是否包含123  
        // false: "ABC123".pContains(""123"") : 看ABC123是否等於123  
        //         也就是說，平常以contains比較，但當字串以""(double quote) 包圍時，採行equals比較
        public static bool pContains(this string src, string compareStr, StringComparison compareType = StringComparison.Ordinal, char quoteChar = '"')
        {
            bool match = false;

            // 比較""123""
            if (compareStr.Length >= 2 && compareStr.First() == quoteChar && compareStr.Last() == quoteChar)
            {
                string compareWord = compareStr.pMid(1, compareStr.Length - 2);
                match = String.Equals(src, compareWord, compareType);
            }
            // 比較 "123"
            else
            {
                match = (src.IndexOf(compareStr, compareType) >= 0);
            }

            return match;
        }


        public static bool pIsToday(this string date)
        {
            return date.pLeft(7) == DateTime.Now.pRyyymmdd();
        }

        /// <summary>
        /// 傳入 "10912290800" 
        /// 回傳 例:109/12/29 08:00 
        /// </summary>
        public static string pSyyymmddhhmm(this string twd)
        {
            string strDTM = string.Empty;
            if ((twd.pEmpty() || strDTM.Length < 12) && twd.pIsTWDate() == false)
            {
                return strDTM;
            }

            strDTM = $"{twd.pLeft(3)}/{twd.pMid(3, 2)}/{twd.pMid(5, 2)} {twd.pMid(7, 2)}:{twd.pRight(2)}";

            return strDTM;
        }


        /// <summary>
        /// 傳入 "1091229" 
        /// 回傳 例:109/12/29 
        /// </summary>
        public static string pSyyymmdd(this string twd)
        {
            string strDTM = string.Empty;
            if ((twd.pEmpty() || strDTM.Length < 8) && twd.pIsTWDate() == false)
            {
                return strDTM;
            }

            strDTM = $"{twd.pLeft(3)}/{twd.pMid(3, 2)}/{twd.pMid(5, 2)}";

            return strDTM;
        }


        public static bool pIsHoliday(this string date)
        {

            DateTime dateTime = date.pToDateTime();

            if (dateTime.DayOfWeek.pNotIn(DayOfWeek.Saturday, DayOfWeek.Sunday))
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public static string pToMD5(this string str)
        {
            using (var cryptoMD5 = System.Security.Cryptography.MD5.Create())
            {
                //將字串編碼成 UTF8 位元組陣列
                var bytes = Encoding.UTF8.GetBytes(str);

                //取得雜湊值位元組陣列
                var hash = cryptoMD5.ComputeHash(bytes);

                //取得 MD5
                var md5 = BitConverter.ToString(hash)
                  .Replace("-", string.Empty)
                  .ToUpper();

                return md5;
            }
        }


    }
    #endregion

    #region DataRow
    public static class DataRowTool
    {
        public static string pCol(this DataRow r, int colIndex)
        {
            if (r == null || r[colIndex] == null)
                return "";

            return (r[colIndex] ?? "").ToString().pNullOrTrim();
        }

        public static string pGetColumn(this DataRow r, string ColumnName)
        {
            // tocheck: r.Table.Columns[ColumnName] == null 表示給錯欄位名，如果濾掉錯誤將不易發現
            if (r == null || r[ColumnName] == null)
                return "";

            return (r[ColumnName] ?? "").ToString().pNullOrTrim();
        }

        // pCol = pGetColumn
        public static string pCol(this DataRow r, string ColumnName)
        {
            return pGetColumn(r, ColumnName);
        }

        public static int pGetColumnInt(this DataRow r, string ColumnName)
        {
            String s = pGetColumn(r, ColumnName);
            return Convert.ToInt32(s.Length > 0 ? s : "0");
        }

        public static long pGetColumnLong(this DataRow r, string ColumnName)
        {
            String s = pGetColumn(r, ColumnName);
            return Convert.ToInt64(s.Length > 0 ? s : "0");
        }

        public static double pGetColumnDouble(this DataRow r, string ColumnName, double defaultVal = 0)
        {
            String s = pGetColumn(r, ColumnName);
            double val = defaultVal;
            try
            {
                val = Convert.ToDouble(s);
            }
            catch (Exception)
            {
                val = defaultVal;
            }
            return val;
        }

        // 以欄位substring取值
        //   例如, 欄位為: "身分證號(A12)"
        //   r.pColSub("A12")即可
        public static string pColSub(this DataRow r, string ColumnSubName)
        {
            if (r == null || r.Table == null)
            {
                Exception e1 = new Exception("DataRow 錯誤");
                throw e1;
            }

            foreach (DataColumn c in r.Table.Columns)
            {
                if (c.ColumnName.Contains(ColumnSubName))
                {
                    string s = r.pCol(c.ColumnName);
                    return s;
                }
            }

            Exception e2 = new Exception($"無相關欄位[{ColumnSubName}]");
            throw e2;
        }

        public static string pToString(this DataRow row)
        {
            if (row == null)
                return "DataRow NULL";

            DataTable dt = row.Table;

            var output = new StringBuilder();

            var columnsWidths = new int[dt.Columns.Count];

            // Get column widths
            //foreach (DataRow row in dt.Rows)
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    var length = row[i].ToString().Length;
                    if (columnsWidths[i] < length)
                        columnsWidths[i] = length;
                }
            }

            // Get Column Titles
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                var length = dt.Columns[i].ColumnName.Length;
                if (columnsWidths[i] < length)
                    columnsWidths[i] = length;
            }

            // Write Column titles
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                var text = dt.Columns[i].ColumnName;
                output.Append("|" + PadCenter(text, columnsWidths[i] + 2));
            }
            output.Append("|\n" + new string('=', output.Length) + "\n");

            // Write Rows
            //foreach (DataRow row in dt.Rows)
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    var text = row[i].ToString();
                    output.Append("|" + PadCenter(text, columnsWidths[i] + 2));
                }
                output.Append("|\n");
            }
            return output.ToString();
        }


        public static bool pContains(this DataRow row, string search)
        {
            foreach (DataColumn c in row.Table.Columns)
            {
                if (row[c].ToString().Contains(search))
                {
                    return true;
                }
            }
            return false;
        }

        private static string PadCenter(string text, int maxLength)
        {
            int diff = maxLength - text.Length;
            return new string(' ', diff / 2) + text + new string(' ', (int)(diff / 2.0 + 0.5));
        }

    }
    #endregion

    #region Int
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
    #endregion

    #region Double
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
    #endregion

    #region Enum
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
    #endregion

    #region Dictionary
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
    #endregion

    #region Array
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
    #endregion

    #region Enumerable
    public static class EnumerableTool
    {
        public static void pForeach<T>(this IEnumerable<T> _this, Action<T> action)
        {
            foreach (var cur in _this)
            {
                action(cur);
            }
        }

        public static IEnumerable<TResult> pForeach<T, TResult>(this IEnumerable<T> _this, Func<T, TResult> action)
        {
            foreach (var cur in _this)
            {
                yield return action(cur);
            }
        }

        public static void pForeachWithIndex<T>(this IEnumerable<T> _this, Action<T, int> action)
        {
            int idx = 0;
            foreach (var cur in _this)
            {
                action(cur, idx++);
            }
        }

        public static IEnumerable<TResult> pForeachWithIndex<T, TResult>(this IEnumerable<T> _this, Func<T, int, TResult> action)
        {
            int idx = 0;
            foreach (var cur in _this)
            {
                yield return action(cur, idx++);
            }
        }

        public static IEnumerable<TSource> pDistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> knownKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (knownKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }
        /// <summary>
        /// 泛型集合轉DataTable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="dtName"></param>
        /// <returns></returns>
        public static DataTable pToDataTable<T>(this IEnumerable<T> obj, string dtName = "")
        {
            DataTable dt = new DataTable() { TableName = dtName };
            PropertyInfo[] props = null;
            foreach (T item in obj)
            {
                if (props == null) //尚未初始化
                {
                    Type t = item.GetType();
                    props = t.GetProperties();
                    foreach (PropertyInfo pi in props)
                    {
                        Type colType = pi.PropertyType;
                        //針對Nullable<>特別處理
                        if (colType.IsGenericType
                            && colType.GetGenericTypeDefinition() == typeof(Nullable<>))
                            colType = colType.GetGenericArguments()[0];
                        //建立欄位
                        dt.Columns.Add(pi.Name, colType);
                    }
                }
                DataRow row = dt.NewRow();
                foreach (PropertyInfo pi in props)
                    row[pi.Name] = pi.GetValue(item, null) ?? DBNull.Value;
                dt.Rows.Add(row);
            }
            return dt;
        }

    }
    #endregion

    #region DataTable
    public static class DataTableUtil
    {
        // https://stackoverflow.com/questions/1104121/how-to-convert-a-datatable-to-a-string-in-c

        //public static string pToString(this DataTable dt)
        //{
        //    return string.Join(Environment.NewLine, dt.Rows.OfType<DataRow>().Select(x => string.Join(" ; ", x.ItemArray)));
        //}

        public static string pToString(this DataTable dt)
        {
            if (dt == null)
                return "DataTable NULL";

            var output = new StringBuilder();

            var columnsWidths = new int[dt.Columns.Count];

            // Get column widths
            foreach (DataRow row in dt.Rows)
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    var length = row[i].ToString().Length;
                    if (columnsWidths[i] < length)
                        columnsWidths[i] = length;
                }
            }

            // Get Column Titles
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                var length = dt.Columns[i].ColumnName.Length;
                if (columnsWidths[i] < length)
                    columnsWidths[i] = length;
            }

            // Write Column titles
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                var text = dt.Columns[i].ColumnName;
                output.Append("|" + PadCenter(text, columnsWidths[i] + 2));
            }
            output.Append("|\n" + new string('=', output.Length) + "\n");

            // Write Rows
            foreach (DataRow row in dt.Rows)
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    var text = row[i].ToString();
                    output.Append("|" + PadCenter(text, columnsWidths[i] + 2));
                }
                output.Append("|\n");
            }
            return output.ToString();
        }

        private static string PadCenter(string text, int maxLength)
        {
            int diff = maxLength - text.Length;
            return new string(' ', diff / 2) + text + new string(' ', (int)(diff / 2.0 + 0.5));
        }

        public static bool pEmpty(this DataTable dt)
        {
            return !pAny(dt);
        }

        public static bool pAny(this DataTable dt)
        {
            bool hasAnyRow = (dt != null && dt.Rows.Count > 0);
            return hasAnyRow;
        }


        public static DataTable pOrderByToDataTable(this DataTable dt, string columnName, SortOrder sort = SortOrder.Ascending)
        {

            if (dt.pEmpty())
            {
                return dt;
            }

            // 用 DataView 排序：依欄位型別比較(數字欄位就是數字排序)，舊版 LINQ 是全部轉字串再排
            DataView dv = new DataView(dt);
            if (sort == SortOrder.Ascending)
            {
                dv.Sort = "[" + columnName + "] ASC";
            }
            else if (sort == SortOrder.Descending)
            {
                dv.Sort = "[" + columnName + "] DESC";
            }
            else
            {
                return dt.Clone();
            }

            return dv.ToTable();
        }


        public static int pCount(this DataTable dt)
        {
            if (dt == null)
                return 0;

            return dt.Rows.Count;
        }

        public static string pRowCol(this DataTable dt, string ColumnName, int RowIndex = 0)
        {
            // RowIndex大於RowCount時讓它Crash
            string s = dt.Rows[RowIndex].pCol(ColumnName);
            return s;
        }

        // Trim所有String資料
        public static DataTable pTrim(this DataTable dt)
        {
            foreach (DataRow r in dt.Rows)
            {
                foreach (DataColumn c in dt.Columns)
                {
                    if (c.DataType == typeof(String))
                    {
                        bool rw = c.ReadOnly;
                        c.ReadOnly = false;
                        r[c.ColumnName] = r.pCol(c.ColumnName);
                        c.ReadOnly = rw;
                    }
                }
            }

            return dt;
        }

        public static int pMax(this DataTable dt, string column)
        {
            int max = int.MinValue;

            foreach (DataRow r in dt.Rows)
            {
                int val = r.pGetColumnInt(column);
                max = Math.Max(max, val);
            }

            return max;
        }

        public static int pMin(this DataTable dt, string column)
        {
            int min = int.MaxValue;

            foreach (DataRow r in dt.Rows)
            {
                int val = r.pGetColumnInt(column);
                min = Math.Min(min, val);
            }

            return min;
        }

        // 取代DataTable Where(), 查詢為空值時, 回傳DataTable clone
        public static DataTable pWhere(this DataTable dt, Func<DataRow, bool> predicate)
        {
            if (dt == null)
            {
                return null;
            }
            var q = dt.AsEnumerable().Where(predicate);
            var r = q.Any() ? q.CopyToDataTable() : dt.Clone();

            return r;
        }

        public static string ConvertObjectToString(object obj)
        {
            return obj.ToString() ?? string.Empty;
        }

        public static DataTable pAddCols(this DataTable dt, params object[] args)
        {
            string[] cols = Array.ConvertAll<object, string>(args, ConvertObjectToString);
            foreach (string col in cols)
            {
                dt.pAddCol(col, Length: 100, ReadOnly: false);
            }

            return dt;
        }


        public static DataTable pAddCol(this DataTable dt, string ColName, int Length = 100, bool ReadOnly = false)
        {
            if (dt.Columns.Contains(ColName) == false)
            {
                dt.Columns.Add(ColName);
                dt.Columns[ColName].ReadOnly = ReadOnly;
                dt.Columns[ColName].MaxLength = Length;
            }

            return dt;
        }

        public static DataTable pSetColWritable(this DataTable dt, string ColName, int Length = 100)
        {
            if (dt.Columns.Contains(ColName))
            {
                dt.Columns[ColName].ReadOnly = false;
                dt.Columns[ColName].MaxLength = Length;
            }

            return dt;
        }

        public static DataTable pAddColInt(this DataTable dt, string ColName, bool ReadOnly = false)
        {
            dt.Columns.Add(ColName, typeof(int));
            dt.Columns[ColName].ReadOnly = ReadOnly;

            return dt;
        }

        public static DataTable pAddColDouble(this DataTable dt, string ColName, bool ReadOnly = false)
        {
            dt.Columns.Add(ColName, typeof(double));
            dt.Columns[ColName].ReadOnly = ReadOnly;

            return dt;
        }

        public static string pSQLInsert(this DataTable dt, string dbName = "DB_OPD")
        {
            string insertSQL = "";
            if (dt.pAny())
            {
                DataTableTool dtt = new DataTableTool();
                insertSQL = dtt.DataTableToSingleInsertSQL(dt, dbName);
            }
            return insertSQL;
        }

        public static string pSQLInsert(this DataTable dt)
        {
            string insertSQL = dt.pSQLInsert("DB_OPD");

            return insertSQL;
        }
        public static string pMIDDLESQLInsert(this DataTable dt)
        {
            string insertSQL = "";
            if (dt.pAny())
            {
                DataTableTool dtt = new DataTableTool();
                insertSQL = dtt.DataTableToSingleInsertMIDDLESQL(dt);
            }
            return insertSQL;
        }


        public static string[] pColumnToArray(this DataTable dt, string ColName)
        {
            string[] strArray = { };
            if (dt.pEmpty())
            {
                return strArray;
            }


            strArray = new string[dt.Rows.Count];
            for (int i = 0; i < dt.Rows.Count; i++)
                strArray[i] = dt.Rows[i].pGetColumn(ColName);

            return strArray;
        }

        // 欄位加總(int 4byte)
        public static int pSumInt(this DataTable dt, string ColName)
        {
            int total = 0;
            foreach (DataRow row in dt.Rows)
                total = checked(total + row.pGetColumnInt(ColName));

            return total;
        }

        // 欄位加總(long 8byte)
        public static long pSumLong(this DataTable dt, string ColName)
        {
            long total = 0;
            foreach (DataRow row in dt.Rows)
                total = checked(total + row.pGetColumnLong(ColName));

            return total;
        }

        // 欄位加總(double)
        public static double pSumDouble(this DataTable dt, string ColName)
        {
            double total = 0;
            foreach (DataRow row in dt.Rows)
                total += row.pGetColumnDouble(ColName);

            return total;
        }

    }
    #endregion

    #region DataSet
    public static class DataSetUtil
    {
        public static bool pAny(this DataSet ds)
        {
            bool hasAnyTables = (ds != null && ds.Tables.Count > 0);
            return hasAnyTables;
        }


        public static string pToString(this DataSet ds)
        {
            var output = new StringBuilder();

            for (int i = 0; i < ds.Tables.Count; i++)
            {
                output.Append($"Table:{ds.Tables[i].TableName}\n");
                output.AppendLine(ds.Tables[i].pToString());
            }

            return output.ToString();
        }

        public static DataSet pAdd(this DataSet ds, DataTable dt, string tableName = null)
        {

            if (ds.Tables.Contains(dt.TableName))
            {
                ds.Tables.Remove(dt.TableName);
            }

            ds.Tables.Add(dt);
            return ds;
        }
    }

    #endregion

    #region DateTime
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
    #endregion

    #region List
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
    #endregion

    #region Boolean
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

    #endregion

}

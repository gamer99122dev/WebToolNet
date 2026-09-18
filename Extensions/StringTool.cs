using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using WebToolNet.Data;
using WebToolNet.Dates;
using WebToolNet.Validation;

namespace WebToolNet.Extensions
{
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

            return stringCut.SubStringByte(sSource, start, end);
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
                d = dc.TWDToDateTime(source, WellFormCheck);
            }
            else if (source.Length == 11)
            {
                d = dc.TWDHMSToDateTime(source + "00", WellFormCheck);
            }
            else if (source.Length == 13)
            {
                d = dc.TWDHMSToDateTime(source, WellFormCheck);
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
            // 11104012為錯誤民國日期, 但過得了IsTWDate()的檢核
            if (expectLength > 0 && src.Length != expectLength)
            {
                return false;
            }

            CheckDate _dateChecker = new CheckDate();
            bool isValid = _dateChecker.IsTWDate(src);
            return isValid;
        }

        public static bool pIsTime4(this string src, int expectLength = 0)
        {
            CheckDate _dateChecker = new CheckDate();
            bool isValid = _dateChecker.IsTime4(src);
            return isValid;
        }



        // 是否西元日期
        public static bool pIsDate(this string src)
        {
            CheckDate _dateChecker = new CheckDate();
            bool isValid = _dateChecker.IsDate(src);
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

            return check.CheckTWID(src) || check.CheckForeignID(src);
        }

        public static bool pCheckPID(this string src, bool IsTW = true)
        {
            CheckID check = new CheckID();

            if (IsTW == true)
            {
                return check.CheckTWID(src);
            }
            else
            {
                return check.CheckForeignID(src);
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
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WebToolNet.Extensions
{
    public class StringCut
    {
        // .NET 10 內建編碼只剩 UTF/ASCII/Latin1，Big5(950) 要先註冊 CodePages 提供者，
        // 否則 Encoding.GetEncoding(950) 會丟 NotSupportedException。
        // 註冊一次全 App 都能用 Big5(讀健保檔、Big5 CSV 也吃得到)
#pragma warning disable CA2255 // 類別庫用 ModuleInitializer 是刻意的，讓呼叫端不用記得註冊
        [ModuleInitializer]
        internal static void RegisterCodePages()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
#pragma warning restore CA2255

        // .NET Framework 的 Encoding.Default 在 zh-TW 是 Big5，.NET 10 是 UTF-8(中文會從2變3 bytes)，
        // 所以要算 byte 長度的地方一律用這個
        public static Encoding Big5
        {
            get { return Encoding.GetEncoding(950); }
        }

        public StringCut()
        {
        }

        public string Left(string sSource, int iLength)
        {
            return sSource.Substring(0, iLength > sSource.Length ? sSource.Length : iLength);
        }

        public string Right(string sSource, int iLength)
        {
            return sSource.Substring(iLength > sSource.Length ? 0 : sSource.Length - iLength);
        }

        public string Mid(string sSource, int iStart, int iLength)
        {
            int iStartPoint = iStart > sSource.Length ? sSource.Length : iStart;
            return sSource.Substring(iStartPoint, iStartPoint + iLength > sSource.Length ? sSource.Length - iStartPoint : iLength);
        }


        /// <summary>
        /// 截字串(依Byte) 
        /// </summary>
        /// <param name="InputSrt">傳入字串</param>
        /// <param name="StartIndex">字串開始位置</param>
        /// <param name="EndIndex">字串結束位置</param>
        /// <returns></returns>
        public string SubStrginByte(string InputSrt, int StartIndex, int EndIndex)
        {
            Encoding econd = Encoding.GetEncoding("Big5", new EncoderExceptionFallback(), new DecoderReplacementFallback(""));
            byte[] bytes = econd.GetBytes(InputSrt);

            if (EndIndex <= 0)
            {
                return string.Empty;
            }


            if ((StartIndex + 1) > bytes.Length)
            {
                return string.Empty;
            }
            else
            {

                if ((StartIndex + EndIndex) > bytes.Length)
                {
                    EndIndex = bytes.Length - StartIndex;
                }
            }

            return econd.GetString(bytes, StartIndex, EndIndex);
        }
    }
}

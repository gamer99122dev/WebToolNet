using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace WebToolNet.UtilExtension
{

    namespace myDateTime
    {
        /* 架構與規劃：
         * 
         * 一、設計一組 getter 可取得 class 內的 Date Time 字串，共有底下幾種組合
         *
         *     時分秒.毫秒4位
         *     時分秒
         *     時:分:秒.毫秒4位
         *     時:分:秒

         *     民國年月日
         *     民國年/月/日
         *     民國年-月-日

         *     西元年月日
         *     西元年/月/日
         *     西元年-月-日

         * 二、class 可由 constructor 決定是否給定 DateTime 物件進來
         * 
         * 三、有一組 getter、setter 可以自己進出 DateTime
         * 
         * 四、本物件只提供 DateTime 物件轉換成字串，日期的加減請在外面做好再傳進來。
         *
         */
        public class GenerateDateString
        {
            private DateTime _dt;

            public DateTime DT
            {
                get { return _dt; }
                set { _dt = value; }
            }

            public GenerateDateString SetDT(DateTime dt)
            {
                DT = dt;
                return this;
            }


            public GenerateDateString()
            {
                this._dt = DateTime.Now;
            }

            public GenerateDateString(DateTime dt)
            {
                this._dt = dt;
            }


            #region 時間


            /// <summary>
            /// 時分秒.毫秒4位
            /// </summary>
            public string hhmmssffff
            {
                get
                {
                    return _dt.ToString("HHmmss.ffff");
                }
            }


            /// <summary>
            /// 時分秒
            /// </summary>
            public string hhmmss
            {
                get
                {
                    return _dt.ToString("HHmmss");
                }
            }

            /// <summary>
            /// 時分
            /// </summary>
            public string hhmm
            {
                get
                {
                    return _dt.ToString("HHmm");
                }
            }


            /// <summary>
            /// 時:分:秒.毫秒4位
            /// </summary>
            public string Chhmmssffff
            {
                get
                {
                    return _dt.ToString("HH:mm:ss.ffff");
                }
            }


            /// <summary>
            /// 時:分:秒
            /// </summary>
            public string Chhmmss
            {
                get
                {
                    return _dt.ToString("HH:mm:ss");
                }
            }

            /// <summary>
            /// 時:分
            /// </summary>
            public string Chhmm
            {
                get
                {
                    return _dt.ToString("HH:mm");
                }
            }


            #endregion


            #region 西元年

            /// <summary>
            /// 西元年月日
            /// </summary>
            public string yyyymmdd
            {
                get
                {
                    return _dt.ToString("yyyyMMdd");
                }
            }


            public string yyyymm
            {
                get
                {
                    return _dt.ToString("yyyyMM");
                }
            }

            public string yyyy
            {
                get
                {
                    return _dt.ToString("yyyy");
                }
            }

            /// <summary>
            /// 西元年/月/日
            /// </summary>
            public string Syyyymmdd
            {
                get
                {
                    return _dt.ToString("yyyy/MM/dd");
                }
            }

            /// <summary>
            /// 西元年/月/日 時:分:秒
            /// </summary>
            public string Syyyymmddhhmmss
            {
                get
                {
                    return _dt.ToString("yyyy/MM/dd HH:mm:ss");
                }
            }

            /// <summary>
            /// 西元月/日/年
            /// </summary>
            public string Smmddyyyy
            {
                get
                {
                    return _dt.ToString("MM/dd/yyyy");
                }
            }


            /// <summary>
            /// 西元年-月-日
            /// </summary>
            public string Dyyyymmdd
            {
                get
                {
                    return _dt.ToString("yyyy-MM-dd");
                }
            }

            #endregion


            #region 民國年

            /// <summary>
            /// 「民國」年月日
            /// </summary>
            /// 
            public string Ryyymmdd
            {
                get
                {
                    DateTime dt = _dt;
                    string sYear = "000" + Convert.ToString(dt.AddYears(-1911).Year);
                    return sYear.Substring(sYear.Length - 3, 3) + dt.ToString("MMdd");
                }
            }

            public string Ryyymm
            {
                get
                {
                    DateTime dt = _dt;
                    string sYear = "000" + Convert.ToString(dt.AddYears(-1911).Year);
                    return sYear.Substring(sYear.Length - 3, 3) + dt.ToString("MMdd").Substring(0,2);
                }
            }

            /// <summary>
            /// 「民國」年月日時分
            /// </summary>
            public string Ryyymmddhhmm
            {
                get
                {
                    return Ryyymmdd + hhmm;
                }
            }

            /// <summary>
            /// 「民國」年月日時分秒
            /// </summary>
            public string Ryyymmddhhmmss
            {
                get
                {
                    return Ryyymmdd + hhmmss;
                }
            }

            public string Ryyy
            {
                get
                {
                    DateTime dt = _dt;
                    string sYear = "000" + Convert.ToString(dt.AddYears(-1911).Year);
                    return sYear.Substring(sYear.Length - 3, 3);
                }
            }


            /// <summary>
            /// 「民國」年/月/日
            /// </summary>
            public string SRyyymmdd
            {
                get
                {
                    DateTime dt = _dt;
                    string sYear = "000" + Convert.ToString(dt.AddYears(-1911).Year);
                    return sYear.Substring(sYear.Length - 3, 3) + dt.ToString("/MM/dd");
                }
            }

            /// <summary>
            /// 分:秒
            /// </summary>
            public string SRhhmm
            {
                get
                {
                    return _dt.ToString("HH:mm");
                }
            }

            /// <summary>
            /// 「民國」年/月/日 時:分
            /// </summary>
            public string SRyyymmddhhmm
            {
                get
                {
                    return SRyyymmdd + " " + SRhhmm;
                }
            }


            /// <summary>
            /// 「民國」年-月-日
            /// </summary>
            public string DRyyymmdd
            {
                get
                {
                    DateTime dt = _dt;
                    string sYear = "000" + Convert.ToString(dt.AddYears(-1911).Year);
                    return sYear.Substring(sYear.Length - 3, 3) + dt.ToString("-MM-dd");
                }
            }

            #endregion

        }


    }

}
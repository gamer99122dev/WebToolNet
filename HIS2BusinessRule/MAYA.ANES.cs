using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace WebToolNet.HIS2BusinessRule
{
    public static class ANESCaller
    {
        // 每次 new HttpClient 會把連線埠耗光，共用一個就好
        private static readonly HttpClient _http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

        /// <summary>
        /// 跟麻醉系統換取該病人指定區間的麻醉記錄檢視網址。
        /// WinForm 版是拿到網址自己開 Chrome，Web 版只回網址，要導去哪由呼叫端決定
        /// </summary>
        public static async Task<string> GetUrl(string chartNo, string userId, string queryStartDate, string queryEndDate)
        {
            // 組 URL（必要參數做 URL Encode，避免斜線/空白等造成問題）
            string url = "https://anes.hosp.nycu.edu.tw/YMUH_API/api/HIS_DataQuery/GetAnesBinderReviewUrlForInterval?AccessToken=kDM1AzMyAjM8lEUB9FSV1UW%3D"
                       + "&ChartNo=" + Uri.EscapeDataString(chartNo ?? "")
                       + "&QueryStartDate=" + Uri.EscapeDataString(queryStartDate ?? "")
                       + "&QueryEndDate=" + Uri.EscapeDataString(queryEndDate ?? "")
                       + "&UserID=" + Uri.EscapeDataString(userId ?? "");

            string result = await _http.GetStringAsync(url);

            // 回的是 JSON 字串，前後包著雙引號，去掉才是真的網址
            return result.Trim().Trim('"');
        }
    }
}

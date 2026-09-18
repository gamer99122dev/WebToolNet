using System.Net;
using Microsoft.AspNetCore.Http;

namespace WebToolNet.Extensions
{
    public static class WebHelper
    {
        public static string JsEncode(string s)
        {
            if (s == null)
                return "";

            return s.Replace("\\", "\\\\").Replace("'", "\\'").Replace("\r", "\\r").Replace("\n", "\\n");
        }

        // .NET 10 沒有 HttpContext.Current，改成擴充方法。
        // Controller 內用法: HttpContext.GetClientIP()
        public static string GetClientIP(this HttpContext ctx)
        {
            string forwarded = ctx.Request.Headers["X-Forwarded-For"].ToString();
            if (!string.IsNullOrWhiteSpace(forwarded))
                return forwarded.Split(',')[0].Trim();

            return ctx.Connection.RemoteIpAddress?.ToString() ?? "";
        }

        public static string GetClientIPv4(this HttpContext ctx)
        {
            // 舊版是拿IP去做DNS查詢挑IPv4，Kestrel 直接給得到位址，不需要查DNS
            IPAddress ip = ctx.Connection.RemoteIpAddress;
            if (ip == null)
                return string.Empty;

            return ip.IsIPv4MappedToIPv6 ? ip.MapToIPv4().ToString() : ip.ToString();
        }

        public static string GetChAmPm(string strTime)
        {
            int t = 0;
            int.TryParse(strTime, out t);
            if (t >= 600 && t <= 1200) return "上午";
            if (t > 1200 && t <= 2400) return "下午";
            return "";
        }
    }
}

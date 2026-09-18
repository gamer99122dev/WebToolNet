using Microsoft.AspNetCore.Builder;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WebToolNet.Data
{
    /// <summary>
    /// 網站啟動時的 DB 接線。每個站的 Program.cs 這段都一樣，所以收在這裡，不要各站複製一份。
    /// 用法見 WebToolNet/README.md，Program.cs 只要三行：HandleEncryptDb → AddDBConn → LogDbTarget。
    ///
    /// 連哪個 DB 由 Database__Target 決定（Test / Production），沒設就啟動失敗。
    /// 連線字串從哪裡來，看跑在哪：
    ///   本機（Development）        → appsettings.Development.json 的 ConnectionStrings:{Target}，Windows 驗證、沒密碼
    ///   IIS 正式站（非 Development）→ DPAPI 密文檔（DbSecret），SQL 帳號
    /// 舊版 .NET Framework 讀 C:\HIS2\DBConfig.xml，檔案不見就靜靜用正式連線，方向剛好相反；這裡沒有任何 fallback。
    /// </summary>
    public static class DbStartup
    {
        /// <summary>
        /// 部署時用：&lt;網站&gt;.exe --encrypt-db [--force]。放在 Program.cs 第一行，回 true 就直接 return，
        /// 不建 web host、不連 DB。哪個站的 exe 跑都寫同一個檔（DbSecret.DefaultPath）。
        /// </summary>
        public static bool HandleEncryptDb(string[] Args)
        {
            if (!Args.Contains("--encrypt-db")) return false;

            DbSecret.EncryptInteractive(DbSecret.DefaultPath, Args.Contains("--force"));
            return true;
        }

        /// <summary>依 Database:Target 決定連線字串，把 DBConn 註冊成 Scoped（每個 request 一個）。設錯就在這裡丟例外，啟動失敗。</summary>
        public static void AddDBConn(this WebApplicationBuilder Builder)
        {
            string connectString = ResolveConnectionString(Builder.Configuration, Builder.Environment);

            DBConn CreateDbConn(IServiceProvider services)
            {
                return new DBConn { connectString = connectString };
            }
            Builder.Services.AddScoped(CreateDbConn);
        }

        /// <summary>部署後確認連對機器用。只記 Target / Server / Database，不含帳密。</summary>
        public static void LogDbTarget(this WebApplication App)
        {
            using (IServiceScope scope = App.Services.CreateScope())
            {
                DBConn db = scope.ServiceProvider.GetRequiredService<DBConn>();
                SqlConnectionStringBuilder csb = new SqlConnectionStringBuilder(db.connectString);
                App.Logger.LogInformation("DB Target={Target} Server={Server} Catalog={Catalog}",
                    App.Configuration["Database:Target"], csb.DataSource, csb.InitialCatalog);
            }
        }

        private static string ResolveConnectionString(IConfiguration Config, IHostEnvironment Env)
        {
            string target = Config["Database:Target"];

            if (target == "Test" || (target == "Production" && Env.IsDevelopment()))
            {
                string fromConfig = Config.GetConnectionString(target);
                if (fromConfig == null)
                {
                    throw new InvalidOperationException($"Database__Target={target}，但找不到 ConnectionStrings:{target}");
                }
                return fromConfig;
            }

            if (target == "Production")
            {
                // 只在啟動時解一次
                return DbSecret.Load(Config["Database:SecretFile"] ?? DbSecret.DefaultPath);
            }

            throw new InvalidOperationException(
                $"Database__Target 未設定或無法識別（目前值：'{target}'）。"
              + "本機請在 Visual Studio 選「測試DB」啟動設定；"
              + "正式站請在 IIS 應用程式集區設 Database__Target=Production。");
        }
    }
}

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.SqlClient;

namespace WebToolNet.Data
{
    /// <summary>
    /// 正式站連線字串的加密存放。用 Windows DPAPI，密文檔放在網站目錄外。
    ///
    /// 為什麼是 LocalMachine 不是 CurrentUser：
    /// 兩者擋得住的攻擊者其實一樣（能以網站帳號跑程式碼的人都解得開），DPAPI 真正多出來的
    /// 防護是「檔案被複製出這台機器就是廢的」，LocalMachine 一樣有。但 CurrentUser 要用
    /// PsExec 以 IIS AppPool 身分跑加密、要 Load User Profile、App Pool 一重建（新 SID）
    /// 就得重新加密。LocalMachine 全部免了。
    ///
    /// 代價：LocalMachine 表示這台機器上「讀得到檔案的帳號」就解得開，所以檔案 ACL 是主要
    /// 防線 —— 部署時的 icacls /inheritance:r 不可略過，見 README。
    /// </summary>
    public static class DbSecret
    {
        /// <summary>
        /// 正式站密文檔的定案路徑。一台機器一個檔、所有網站共用（公司 DB 帳密都一樣），
        /// 各站集區各自給唯讀權限即可。放在網站目錄外，publish 覆蓋不到它。
        /// 單站要換位置用環境變數 Database__SecretFile 覆蓋，不要改這裡。
        /// </summary>
        public const string DefaultPath = @"C:\WebConfig\db.dat";

        // DPAPI 是 Windows 專屬 API。這個專案本來就只跑在 IIS 上，把平台限制收斂在這個檔案，
        // 呼叫端就不用跟著標 [SupportedOSPlatform]
#pragma warning disable CA1416

        /// <summary>讀取並解密密文檔。任何一步不對就丟例外，不 fallback 到別的來源。</summary>
        public static string Load(string FilePath)
        {
            RequireWindows();

            if (!File.Exists(FilePath))
            {
                throw new InvalidOperationException(
                    $"找不到 DB 密文檔：{FilePath}。請在這台主機執行一次任一站的 <網站>.exe --encrypt-db（哪個站的 exe 都寫同一個檔）");
            }

            byte[] cipher;
            try
            {
                cipher = Convert.FromBase64String(File.ReadAllText(FilePath).Trim());
            }
            catch (FormatException)
            {
                // 例外訊息一律不帶檔案內容，免得密文被寫進 Log
                throw new InvalidOperationException($"DB 密文檔格式不正確：{FilePath}");
            }

            byte[] plain;
            try
            {
                plain = ProtectedData.Unprotect(cipher, null, DataProtectionScope.LocalMachine);
            }
            catch (CryptographicException)
            {
                // 從別台主機複製過來、或檔案被改過都會走到這裡
                throw new InvalidOperationException(
                    $"DB 密文檔解密失敗（是不是從別台主機複製過來的？）：{FilePath}");
            }

            string connectString;
            try
            {
                connectString = Encoding.UTF8.GetString(plain);
            }
            finally
            {
                Array.Clear(plain);   // string 清不掉，至少 byte[] 不留在記憶體
            }

            // 解得開不代表內容是對的。語法壞掉要在啟動時就炸，不要拖到第一次查詢才發現
            SqlConnectionStringBuilder check = new SqlConnectionStringBuilder(connectString);
            if (string.IsNullOrWhiteSpace(check.DataSource))
            {
                throw new InvalidOperationException($"DB 密文檔的內容不是有效的連線字串：{FilePath}");
            }

            return connectString;
        }

        /// <summary>
        /// 部署時用。輸入連線字串（人打就不回顯；Deploy/Deploy-IisSite.ps1 用管線餵進來就直接讀 stdin），
        /// 先真的連一次 DB，再加密寫檔，並立刻讀回來驗證。
        /// </summary>
        public static void EncryptInteractive(string FilePath, bool Force)
        {
            RequireWindows();

            if (File.Exists(FilePath) && !Force)
            {
                Console.WriteLine($"密文檔已存在：{FilePath}");
                Console.WriteLine("確定要覆蓋請加 --force");
                return;
            }

            Console.WriteLine($"密文檔位置：{FilePath}");

            string connectString;
            if (Console.IsInputRedirected)
            {
                // 固定當 UTF-8 讀，PowerShell 那邊也固定送 UTF-8；pwsh 7 會多送一個 BOM，要剝掉
                using (StreamReader stdin = new StreamReader(Console.OpenStandardInput(), Encoding.UTF8, true))
                {
                    connectString = (stdin.ReadLine() ?? "").Trim().TrimStart('﻿');
                }
            }
            else
            {
                Console.WriteLine("請貼上正式站連線字串（畫面不會顯示），按 Enter 結束：");
                connectString = ReadHidden();
            }
            if (string.IsNullOrWhiteSpace(connectString))
            {
                Console.WriteLine("沒有輸入，取消。");
                return;
            }

            // 先驗語法，免得加密完才發現字串本身是壞的
            SqlConnectionStringBuilder check;
            try
            {
                check = new SqlConnectionStringBuilder(connectString);
            }
            catch (ArgumentException ex)
            {
                throw new InvalidOperationException($"連線字串格式不對：{ex.Message}");
            }
            if (string.IsNullOrWhiteSpace(check.DataSource) || string.IsNullOrWhiteSpace(check.InitialCatalog))
            {
                throw new InvalidOperationException("連線字串缺少 Server 或 Database");
            }

            // 再真的連一次。帳密打錯在這裡就知道，不要等網站 500.30 才從 log 裡找
            try
            {
                using (SqlConnection conn = new SqlConnection(connectString)) conn.Open();
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException($"連不上 DB（Server={check.DataSource} Database={check.InitialCatalog}）：{ex.Message}");
            }

            byte[] plain = Encoding.UTF8.GetBytes(connectString);
            try
            {
                byte[] cipher = ProtectedData.Protect(plain, null, DataProtectionScope.LocalMachine);

                string dir = Path.GetDirectoryName(FilePath);
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

                File.WriteAllText(FilePath, Convert.ToBase64String(cipher));
            }
            finally
            {
                Array.Clear(plain);
            }

            // 立刻讀回來比對。這是這段邏輯唯一需要的檢查，而且跑在真正重要的時機：部署當下
            if (Load(FilePath) != connectString)
            {
                File.Delete(FilePath);
                throw new InvalidOperationException("寫入後讀回的內容不一致，已刪除密文檔");
            }

            Console.WriteLine();
            Console.WriteLine($"完成。Server={check.DataSource}  Database={check.InitialCatalog}");
            Console.WriteLine();
            Console.WriteLine("接下來一定要收緊檔案權限，否則這台機器上任何帳號都解得開：");
            Console.WriteLine($"  icacls \"{FilePath}\" /inheritance:r /grant \"SYSTEM:(F)\" \"Administrators:(F)\" \"IIS AppPool\\<應用程式集區名稱>:(R)\"");
        }

        /// <summary>從 Console 讀一行但不回顯，避免肩窺、也避免密碼留在終端機捲軸裡</summary>
        private static string ReadHidden()
        {
            StringBuilder sb = new StringBuilder();

            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(intercept: true);

                if (key.Key == ConsoleKey.Enter) break;

                if (key.Key == ConsoleKey.Backspace)
                {
                    if (sb.Length > 0) sb.Length--;
                    continue;
                }

                // 方向鍵、F1 這類特殊鍵的 KeyChar 是 '\0'，不能收進字串裡
                if (key.KeyChar == '\0') continue;

                sb.Append(key.KeyChar);
            }

            Console.WriteLine();
            return sb.ToString();
        }

        private static void RequireWindows()
        {
            if (!OperatingSystem.IsWindows())
            {
                throw new PlatformNotSupportedException("DPAPI 只有 Windows 有，密文檔無法在此平台加解密");
            }
        }

#pragma warning restore CA1416
    }
}

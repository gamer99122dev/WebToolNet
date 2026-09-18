# .NET 10 ASP.NET Core MVC／MSSQL 資料庫連線與環境切換規格書

版本：3.1

本文件取代 v2（DPAPI 修訂版）。供之後所有 .NET 10 專案沿用、同事交接及 Windows／IIS 部署使用。所有帳號、路徑與主機名稱範例均為示意，不含真正帳密。

操作步驟（引用、切換、部署）見 `WebToolNet/README.md`，新站範本在 `WebToolNet/Templates/`；本文件只講規則與理由。第一個實作範例：`WebDaySurgery`。

## 1. 速查

| 要做的事 | 操作位置 |
| --- | --- |
| 本機開發 | Clone、開 Solution、F5。不用設定任何東西 |
| 本機切換 Test／Production DB | Visual Studio 啟動設定下拉選單 |
| 設定正式站連線字串 | 在正式主機執行任一站的 `<網站>.exe --encrypt-db`，每台主機一次，所有網站共用同一個檔 |
| 設定正式站用哪個 DB | IIS 應用程式集區環境變數 `Database__Target` |
| 建立 SQL 連線 | `WebToolNet.Data.DBConn`（DI 注入） |
| 新站接上 DB | `Program.cs` 呼叫 `WebToolNet.Data.DbStartup` 三行，見 `WebToolNet/README.md` |
| 撰寫業務 SQL | Web 專案的 Services 或系統對應類別 |
| 更換正式 DB 密碼 | 重跑 `--encrypt-db --force`、重設 ACL、回收該主機所有用到它的應用程式集區 |

## 2. v3 相對 v2 的變更與理由

v2 是在「正式站可能要用 SQL 帳密、且可能有稽核要求」的假設下寫的，實際條件確認後有幾項可以大幅簡化：

| v2 | v3 | 為什麼 |
| --- | --- | --- |
| 本機用 User Secrets | 本機用 Windows 驗證，設定進版控 | 開發者 AD 帳號本來就連得上測試 DB。沒有密碼就沒有東西要保管，同事 clone 完直接 F5 |
| `Database:Target` + `Database:SecretSource` 兩個開關 | 只有 `Database:Target` | Target 已經隱含來源（Test→設定檔、Production→密文檔），第二個開關只是多一個可以設錯的地方 |
| DPAPI `CurrentUser` | DPAPI `LocalMachine` | 見第 5 節。兩者擋得住的攻擊者一樣，但 CurrentUser 的維運成本高很多 |
| 獨立的 `DbSecretTool` Console 專案 | 加密模式內建成網站 exe 的 `--encrypt-db` 參數 | 少一個專案、少一份要同步的程式碼。部署時要跑的東西就在發布目錄裡 |
| 密文檔用 JSON 外殼（formatVersion／scope／applicationId／target） | 檔案就是一段 Base64 | 那些欄位防的是「拿錯密文檔」，但密文檔全機只有一個、所有網站共用（第 6 節），沒有拿錯的問題。內容正確性改用 `SqlConnectionStringBuilder` 一行驗證 |
| 新增 `DbConnectionFactory` 類別 | 用既有的 `WebToolNet.Data.DBConn` | 專案已經有這個類別，`CLAUDE.md` 也規定資料存取走它。多一層只是換個名字做一樣的事 |
| TFM 改 `net10.0-windows` | 維持 `net10.0` | DPAPI 的平台限制用 `DbSecret.cs` 單檔 `#pragma warning disable CA1416` + `OperatingSystem.IsWindows()` 收斂，不必讓整個共用 Library 綁死平台 |

v2 保留下來的（都是對的）：不用 EF Core／Dapper／自製加密演算法、秘密不進 git 不進 Log、錯誤即停止不 fallback、ACL 要求、Log 禁止項目、驗收清單，以及「通過 DPAPI 加密」不等於「保證通過所有資安稽核」。

### v3.1（2026-09）

| v3.0 | v3.1 | 為什麼 |
| --- | --- | --- |
| 各站 `Program.cs` 各寫一份 DB 接線（讀 Target、判斷環境、註冊 `DBConn`、啟動 Log） | 收進 `WebToolNet.Data.DbStartup`，`Program.cs` 剩三行 | 第二個站起就是整段複製貼上，邏輯一改要每站各改。共用 library 的目的就是這個 |
| 密文檔路徑寫在各站 `Program.cs` | `DbSecret.DefaultPath` 常數 | 各站不可能寫歪；換路徑仍用 `Database:SecretFile` |

## 3. 技術範圍

- .NET 10、ASP.NET Core MVC、Microsoft SQL Server。
- 資料存取用原生 ADO.NET 與 `Microsoft.Data.SqlClient`。
- 不用 EF Core、Dapper、ADO.NET Entity Model 或自製 ORM。
- 不做 GenericRepository、UnitOfWork、多層 Interface、自製 DI 或連線管理器。
- 帳密不寫死於程式碼，不進版控，不輸出至 Log。
- 共用程式碼放 `WebToolNet`，以 Project Reference 引用，不做自有 NuGet Package。`WebToolNet` 是獨立 repo，clone 在網站 repo 的隔壁資料夾（`..\WebToolNet`）。
- 可用官方套件：`Microsoft.Data.SqlClient`、`System.Security.Cryptography.ProtectedData`、`Microsoft.Extensions.*`。固定穩定版本，不用預覽版。
- 正式環境是 Windows／IIS；DPAPI 只有 Windows 有。

## 4. 唯一的開關

| Configuration Key | 允許值 | 設定位置 |
| --- | --- | --- |
| `Database:Target` | `Test` 或 `Production` | 本機：launchSettings 啟動設定；正式站：IIS 應用程式集區環境變數 |
| `Database:SecretFile` | 密文檔絕對路徑（選用） | 只有要放非預設路徑時才設 |
| `ConnectionStrings:Test` | 測試 DB 連線字串（Windows 驗證，無密碼） | `appsettings.Development.json`，可進版控 |
| `ConnectionStrings:Production` | 正式 DB 連線字串（Windows 驗證，無密碼），只給本機除錯用 | `appsettings.Development.json`，可進版控 |

`Database:Target` 同時決定連哪個 DB 和連線字串從哪裡來：

| `Database:Target` | 環境 | 連線字串來源 |
| --- | --- | --- |
| `Test` | — | `ConnectionStrings:Test` |
| `Production` | Development | `ConnectionStrings:Production`（開發者 AD 帳號，不需密文檔） |
| `Production` | 其他 | DPAPI 密文檔（SQL 帳號） |
| 沒設 / 其他值 | — | **啟動失敗** |

**沒有 fallback。** 這是相對舊 .NET Framework 做法最重要的一項修正：舊版讀 `C:\HIS2\DBConfig.xml`，`if (File.Exists(...))` 一旦 false 就靜靜使用正式連線字串，開發機少一個檔就直接連上線上 HIS（fail-open）。v3 猜不到就停（fail-closed）。

禁止 `#if DEBUG` 切換資料庫。啟動後不提供任何 HTTP API 或管理頁面切換 DB，改設定一律要重新啟動網站。

`ASPNETCORE_ENVIRONMENT` 維持框架原本的用途（決定載入哪個 appsettings、要不要顯示詳細錯誤頁），**不拿來推算 `Database:Target`**；它只影響 `Production` 的連線字串來源（Development 讀 appsettings，其他讀 DPAPI）。Library 也不依主機名稱、Debug／Release 自行猜測目標。不讀 `C:\HIS2\DBConfig.xml`。

| 使用情境 | ASPNETCORE_ENVIRONMENT | Database:Target |
| --- | --- | --- |
| 本機測試 | Development | Test |
| 本機正式 DB 除錯 | Development | Production |
| 正式 IIS | Production | Production |

## 5. 為什麼是 LocalMachine 不是 CurrentUser

先講清楚 DPAPI 在這裡實際擋得住什麼：

**擋不住**能以網站執行帳號跑程式碼的人 —— 程式自己就要能讀到密碼，`CurrentUser` 和 `LocalMachine` 在這一點上沒有差別。

**擋得住**的是「檔案被複製出這台機器」：備份、磁碟映像、誤丟到共用資料夾。這是 DPAPI 真正的價值，兩種 scope 都有。

那 `CurrentUser` 多出來的是「同一台機器上的其他帳號解不開」—— 但那件事**檔案 ACL 已經在做了**。而它的成本是：

- 加密工具必須以 `IIS AppPool\<集區名>` 身分執行，虛擬帳號沒辦法直接登入，要靠 PsExec 或排程
- 應用程式集區要 Load User Profile=True，且 profile 要確實載入
- 應用程式集區一重建就是新的 SID，密文全部作廢
- 換主機、還原備份、管理員重設帳號都可能解不開

換來的是一項 ACL 已經涵蓋的防護。不划算，所以 v3 固定用 `LocalMachine`。

**代價要講明白**：`LocalMachine` 表示這台機器上讀得到檔案的帳號就解得開，所以**檔案 ACL 是主要防線，不是輔助**。部署時的 `icacls /inheritance:r` 是必要步驟，不是建議。

## 6. 密文檔

- 位置：全公司固定 `C:\WebConfig\db.dat`（可用 `Database:SecretFile` 覆蓋）。**一台機器一個檔、所有網站共用**：公司 DB 帳密都一樣，`LocalMachine` 密文本來就不綁應用程式，各網站集區各給唯讀即可，不必每站各建一份
- 內容：`ProtectedData.Protect(UTF8(連線字串), null, LocalMachine)` 的結果轉 Base64，沒有其他欄位
- `optionalEntropy` 固定 `null`，不自訂金鑰或演算法
- Base64 只是為了把二進位寫成文字檔，**不是加密**

檔案必須在網站發布目錄、`wwwroot`、版控工作目錄之外，不得由 HTTP 下載，發布不得覆蓋或刪除它。密文、密文備份同樣不得進版控。

DPAPI 只在**啟動時解一次**。不監看檔案、不即時輪替。不得把明碼寫回 appsettings、User Secrets 或環境變數。

### 檔案權限

| 對象 | 權限 |
| --- | --- |
| SYSTEM、Administrators | 完全控制 |
| 用到它的各網站集區帳號 | **唯讀**，不給寫入 |
| 一般使用者 | 不得讀寫 |

```powershell
icacls "C:\WebConfig\db.dat" /inheritance:r `
  /grant "SYSTEM:(F)" "Administrators:(F)" "IIS AppPool\<集區名>:(R)"
```

`/inheritance:r` 不可省 —— 不砍掉繼承的話從 `C:\` 繼承來的 `Users:Read` 會讓本機任何帳號讀得到，而 `LocalMachine` 密文任何本機帳號都解得開。

同一台機器再上一個站，只補 `icacls "C:\WebConfig\db.dat" /grant "IIS AppPool\<新集區名>:(R)"`，不重打 `/inheritance:r`。

替換密文檔時保持同樣嚴格的 ACL，**不得**先產生一個所有人可讀的暫存檔再補權限。密碼輪替由管理人員替換檔案，Web 程式不重寫正式密文。

## 7. 實作分工

| 檔案 | 責任 |
| --- | --- |
| `WebToolNet/Data/DbSecret.cs` | 封裝 DPAPI 與密文檔讀寫 |
| `WebToolNet/Data/DbStartup.cs` | 啟動接線：`--encrypt-db` 分派、依 `Database:Target` 決定連線字串、註冊 `DBConn`、啟動 Log |
| `WebToolNet/Data/DBConn.cs` | 建立 SqlConnection、執行 SQL |
| Web 專案 `Program.cs` | 呼叫 `DbStartup` 三行，其餘服務註冊 |
| Web 專案 `Services/` | 業務 SQL |

`DbSecret` 只有一個常數和兩個 public static 方法：

- **`DefaultPath`** —— `C:\WebConfig\db.dat`，全公司定案的密文檔路徑。各站 `Program.cs` 直接用它，不各自重寫，路徑就不可能寫歪。
- **`Load(path)`** —— 讀檔、Base64 解碼、DPAPI 解密、用 `SqlConnectionStringBuilder` 驗證語法，回傳連線字串。任何一步不對就丟例外，**不 fallback、不改試其他 scope、不改讀其他來源**。例外訊息帶路徑，但絕不帶檔案內容。
- **`EncryptInteractive(path, force)`** —— 互動輸入（`Console.ReadKey(intercept: true)`，**不回顯**）、驗證語法、加密寫檔、**立刻讀回來比對**（不一致就刪檔並丟例外）、只印出 Server 和 Database、提醒要跑 `icacls`。既有檔案要 `--force` 才覆蓋。

不接受含秘密的命令列參數。明碼不落地；用完的 `byte[]` 以 `Array.Clear` 清除（不宣稱 .NET `string` 或整個程序記憶體能可靠清零）。

不得自製 AES／DES 演算法、把金鑰寫死，或提供任何把解密結果輸出到畫面／檔案／HTTP 的功能。

`DbStartup` 三個 public static 方法，`Program.cs` 依序呼叫，不自己重寫這段邏輯：

1. `DbStartup.HandleEncryptDb(args)` —— `args` 含 `--encrypt-db` 就呼叫 `EncryptInteractive` 並回 `true`，`Program.cs` 直接 `return`，不建 web host。連線字串從 stdin 有東西就讀 stdin（給 Deploy 工具用），否則互動輸入不回顯；失敗只印訊息、exit code 1
2. `builder.AddDBConn()` —— 讀 `Database:Target`，用 if 分派；`Test` 或 `Production`+Development 讀 `ConnectionStrings:{Target}`，`Production` 其他情況讀 DPAPI，其餘丟例外，訊息要說得出「該去哪裡設什麼」。用具名的 local function `CreateDbConn(IServiceProvider)` 把 `new DBConn { connectString = ... }` 註冊為 Scoped；不用 lambda、switch expression 或 expression-bodied 成員（專案慣例：不用箭頭寫法）
3. `app.LogDbTarget()` —— `builder.Build()` 之後記一行啟動 Log：Target、Server、Catalog（用 `SqlConnectionStringBuilder` 取，**不含密碼**）

要改連線邏輯改 `DbStartup`，所有站一起變。

## 8. ADO.NET 與 SQL 規則

- 用 `SqlConnection`、`SqlCommand`、`SqlDataReader`、`SqlParameter`。
- 不可參數化的表名、欄位名、排序方向用固定白名單。
- 非同步操作傳 `CancellationToken`，`CommandTimeout` 依實際操作設定。
- 交易在同一條連線執行，相關 `SqlCommand` 明確指定 `Transaction`；不為此建立 UnitOfWork。
- 以 `using`／`await using` 釋放連線、Command、Reader 與交易。
- 業務 SQL 放 Services 或系統對應類別，`DBConn` 不含 Patient／Order／Report SQL。

各專案自己的 SQL 慣例（跳脫方式、民國日期處理、取值方式）以該專案的 `CLAUDE.md` 為準。

## 9. TLS

`Microsoft.Data.SqlClient` 4.0 起 `Encrypt` 預設為 `True`，所以流量預設就有加密。

`TrustServerCertificate=True` 表示**不驗證伺服器身分**，理論上可被中間人攔截。目標是讓 DBA 在 SQL Server 裝一張內部 CA 簽的憑證，之後改成 `TrustServerCertificate=False`。

這是連線字串內容的調整，不用改程式、不用重新發布，只要重跑 `--encrypt-db --force`。在憑證就位之前 `TrustServerCertificate=True` 是已知且記錄在案的暫時狀態，不是「忘了處理」。

DPAPI 是儲存加密，TLS 是傳輸加密，兩者不互相取代。

## 10. 錯誤處理與 Log

設定遺失、格式錯誤、解密失敗、檔案權限不足、連線字串無效 —— 一律**停止啟動**。

禁止：解密失敗後把密文當明碼用、改試其他 scope、改讀環境變數、或連到其他 DB。

Log 可記錄：錯誤代碼、`Database:Target`、Server、Catalog。

Log **不得**記錄：完整 `IConfiguration`、連線字串、Password、密文、解密內容、包含敏感輸入的原始例外。

啟動失敗向使用者回傳一般錯誤，詳細診斷只供授權維運人員查看。

## 11. 版控

`.gitignore` 至少要有：`*.dat`／實際採用的密文檔副檔名、`*.user`、`bin`／`obj`。

密文檔放在 `C:\WebConfig` 本來就在工作目錄外，但仍要有規則擋住有人手滑複製進來。

忽略規則**不能**移除已經提交的秘密。發現曾提交秘密，該組帳密即視為已外洩，須停止使用並依流程輪替。

## 12. 驗收條件

實作後必須實際跑過。只用測試帳密驗證工具與失敗情境；正式連線只做授權的非破壞性驗證。

| 驗收項目 | 預期結果 |
| --- | --- |
| 全新 clone 直接 F5 | 連上測試 DB，顯示「測試環境」，不需任何額外設定 |
| 未設 `Database__Target` | 啟動失敗，訊息說得出要設什麼 |
| `Database__Target` 是未知值 | 啟動失敗，無 fallback |
| `Target=Test` 但沒有 `ConnectionStrings:Test` | 啟動失敗 |
| `Target=Production`、Development | 連上正式 DB（Windows 驗證），不讀密文檔，無「測試環境」字樣 |
| `Target=Production`、非 Development 但密文檔不存在 | 啟動失敗，訊息指向 `--encrypt-db` |
| 密文檔被改壞一個字元 | 啟動失敗，訊息不含檔案內容 |
| 密文檔從別台主機複製過來 | 解密失敗，啟動失敗 |
| 有效密文檔 | 啟動成功，Log 出現 Target／Server／Catalog |
| Log 內容檢查 | 搜不到密碼、密文或完整連線字串 |
| 一般 Windows 帳號讀密文檔 | 被檔案權限拒絕 |
| 網站帳號嘗試修改密文檔 | 被檔案權限拒絕 |
| 替換密文但未重新啟動 | 舊程序維持啟動時的設定 |
| 替換密文並重新啟動 | 載入新值 |
| 發布產物檢查 | 目錄下搜不到任何密碼；`appsettings.json` 沒有 `ConnectionStrings` |

`--encrypt-db` 加密前先 `SqlConnection.Open()` 一次（帳密錯在部署現場就擋下，不寫檔），寫檔後立刻讀回比對就是 DPAPI 往返的檢查；兩者都跑在真正重要的時機（部署當下），不另外開測試專案。

IIS 身分、ACL 與 SQL 憑證必須實機驗證，不能只靠單元測試。

## 13. 最終原則

DPAPI 保護的是**靜態儲存**的秘密。程式建立 SQL 連線時仍需使用可讀取的連線資訊，取得網站執行身分或控制程序的人仍可能讀到秘密。

因此作業系統權限、DB 帳號最小權限（不用 sa／sysadmin 作為應用程式帳號）、TLS 與維運控管都還是要做，**不能因為「有加密」就省掉**。

不得把「通過 DPAPI 加密」宣稱為「保證通過所有資安稽核」。

# WebToolNet

公司 HIS 網站共用的 .NET 10 類別庫，從 .NET Framework 版搬過來。各網站以 Project Reference 引用，不做 NuGet。

規則與理由見 `../Docs/NET10-MVC-MSSQL-DPAPI-Spec.md`，這份只講怎麼用。

---

## 1. 裡面有什麼

| 命名空間 | 用途 | 入口 |
| --- | --- | --- |
| `WebToolNet.DBConn` | 連 MSSQL、決定連測試還是正式 DB、正式站密碼加密存放 | `DbStartup`、`DBConn`、`DbSecret` |
| `WebToolNet.UtilExtension` | 上百個 `p` 開頭的擴充方法：`pRyyymmdd`、`pSQLValidator`、`pCol`、`pToInt`、`pLeft`…；`WebHelper` 有 `HttpContext.GetClientIP()` | `StringTool.cs`（先看這個檔）、`WebHelper.cs` |
| `WebToolNet.myDateTime` | 民國／西元換算、算年齡：`TWD2DateTime`、`TWDsAge` | `DateComputing` |
| `WebToolNet.Validation` | 身分證（本國／外籍／居留證）、民國日期、時間格式檢查 | `CheckID`、`CheckDate` |
| `WebToolNet.DBTool` | DataTable／Dictionary 轉 INSERT／DELETE SQL | `DataTableTool`、`DBTool` |
| `WebToolNet.myString` | `Left`／`Right`／`Mid`、依位元組切字串 | `StringCut` |

寫任何工具函式前先 grep 這裡有沒有現成的。擴充方法一律 `p` 開頭：`dateFrom.pRyyymmdd()`、`str.pSQLValidator()`、`row.pCol("欄位名")`。

---

## 2. 新網站怎麼引用

WebToolNet 是獨立 repo，**clone 在網站 repo 的隔壁**，兩個資料夾同一層：

```
任意資料夾\
├── WebToolNet\        ← git clone https://github.com/gamer99122dev/WebToolNet.git
└── <網站>\            ← 你的網站 repo
```

網站專案（`Microsoft.NET.Sdk.Web`、`net10.0`）加兩處：

`<網站>.csproj`：

```xml
<ItemGroup>
  <ProjectReference Include="..\WebToolNet\WebToolNet.csproj" />
</ItemGroup>
```

`<網站>.slnx`（或 Visual Studio：方案總管 → 右鍵方案 → 加入 → 現有專案 → 選 `..\WebToolNet\WebToolNet.csproj`）：

```xml
<Solution>
  <Project Path="../WebToolNet/WebToolNet.csproj" />
  <Project Path="<網站>.csproj" />
</Solution>
```

建置成功就接好了。同事 clone 你的網站時也要一起 clone WebToolNet 到隔壁，寫進你網站的 README。

---

## 3. 接上 DB

### 3.1 複製 `Templates/` 的四個檔

`Templates/` 的目錄結構就是網站專案的結構，**照相同相對路徑複製過去，覆蓋 Visual Studio 產生的**：

| `Templates/` 裡的檔案 | 複製到網站專案 | 複製後 |
| --- | --- | --- |
| `Program.cs` | `Program.cs` | 三行 `DbStartup` 呼叫已經在裡面，再加你自己的服務註冊、改預設路由 |
| `appsettings.Development.json` | `appsettings.Development.json` | 不用改。`ConnectionStrings:Test` / `Production` 都是 Windows 驗證、沒密碼，可以進 git。這個站連別的 DB 才改 `Database=` |
| `Properties/launchSettings.json` | `Properties/launchSettings.json` | 兩個啟動設定「測試DB」「正式DB」。port 要不一樣就改 `applicationUrl` |
| `Views/Shared/_TestEnv.cshtml` | `Views/Shared/_TestEnv.cshtml` | 不用改。在 layout 或各頁放 `<partial name="_TestEnv" />`，非正式 DB 時畫面出現紅字「測試環境」 |

`appsettings.json` **不能有** `ConnectionStrings`。

### 3.2 `Program.cs` 裡的三行

範本裡 DB 相關的只有這三行，連線邏輯全在 `DbStartup.cs`，各站不要自己重寫：

```csharp
using WebToolNet.DBConn;

if (DbStartup.HandleEncryptDb(args)) return;          // 部署時 <網站>.exe --encrypt-db 走這裡

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.AddDBConn();                                   // 讀 Database__Target 決定連哪個 DB，註冊 DBConn

WebApplication app = builder.Build();
app.LogDbTarget();                                     // 啟動 log 印 Target / Server / Database，不含帳密
```

### 3.3 用 DBConn

```csharp
using System.Data;
using WebToolNet.UtilExtension;   // pSQLValidator、pCol 這些擴充方法

public class FooController : Controller
{
    private readonly WebToolNet.DBConn.DBConn _db;
    public FooController(WebToolNet.DBConn.DBConn db) => _db = db;

    public IActionResult Index(string MrNo)
    {
        string sMrNo = MrNo.pSQLValidator();                         // 進 SQL 的字串一律跳脫
        string SQL = "SELECT chName FROM DB_OPD..PatientTbl ";
        SQL += $"\n WHERE chMrNo = '{sMrNo}' ";
        DataTable dt = _db.executesqldt(SQL);                        // 查詢回 DataTable
        string name = dt.Rows[0].pCol("chName");                     // 取值用 pCol，會自動 Trim
        ...
    }
}
```

`executesqltrans(SQL)` 是有交易的寫入，失敗看 `ExecuteFail` / `_ExecuteFailMsg`。

---

## 4. 切換測試／正式 DB

只有一個開關 `Database__Target`，Visual Studio 上方的啟動設定下拉選單就是在切它：

| 啟動設定 | `Database__Target` | 連線字串來源 | 畫面 |
| --- | --- | --- | --- |
| **測試DB**（預設，F5） | `Test` | `appsettings.Development.json`，你自己的 AD 帳號 | 紅字「測試環境」 |
| **正式DB** | `Production` | `appsettings.Development.json`，你自己的 AD 帳號 | 沒有紅字，動資料前想清楚 |
| （IIS 正式站） | `Production` | DPAPI 密文檔 `C:\ProgramData\HIS\db.dat`，SQL 帳號 | 沒有紅字 |

沒設、設錯 → **啟動失敗**，訊息會說要去哪裡設。不會猜、不會 fallback。

`ASPNETCORE_ENVIRONMENT` 維持框架原本用途（載入哪個 appsettings、顯不顯示詳細錯誤頁），不拿來決定 DB。本機兩個啟動設定都是 `Development`，正式站是 `Production`。

---

## 5. 部署到 IIS

密文檔是**一台機器一個檔、所有網站共用**（公司 DB 帳密都一樣）。所以 5.2 每台機器只做一次，其餘每個站各做。
可以先在自己電腦的 IIS 走一遍再上正式主機；`--encrypt-db` 時貼**測試 DB** 的連線字串，不要在開發機放正式密碼。

### 5.1 主機前置（每台機器一次）

1. 安裝 **ASP.NET Core 10.0 Hosting Bundle**：<https://dotnet.microsoft.com/download/dotnet/10.0> → Hosting Bundle。
   裝完在系統管理員命令列跑 `net stop was /y` 再 `net start w3svc`，IIS 才看得到新模組。
2. IIS 管理員確認「模組」裡有 `AspNetCoreModuleV2`。

### 5.2 建密文檔（每台機器一次）

這台機器已經有別的站做過就跳過（跑了也只會回「密文檔已存在」）。要先完成一次 5.4 發布才有 exe 可跑。

以**系統管理員**開 PowerShell：

```powershell
cd C:\inetpub\wwwroot\<網站>
.\<網站>.exe --encrypt-db
```

貼上連線字串（畫面不顯示），格式：

```
Server=<主機>;Database=<DB>;User ID=<帳號>;Password=<密碼>;Encrypt=True;TrustServerCertificate=True;
```

工具會驗證語法 → 加密寫到 `C:\ProgramData\HIS\db.dat` → 立刻讀回比對 → 只印出 Server 和 Database。要覆蓋既有檔加 `--force`。

接著收緊權限，**不可略過**（DPAPI LocalMachine 表示這台機器上讀得到檔的帳號都解得開，ACL 才是主要防線）：

```powershell
icacls "C:\ProgramData\HIS\db.dat" /inheritance:r `
  /grant "SYSTEM:(F)" "Administrators:(F)" "IIS AppPool\<網站>:(R)"
```

### 5.3 建集區和應用程式（每個站）

IIS 管理員：

1. 應用程式集區 → 新增 → 名稱 `<網站>`，.NET CLR 版本選「**無受控碼**」。
2. 檔案總管先建 `C:\inetpub\wwwroot\<網站>`。
3. Default Web Site → 右鍵 → 新增應用程式 → 別名 `<網站>`、集區選 `<網站>`、實體路徑選上一步的資料夾。
4. 集區環境變數：選集區 → 設定編輯器 → 區段 `system.applicationHost/applicationPools` → 找到 `<網站>` → `environmentVariables` → 加兩筆：

   | 名稱 | 值 |
   | --- | --- |
   | `ASPNETCORE_ENVIRONMENT` | `Production` |
   | `Database__Target` | `Production` |

   **不要**設 `ConnectionStrings__Production` 之類的明碼變數。

5. 密文檔已存在（不是這台第一個站）就補這個站的讀取權限：

   ```powershell
   icacls "C:\ProgramData\HIS\db.dat" /grant "IIS AppPool\<網站>:(R)"
   ```

### 5.4 發布（每個站，每次更新）

Visual Studio：右鍵專案 → 發佈 → 資料夾 → 目標 `C:\inetpub\wwwroot\<網站>`。或命令列：

```powershell
dotnet publish -c Release -o C:\inetpub\wwwroot\<網站>
```

更新已在跑的站要先**停止集區**再發布，否則 dll 被鎖住會失敗；發布完再啟動。密文檔在 `C:\ProgramData`，發布動不到它。

### 5.5 確認

開 `http://<主機>/<網站>`：頁面正常、**沒有**紅字「測試環境」。

要確認連到哪台 DB，看啟動 log。IIS 預設不留 stdout，臨時打開：`C:\inetpub\wwwroot\<網站>\web.config` 的 `stdoutLogEnabled="false"` 改 `true`，同目錄建 `logs` 資料夾，回收集區，發一次請求，開 `logs\stdout_*.log` 找：

```
DB Target=Production Server=<主機> Catalog=<DB>
```

Server / Catalog 對、檔案裡搜不到密碼，就成了。看完改回 `false`（stdout log 只用來排錯，會一直長；下次發布也會把 web.config 蓋回去）。

### 5.6 常見錯誤

| 現象 | 原因 | 怎麼修 |
| --- | --- | --- |
| 500.30，事件檢視器 → Windows 記錄 → 應用程式有 `Database__Target 未設定` | 集區沒設環境變數 | 5.3 第 4 步 |
| 500.30，`找不到 DB 密文檔` | 沒跑過 `--encrypt-db` | 5.2 |
| 500.30，`DB 密文檔解密失敗` | 檔案是從別台機器複製來的 | 在這台重跑 `--encrypt-db --force`，不要複製密文檔 |
| 500.30，`Access to the path ... is denied`（存取被拒） | 集區帳號讀不到密文檔 | 檢查 5.2 / 5.3 第 5 步的 `icacls`，集區名稱要對 |
| 500.19 或 HTTP 錯誤 0x8007000d | Hosting Bundle 沒裝或裝完沒重啟 IIS | 5.1 |

### 5.7 換 DB 密碼

一台機器做一次，這台機器上所有站一起換：重跑 `--encrypt-db --force` → 重打 5.2 的 `icacls`（把這台所有站的集區都列進去）→ 回收所有用到它的集區。密文只在啟動時解一次，不重啟不生效。

---

## 6. 哪些是標準做法、哪些是本公司自訂

**標準（Microsoft 官方文件的做法）**：Project Reference、`launchSettings.json` 啟動設定、`appsettings.{環境}.json`、`ASPNETCORE_ENVIRONMENT`、IIS 集區環境變數、資料夾發布、Hosting Bundle、DPAPI（`ProtectedData`）。

**本公司自訂**：
- `Database__Target` 這個開關名稱。標準只有 `ASPNETCORE_ENVIRONMENT`，但「本機 Development 模式連正式 DB 除錯」需要第二個開關，所以多這一個。
- 密文檔 `C:\ProgramData\HIS\db.dat` 全機共用、`--encrypt-db` 內建在網站 exe。等同 .NET Framework 時代 `aspnet_regiis -pe` 加密連線字串的做法，只是 .NET Core 沒有那個工具，自己補了一個。
- `p` 開頭的擴充方法命名。

正式站如果哪天集區能拿到有 DB 權限的網域帳號（走 Windows 驗證），密文檔整套就不需要了，`DbStartup` 改讀 `ConnectionStrings` 即可。

# WebToolNet

公司 HIS 網站共用的 .NET 10 類別庫。各網站以 Project Reference 引用，不做 NuGet。
這份只講怎麼用；為什麼這樣設計見 [`Docs/NET10-MVC-MSSQL-DPAPI-Spec.md`](Docs/NET10-MVC-MSSQL-DPAPI-Spec.md)。

## 1. 引用

WebToolNet 是獨立 repo，clone 在網站 repo 的隔壁：

```
任意資料夾\
├── WebToolNet\        ← git clone https://github.com/gamer99122dev/WebToolNet.git
└── <網站>\            ← 你的網站 repo
```

`<網站>.csproj`：

```xml
<ItemGroup>
  <ProjectReference Include="..\WebToolNet\WebToolNet.csproj" />
</ItemGroup>
```

`<網站>.slnx`（或 Visual Studio：方案總管 → 右鍵方案 → 加入 → 現有專案）：

```xml
<Solution>
  <Project Path="../WebToolNet/WebToolNet.csproj" />
  <Project Path="<網站>.csproj" />
</Solution>
```

同事 clone 你的網站時也要一起 clone WebToolNet，寫進網站的 README。

## 2. 接上 DB

把 `Templates/` 的四個檔照相同相對路徑複製到網站專案，覆蓋 Visual Studio 產生的：

| 檔案 | 說明 |
| --- | --- |
| `Program.cs` | DB 接線三行已在裡面，再加你自己的服務註冊 |
| `appsettings.Development.json` | 測試／正式 DB 連線字串，Windows 驗證、沒密碼，可進 git。連別的 DB 才改 `Database=` |
| `Properties/launchSettings.json` | 兩個啟動設定「測試DB」「正式DB」，port 要改就改 `applicationUrl` |
| `Views/Shared/_TestEnv.cshtml` | 在 layout 放 `<partial name="_TestEnv" />`，非正式 DB 時畫面出現紅字「測試環境」 |

`appsettings.json` **不能有** `ConnectionStrings`。

`Program.cs` 裡 DB 相關只有這三行，連線邏輯全在 `DbStartup`，各站不要自己寫：

```csharp
using WebToolNet.Data;

if (DbStartup.HandleEncryptDb(args)) return;   // 部署時 <網站>.exe --encrypt-db 走這裡

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.AddDBConn();                            // 讀 Database__Target 決定連哪個 DB，註冊 DBConn

WebApplication app = builder.Build();
app.LogDbTarget();                              // 啟動 log 印 Target / Server / Database
```

## 3. 寫程式

```csharp
using System.Data;
using WebToolNet.Data;            // DBConn
using WebToolNet.Extensions;      // p 開頭的擴充方法

public class FooController : Controller
{
    private readonly DBConn _db;
    public FooController(DBConn db) => _db = db;

    public IActionResult Index(string MrNo)
    {
        string sMrNo = MrNo.pSQLValidator();                 // 進 SQL 的字串一律跳脫
        string SQL = "SELECT chName FROM DB_OPD..PatientTbl ";
        SQL += $"\n WHERE chMrNo = '{sMrNo}' ";
        DataTable dt = _db.Query(SQL);                       // 查詢回 DataTable
        string name = dt.Rows[0].pCol("chName");             // 取值用 pCol，會自動 Trim
        ...
    }
}
```

`Execute(SQL)` 是有交易的寫入，失敗看 `ExecuteFail` / `ExecuteFailMsg`。

| 命名空間 | 內容 |
| --- | --- |
| `WebToolNet.Data` | `DBConn`（查詢）、`DbStartup`／`DbSecret`（啟動接線、密文檔）、`DataTableTool`／`DBTool`（DataTable 轉 SQL）、`SqlEscape` |
| `WebToolNet.Extensions` | 上百個 `p` 開頭的擴充方法，一檔一類、檔名＝被擴充的型別（`StringTool`、`DateTimeUtil`、`DataRowTool`…）；`WebHelper` 有 `HttpContext.GetClientIP()` |
| `WebToolNet.Dates` | 民國／西元換算、算年齡：`DateComputing`、`GenerateDateString` |
| `WebToolNet.Validation` | 身分證、民國日期、時間格式檢查：`CheckID`、`CheckDate` |

寫任何工具函式前先 `grep -rn "pXxx" Extensions/` 看有沒有現成的。

## 4. 測試／正式 DB

只有一個開關 `Database__Target`，Visual Studio 的啟動設定下拉選單就是在切它：

| 啟動設定 | `Database__Target` | 連線字串來源 | 畫面 |
| --- | --- | --- | --- |
| **測試DB**（預設，F5） | `Test` | `appsettings.Development.json`，Windows 驗證 | 紅字「測試環境」 |
| **正式DB** | `Production` | `appsettings.Development.json`，Windows 驗證 | 沒有紅字，動資料前想清楚 |
| IIS 正式站 | `Production` | 密文檔 `C:\ProgramData\HIS\db.dat`，SQL 帳號 | 沒有紅字 |

沒設、設錯 → 啟動失敗，訊息會說要去哪裡設。

## 5. 部署到 IIS

照順序做。密文檔一台機器一個檔、所有網站共用，所以 5.1 和 5.4 每台機器只做一次，其餘每個站各做。
可以先在自己電腦的 IIS 走一遍；`--encrypt-db` 時貼測試 DB 的連線字串，不要在開發機放正式密碼。

### 5.1 主機前置（每台機器一次）

安裝 ASP.NET Core 10.0 Hosting Bundle：<https://dotnet.microsoft.com/download/dotnet/10.0>。
裝完以系統管理員命令列跑 `net stop was /y` 再 `net start w3svc`。

### 5.2 建集區和應用程式（每個站）

IIS 管理員：

1. 應用程式集區 → 新增 → 名稱 `<網站>`，.NET CLR 版本選「無受控碼」。
2. 建資料夾 `C:\inetpub\wwwroot\<網站>`；Default Web Site → 新增應用程式 → 別名 `<網站>`、集區 `<網站>`、實體路徑選它。
3. 集區環境變數（設定編輯器 → `system.applicationHost/applicationPools` → `<網站>` → `environmentVariables`）加兩筆：

   | 名稱 | 值 |
   | --- | --- |
   | `ASPNETCORE_ENVIRONMENT` | `Production` |
   | `Database__Target` | `Production` |

   不要設 `ConnectionStrings__*` 之類的明碼變數。

### 5.3 發布（每個站，每次更新）

更新已在跑的站要先停止集區，發完再啟動：

```powershell
dotnet publish -c Release -o C:\inetpub\wwwroot\<網站>
```

或 Visual Studio：右鍵專案 → 發佈 → 資料夾。密文檔在 `C:\ProgramData`，發布動不到它。

### 5.4 建密文檔（每台機器一次）

這台機器已經有別的站做過就跳到第 3 步。以系統管理員開 PowerShell：

1. 跑任一個站的 exe：

   ```powershell
   cd C:\inetpub\wwwroot\<網站>
   .\<網站>.exe --encrypt-db
   ```

   貼上連線字串（畫面不顯示）：`Server=<主機>;Database=<DB>;User ID=<帳號>;Password=<密碼>;Encrypt=True;TrustServerCertificate=True;`
   要覆蓋既有檔加 `--force`。

2. 收緊權限，**不可略過**：

   ```powershell
   icacls "C:\ProgramData\HIS\db.dat" /inheritance:r `
     /grant "SYSTEM:(F)" "Administrators:(F)" "IIS AppPool\<網站>:(R)"
   ```

3. 之後每加一個站補它的讀取權限：

   ```powershell
   icacls "C:\ProgramData\HIS\db.dat" /grant "IIS AppPool\<網站>:(R)"
   ```

### 5.5 確認

開 `http://<主機>/<網站>`：頁面正常、**沒有**紅字「測試環境」。

要確認連到哪台 DB：`C:\inetpub\wwwroot\<網站>\web.config` 的 `stdoutLogEnabled` 暫時改 `true`、同目錄建 `logs` 資料夾、回收集區、發一次請求，`logs\stdout_*.log` 裡找 `DB Target=Production Server=<主機> Catalog=<DB>`。看完改回 `false`。

### 5.6 出錯

| 現象 | 怎麼修 |
| --- | --- |
| 500.30，事件檢視器有 `Database__Target 未設定` | 5.2 第 3 步 |
| 500.30，`找不到 DB 密文檔` | 5.4 |
| 500.30，`DB 密文檔解密失敗` | 密文檔是從別台複製來的，在這台重跑 `--encrypt-db --force` |
| 500.30，`Access to the path ... is denied` | 集區帳號讀不到密文檔，檢查 5.4 的 `icacls`，集區名稱要對 |
| 500.19 或 HTTP 錯誤 0x8007000d | Hosting Bundle 沒裝或裝完沒重啟 IIS |

換 DB 密碼：重跑 `--encrypt-db --force` → 重打 5.4 的 `icacls`（這台所有站的集區都列進去）→ 回收所有集區。

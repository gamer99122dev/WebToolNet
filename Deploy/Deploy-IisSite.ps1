<#
雙擊「部署網站.cmd」會用系統管理員身分跑這支。對應 WebToolNet/README.md 第 5 節：
  新站   → 建集區（無受控碼 + 兩個環境變數）、建應用程式、收密文檔權限
  既有站 → 停集區、等使用者放新檔、收密文檔權限、啟動集區
檔案由使用者自己放進網站資料夾（VS 直接發佈到那裡、或檔案總管複製），工具不複製：
提權後的視窗收不到從檔案總管拖進來的東西（UIPI），而且開發機本來就是 VS 直接發到那裡。
網站資料夾在哪以 IIS 記的實體路徑為準，不寫死磁碟；新站預設跟其他站放同一層，可改。
重跑安全。IT 也可以直接在 PowerShell 跑：.\Deploy-IisSite.ps1
直接用 IIS 內建的 Microsoft.Web.Administration.dll，不依賴 WebAdministration／IISAdministration 模組，主機不一定有裝那個功能。
#>

# ── 設定 ────────────────────────────────────────────────
$SecretFile = 'C:\WebConfig\db.dat'   # 要跟 WebToolNet 的 DbSecret.DefaultPath 一樣
$Contact    = '資訊室'                # 出錯時叫使用者找誰
# ── 設定結束 ────────────────────────────────────────────

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $false   # PowerShell 7.4 起外部程式回非 0 會直接當錯誤，icacls 的結果自己看 $LASTEXITCODE

# 沒提權就用系統管理員重開自己（UAC 會跳出來），雙擊 .cmd 的人不用知道什麼叫「以系統管理員執行」
$identity = [Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()
if (-not $identity.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    try {
        Start-Process -FilePath (Get-Process -Id $PID).Path -Verb RunAs `
            -ArgumentList "-NoProfile -ExecutionPolicy Bypass -File `"$PSCommandPath`""
    }
    catch {
        Write-Host '沒有取得系統管理員權限（UAC 按了「否」？），什麼都沒做。' -ForegroundColor Red
        Read-Host '按 Enter 關閉'
    }
    exit
}

$stoppedPool = ''   # 工具自己停掉的集區。中途失敗它會一直停著（網站 503），要提醒使用者

function Stop-Fail([string] $Message) {
    Write-Host ''
    Write-Host "失敗：$Message" -ForegroundColor Red
    if ($stoppedPool -ne '') { Write-Host "集區 $stoppedPool 目前是停止的，網站會顯示 Service Unavailable；問題解決後再跑一次這個工具就會啟動。" -ForegroundColor Yellow }
    Write-Host "請截圖這個畫面找 $Contact。" -ForegroundColor Yellow
    Read-Host '按 Enter 關閉'
    exit 1
}

try {
    # 1. 機器前置（README 5.1）
    $inetsrv = "$env:windir\System32\inetsrv"
    if (-not (Test-Path "$inetsrv\Microsoft.Web.Administration.dll")) { Stop-Fail '這台機器沒有安裝 IIS。' }
    Add-Type -Path "$inetsrv\Microsoft.Web.Administration.dll"
    $mgr = New-Object Microsoft.Web.Administration.ServerManager
    # Hosting Bundle 裝好會在 IIS 註冊 AspNetCoreModuleV2，查註冊比找 dll 準（新版 Bundle 的 dll 不在 inetsrv）
    $ancm = $null
    foreach ($e in $mgr.GetApplicationHostConfiguration().GetSection('system.webServer/globalModules').GetCollection()) {
        if ($e['name'] -eq 'AspNetCoreModuleV2') { $ancm = $e }
    }
    if ($null -eq $ancm -or -not (Test-Path "$env:ProgramFiles\dotnet\shared\Microsoft.AspNetCore.App\10.*")) {
        Stop-Fail '這台機器沒有安裝 ASP.NET Core 10.0 Hosting Bundle（README 5.1，每台機器一次）。'
    }
    # 2. 站台：正式機的站台不一定叫 Default Web Site，也可能不只一個。只有一個就直接用，多個才問
    $sites = @($mgr.Sites)
    if ($sites.Count -eq 0) { Stop-Fail 'IIS 裡沒有任何站台。' }
    $iisSite = $sites[0]
    if ($sites.Count -gt 1) {
        Write-Host 'IIS 上有多個站台：'
        for ($i = 0; $i -lt $sites.Count; $i++) { Write-Host ("  {0}) {1}" -f ($i + 1), $sites[$i].Name) }
        $ans = (Read-Host '網站在哪個站台底下？輸入編號').Trim()
        if (-not ($ans -match '^\d+$') -or [int]$ans -lt 1 -or [int]$ans -gt $sites.Count) { Stop-Fail '編號不對。' }
        $iisSite = $sites[[int]$ans - 1]
    }
    # 最後印網址用：站台綁的不是 80 就要帶 port
    $port = 80
    foreach ($b in $iisSite.Bindings) { if ($b.Protocol -eq 'http') { $port = [int]$b.BindingInformation.Split(':')[1]; break } }
    $urlBase = "http://$env:COMPUTERNAME" + $(if ($port -ne 80) { ":$port" } else { '' })

    # 3. 選網站。既有站的實體路徑以 IIS 記的為準，不假設在哪個磁碟（正式機在 D:、開發機在 C:\inetpub\wwwroot）
    #    只列 .NET Core 站（web.config 有 aspNetCore），舊 .NET Framework 站不列也不碰。站台根目錄本身是 .NET Core 站也列
    function Get-AppPath($App) { return [Environment]::ExpandEnvironmentVariables($App.VirtualDirectories['/'].PhysicalPath) }
    function Get-AppLabel($App) { if ($App.Path -eq '/') { return "$($iisSite.Name)（站台根目錄）" } else { return $App.Path.TrimStart('/') } }
    function Test-CoreApp($Dir) { return (Test-Path "$Dir\web.config") -and ((Get-Content "$Dir\web.config" -Raw) -match 'aspNetCore') }

    $apps = @()
    foreach ($a in $iisSite.Applications) {
        if (Test-CoreApp (Get-AppPath $a)) { $apps += $a }
    }
    Write-Host "站台「$($iisSite.Name)」底下現有的 .NET 站："
    for ($i = 0; $i -lt $apps.Count; $i++) {
        Write-Host ("  {0}) {1}   {2}" -f ($i + 1), (Get-AppLabel $apps[$i]), (Get-AppPath $apps[$i]))
    }
    $ans = (Read-Host '要部署哪一個？輸入編號；新站直接輸入網站名稱').Trim()
    if ($ans -eq '') { Stop-Fail '沒有輸入。' }

    $app = $null
    if ($ans -match '^\d+$' -and [int]$ans -ge 1 -and [int]$ans -le $apps.Count) { $app = $apps[[int]$ans - 1] }
    else { $app = $iisSite.Applications["/$ans"] }   # 打名字也可能是既有站

    if ($null -ne $app) {
        $name     = Get-AppLabel $app
        $dst      = Get-AppPath $app
        $poolName = $app.ApplicationPoolName
        $url      = "$urlBase/" + $app.Path.TrimStart('/')
        if ((Test-Path "$dst\web.config") -and -not (Test-CoreApp $dst)) {
            Stop-Fail "$name 是舊的 .NET Framework 站，這個工具不處理。要換成 .NET 10 版，先在 IIS 管理員移除這個應用程式，再當新站部署。"
        }
        foreach ($other in $iisSite.Applications) {
            if ($other.Path -ne $app.Path -and $other.ApplicationPoolName -eq $poolName) {
                Stop-Fail "集區 $poolName 還有別的應用程式（$($other.Path)）在用，改它會連累別站。請在 IIS 管理員給 $name 自己的集區。"
            }
        }
        Write-Host "更新既有站 $name，資料夾 $dst"
    }
    else {
        $name     = $ans
        $poolName = $name
        # 新站放哪一層：預設跟最後一個現有站同一層，一個都沒有就用站台根目錄；可以改
        $parent = Get-AppPath $iisSite.Applications['/']
        if ($apps.Count -gt 0) { $parent = Split-Path (Get-AppPath $apps[$apps.Count - 1]) -Parent }
        $typed = (Read-Host "新站資料夾要放在哪一層？直接按 Enter 用「$parent」").Trim().Trim('"')
        if ($typed -ne '') { $parent = $typed }
        $dst = Join-Path $parent $name
        $url = "$urlBase/$name"
        Write-Host "建新站 $name，資料夾 $dst"
    }
    $pool = $mgr.ApplicationPools[$poolName]

    # 3. 換檔前先停集區，不然 DLL 使用中，VS 發佈或複製都會失敗。停好了才叫使用者放檔
    if ($null -ne $pool -and $pool.State -ne 'Stopped') {
        $pool.Stop() | Out-Null
        $wait = 0
        while ($pool.State -ne 'Stopped' -and $wait -lt 30) { Start-Sleep -Seconds 1; $wait++ }
        $stoppedPool = $poolName
        Write-Host "集區 $poolName 已停止。" -ForegroundColor Yellow
    }
    New-Item -ItemType Directory -Force $dst | Out-Null
    if ($stoppedPool -ne '' -or -not (Test-Path "$dst\web.config")) {
        Write-Host "現在把新版網站檔案放進 $dst（Visual Studio 直接發佈到這裡、或用檔案總管複製進來都可以）。"
        Read-Host '放好了按 Enter' | Out-Null
    }
    if (-not (Test-Path "$dst\web.config")) { Stop-Fail "$dst 裡沒有 web.config，不是發布好的網站檔案。" }

    # 4. 集區：無受控碼 + 兩個環境變數（README 5.3）。找得到就改值、找不到就新增，重跑不會重複
    if ($null -eq $pool) { $pool = $mgr.ApplicationPools.Add($poolName) }
    $pool.ManagedRuntimeVersion = ''
    $vars = $pool.GetCollection('environmentVariables')
    foreach ($kv in @{ ASPNETCORE_ENVIRONMENT = 'Production'; Database__Target = 'Production' }.GetEnumerator()) {
        $found = $null
        foreach ($v in $vars) { if ($v['name'] -eq $kv.Key) { $found = $v } }
        if ($null -ne $found) {
            $found['value'] = $kv.Value
        }
        else {
            $v = $vars.CreateElement('add')
            $v['name']  = $kv.Key
            $v['value'] = $kv.Value
            $vars.Add($v) | Out-Null
        }
    }

    # 5. 應用程式（README 5.3）
    if ($null -eq $app) { $app = $iisSite.Applications.Add("/$name", $dst) }
    $app.ApplicationPoolName = $poolName
    $mgr.CommitChanges()

    # 6. 密文檔（README 5.4）。每台機器一個檔，第一次沒有就在這裡建
    if (-not (Test-Path $SecretFile)) {
        Write-Host ''
        Write-Host "這台機器還沒有 DB 密文檔（$SecretFile），第一次部署要建一次，之後的站不用。" -ForegroundColor Yellow
        Write-Host '要輸入正式 DB 的四個資料：主機、資料庫名稱、SQL 帳號、密碼。'
        $ans = Read-Host '你有這四個資料嗎？有按 Y，沒有按 N'
        if ($ans -ne 'Y' -and $ans -ne 'y') {
            Stop-Fail '還沒有 DB 密文檔，網站開不起來。跟 IT 要到那四個資料後，再跑一次這個工具。'
        }
        $server = (Read-Host '  DB 主機（IP 或名稱，例如 192.168.1.10 或 SQLHOST\INST）').Trim()
        $dbName = (Read-Host '  資料庫名稱（例如 DB_OPD）').Trim()
        $user   = (Read-Host '  SQL 帳號').Trim()
        $secure = Read-Host '  密碼（畫面不會顯示）' -AsSecureString
        $pw = [Runtime.InteropServices.Marshal]::PtrToStringBSTR([Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure))
        if ($server -eq '' -or $dbName -eq '' -or $user -eq '' -or $pw -eq '') { Stop-Fail '四個資料都要填。' }
        # 密碼有 ; 或引號要用雙引號包起來，裡面的雙引號寫兩次，這是連線字串的規則
        if ($pw -match '[;''"]') { $pw = '"' + ($pw -replace '"', '""') + '"' }
        $cs = "Server=$server;Database=$dbName;User ID=$user;Password=$pw;Encrypt=True;TrustServerCertificate=True;"

        # 加密交給網站自己的 exe（DbSecret），格式只有一份；它會先真的連一次 DB，帳密錯會直接說
        $exe = Get-ChildItem $dst -Filter *.exe | Select-Object -First 1
        if ($null -eq $exe) { Stop-Fail "$dst 裡沒有 exe，沒辦法建密文檔。" }
        $OutputEncoding = [Text.Encoding]::UTF8   # 管線餵給 exe 固定用 UTF-8，DbSecret 那邊也固定當 UTF-8 讀
        $cs | & $exe.FullName --encrypt-db
        if ($LASTEXITCODE -ne 0 -or -not (Test-Path $SecretFile)) { Stop-Fail '密文檔沒有建立成功，原因看上面。' }
    }
    # /inheritance:r 只砍繼承來的「所有人可讀」，不動其他站已經拿到的授權，所以每次都重打是安全的
    icacls $SecretFile /inheritance:r /grant 'SYSTEM:(F)' 'Administrators:(F)' "IIS AppPool\${poolName}:(R)" | Out-Null
    if ($LASTEXITCODE -ne 0) { Stop-Fail "設定密文檔權限失敗（icacls 代碼 $LASTEXITCODE）。" }

    # 7. 集區不是啟動狀態就啟動——不只啟動自己停的：上一次中途失敗留下的停止狀態也要救回來。
    #    用新的 ServerManager 看，剛 Commit 的新集區才查得到狀態
    $live = (New-Object Microsoft.Web.Administration.ServerManager).ApplicationPools[$poolName]
    if ($live.State -ne 'Started') { $live.Start() | Out-Null }
    $stoppedPool = ''

    Write-Host ''
    Write-Host "完成。用瀏覽器開 $url 確認：頁面正常、沒有紅字「測試環境」。" -ForegroundColor Green
    Read-Host '按 Enter 關閉'
}
catch {
    Stop-Fail $_.Exception.Message
}

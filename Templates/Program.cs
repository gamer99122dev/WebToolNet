using WebToolNet.Data;

// 新網站的 Program.cs 範本：整個檔覆蓋 Visual Studio 產生的，再加你自己的服務註冊。
// 部署時用：<網站>.exe --encrypt-db [--force]，一台機器跑一次就好，哪個站的 exe 跑都寫同一個檔
if (DbStartup.HandleEncryptDb(args)) return;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// 連哪個 DB 由 Database__Target 決定，邏輯在 WebToolNet.Data.DbStartup（各站共用，不在這裡重寫）；每個 request 一個 DBConn
builder.AddDBConn();

WebApplication app = builder.Build();

// 部署後確認連對機器用。只記 Server/Database，不含帳密。
app.LogDbTarget();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();

using Microsoft.EntityFrameworkCore;
using despachoAeronave.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.IO;
using System.Linq;
using System.Text.Json;

string contentRoot = Directory.GetCurrentDirectory();
var baseDir = AppContext.BaseDirectory;
var dir = new DirectoryInfo(baseDir);
while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "wwwroot")))
{
    dir = dir.Parent;
}
if (dir != null)
{
    contentRoot = dir.FullName;
}

bool isDirectRun = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
if (isDirectRun)
{
    Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
}

string? launchUrl = null;
if (isDirectRun)
{
    try
    {
        var launchSettingsPath = Path.Combine(contentRoot, "Properties", "launchSettings.json");
        if (File.Exists(launchSettingsPath))
        {
            var json = File.ReadAllText(launchSettingsPath);
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("profiles", out var profilesProp) &&
                profilesProp.TryGetProperty("http", out var httpProp) &&
                httpProp.TryGetProperty("applicationUrl", out var urlProp))
            {
                launchUrl = urlProp.GetString();
            }
        }
    }
    catch { }
}

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = contentRoot
});

if (!string.IsNullOrEmpty(launchUrl))
{
    builder.WebHost.UseUrls(launchUrl.Split(';'));
}

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<EscuelaDatabaseContext>(options => 
    options.UseSqlServer(builder.Configuration["ConnectionString:EscuelaDBConnection"]));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
    });

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<EscuelaDatabaseContext>();
        context.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al crear la base de datos.");
    }
}

if (isDirectRun)
{
    app.Lifetime.ApplicationStarted.Register(() =>
    {
        try
        {
            var targetUrl = launchUrl?.Split(';').FirstOrDefault() ?? "http://localhost:5000";
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = targetUrl,
                UseShellExecute = true
            });
        }
        catch { }
    });
}

app.Run();

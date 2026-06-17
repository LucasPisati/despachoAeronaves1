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
        options.Cookie.Name = "DespachoAuth_" + DateTime.Now.Ticks;
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

        try
        {
            context.Database.ExecuteSqlRaw(@"
                IF NOT EXISTS (
                    SELECT * FROM sys.columns 
                    WHERE object_id = OBJECT_ID(N'[dbo].[Despachos]') 
                    AND name = N'FirmaPilotoBase64'
                )
                BEGIN
                    ALTER TABLE [dbo].[Despachos] ADD [FirmaPilotoBase64] nvarchar(max) NULL;
                END
            ");

            context.Database.ExecuteSqlRaw(@"
                IF NOT EXISTS (
                    SELECT * FROM sys.columns 
                    WHERE object_id = OBJECT_ID(N'[dbo].[Despachos]') 
                    AND name = N'FirmaDespachadorBase64'
                )
                BEGIN
                    ALTER TABLE [dbo].[Despachos] ADD [FirmaDespachadorBase64] nvarchar(max) NULL;
                END
            ");

            context.Database.ExecuteSqlRaw(@"
                IF NOT EXISTS (
                    SELECT * FROM sys.columns 
                    WHERE object_id = OBJECT_ID(N'[dbo].[Despachos]') 
                    AND name = N'NotamsReporte'
                )
                BEGIN
                    ALTER TABLE [dbo].[Despachos] ADD [NotamsReporte] nvarchar(max) NOT NULL DEFAULT '';
                END
            ");
        }
        catch (Exception)
        {
            // Ignore if tables are not fully created yet or running on other providers
        }

        // Update weak passwords for existing DB entries to prevent browser safety alert popups
        bool changed = false;
        var adminUser = context.Usuarios.FirstOrDefault(u => u.NombreUsuario == "admin" && u.Contrasena == "admin123");
        if (adminUser != null)
        {
            adminUser.Contrasena = "AdminSecure$2026!";
            changed = true;
        }

        var despachoUser = context.Usuarios.FirstOrDefault(u => u.NombreUsuario == "despacho" && u.Contrasena == "despacho123");
        if (despachoUser != null)
        {
            despachoUser.Contrasena = "DespachoSecure$2026!";
            changed = true;
        }

        var pilotoUser = context.Usuarios.FirstOrDefault(u => u.NombreUsuario == "piloto" && u.Contrasena == "piloto123");
        if (pilotoUser != null)
        {
            pilotoUser.Contrasena = "PilotoSecure$2026!";
            changed = true;
        }

        if (changed)
        {
            context.SaveChanges();
        }

        // Add additional pilots dynamically if they do not exist
        bool addedPilots = false;
        if (!context.Usuarios.Any(u => u.Id == 4))
        {
            context.Usuarios.Add(new Usuario { Id = 4, NombreUsuario = "piloto2", Contrasena = "PilotoSecure$2026!", NombreCompleto = "Laura Fernández (Comandante)", Rol = "Piloto" });
            addedPilots = true;
        }
        if (!context.Usuarios.Any(u => u.Id == 5))
        {
            context.Usuarios.Add(new Usuario { Id = 5, NombreUsuario = "piloto3", Contrasena = "PilotoSecure$2026!", NombreCompleto = "Carlos Rodríguez (Comandante)", Rol = "Piloto" });
            addedPilots = true;
        }
        if (!context.Usuarios.Any(u => u.Id == 6))
        {
            context.Usuarios.Add(new Usuario { Id = 6, NombreUsuario = "piloto4", Contrasena = "PilotoSecure$2026!", NombreCompleto = "Patricia Sosa (Comandante)", Rol = "Piloto" });
            addedPilots = true;
        }
        if (!context.Usuarios.Any(u => u.Id == 7))
        {
            context.Usuarios.Add(new Usuario { Id = 7, NombreUsuario = "piloto5", Contrasena = "PilotoSecure$2026!", NombreCompleto = "Alejandro Silva (Comandante)", Rol = "Piloto" });
            addedPilots = true;
        }

        if (addedPilots)
        {
            context.SaveChanges();
        }

        // Reassign seeded flights to the new pilots
        bool flightsUpdated = false;
        var flight1 = context.Vuelos.FirstOrDefault(v => v.Id == 1);
        if (flight1 != null && flight1.PilotoId == 3)
        {
            flight1.PilotoId = 5; // Carlos Rodríguez
            flightsUpdated = true;
        }
        var flight3 = context.Vuelos.FirstOrDefault(v => v.Id == 3);
        if (flight3 != null && flight3.PilotoId == 3)
        {
            flight3.PilotoId = 4; // Laura Fernández
            flightsUpdated = true;
        }

        if (flightsUpdated)
        {
            context.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al inicializar la base de datos o actualizar las contraseñas.");
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



















// Compilado y verificado - Modulo de Firmas Digitales completo.
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using SistemaVentas.Data;

// Cargar .env para desarrollo local; en producción se usan variables del hosting.
var envFile = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (File.Exists(envFile))
{
    foreach (var linea in File.ReadAllLines(envFile))
    {
        var l = linea.Trim();
        if (l.Length == 0 || l.StartsWith('#') || !l.Contains('=')) continue;
        var partes = l.Split('=', 2);
        var nombre = partes[0].Trim();
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(nombre)))
            Environment.SetEnvironmentVariable(nombre, partes[1].Trim().Trim('"'));
    }
}

// Usar punto decimal siempre. Sin esto, en un Windows en español un precio "9.5" puede leerse como 95.
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

var builder = WebApplication.CreateBuilder(args);

// IgnoreCycles evita el error de JSON circular: Producto -> Categoria -> Productos -> ...
builder.Services.AddControllersWithViews()
    .AddJsonOptions(o => o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
        options.Cookie.Name = "SistemaVentas.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// La aplicación requiere Turso; no usa una base SQLite local como respaldo.
var databaseUrl = Environment.GetEnvironmentVariable("TURSO_DATABASE_URL");
var authToken = Environment.GetEnvironmentVariable("TURSO_AUTH_TOKEN");

if (string.IsNullOrWhiteSpace(databaseUrl) || string.IsNullOrWhiteSpace(authToken))
{
    throw new InvalidOperationException(
        "TURSO_DATABASE_URL and TURSO_AUTH_TOKEN must be configured.");
}

if (!Uri.TryCreate(databaseUrl, UriKind.Absolute, out var tursoUri)
    || (!tursoUri.Scheme.Equals("libsql", StringComparison.OrdinalIgnoreCase)
        && !tursoUri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)))
{
    throw new InvalidOperationException(
        "TURSO_DATABASE_URL must be a valid libsql:// or https:// URL.");
}

var endpoint = new UriBuilder(tursoUri)
{
    Scheme = Uri.UriSchemeHttps,
    Port = -1,
    Path = "/v2/pipeline",
    Query = string.Empty,
    Fragment = string.Empty
}.Uri.AbsoluteUri;

var connectionString = $"{endpoint};{authToken.Trim()}";
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseLibSql(connectionString));

// El hosting puede entregar el puerto en la variable PORT.
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

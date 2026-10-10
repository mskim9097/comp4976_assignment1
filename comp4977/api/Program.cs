using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MunicipalElections.Api.Data;
using MunicipalElections.Api.Extensions;
using MunicipalElections.Api.Models;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// SQLite creates the database file but not its folder (e.g. /home/data on Azure App Service).
var databaseFolder = Path.GetDirectoryName(Path.GetFullPath(new SqliteConnectionStringBuilder(connectionString).DataSource));
if (!string.IsNullOrEmpty(databaseFolder))
{
    Directory.CreateDirectory(databaseFolder);
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        // Matches the minimum password length checked on the iOS registration screen.
        options.Password.RequiredLength = 6;
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>();

var jwt = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();
if (jwt.Key.Length < 32)
{
    throw new InvalidOperationException(
        "Jwt:Key must be set to a random string of at least 32 characters (appsettings.Development.json or the Jwt__Key environment variable).");
}
builder.Services.AddSingleton(jwt);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddApiDocumentation();

var app = builder.Build();

// Create/upgrade the SQLite database and load sample data on startup (also runs on Azure).
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.MigrateAsync();
    await SeedData.InitializeAsync(context);
}

// API documentation (also on Azure so the deployed API can be tried from a browser).
app.MapOpenApi();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "Municipal Elections API v1");
    options.RoutePrefix = "swagger";
    options.EnablePersistAuthorization();
});
// The site root and /api open the documentation, so the base URL is enough to try the API.
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
app.MapGet("/api", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

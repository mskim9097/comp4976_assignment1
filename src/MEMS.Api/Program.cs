using System.Text;
using MEMS.Api.Auth;
using MEMS.Api.Errors;
using MEMS.Api.Extensions;
using MEMS.Api.Tenancy;
using MEMS.Data;
using MEMS.Data.Entities;
using MEMS.Data.Seed;
using MEMS.Data.Services;
using MEMS.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

// Database: the connection string "memsdb" comes from the Aspire AppHost.
builder.Services.AddMemsServices(builder.Configuration);
// Not a pooled context: it takes the scoped TenantContext. Aspire adds health checks, telemetry and retries.
builder.Services.AddDbContext<MemsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("memsdb")));
builder.EnrichSqlServerDbContext<MemsDbContext>();
builder.Services.AddMemsIdentity();

// JWT bearer authentication for administrators and the mobile app.
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();
if (jwt.Key.Length < 32)
{
    throw new InvalidOperationException(
        "Jwt:Key must be set to a random string of at least 32 characters (the AppHost passes it in development; use the Jwt__Key setting elsewhere).");
}
builder.Services.AddSingleton(jwt);
builder.Services.AddScoped<JwtTokenService>();
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
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = ClaimNames.Name,
            RoleClaimType = ClaimNames.Role
        };
    });
builder.Services.AddAuthorization();

// The public Blazor site runs in the browser, so the Api has to allow its address.
builder.Services.AddCors(options => options.AddPolicy("PublicSite", policy =>
    policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
        .WithMethods("GET")
        .AllowAnyHeader()));

builder.Services.AddScoped<TenantScopeFilter>();
builder.Services.AddControllers(options => options.Filters.AddService<TenantScopeFilter>());
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddApiDocumentation();

var app = builder.Build();

// Create/update the database and load the sample data (the other projects wait for the Api in the AppHost).
using (var scope = app.Services.CreateScope())
{
    await MemsDbSeeder.InitializeAsync(scope.ServiceProvider);
}

app.UseExceptionHandler();
app.UseStatusCodePages();
app.MapDefaultEndpoints();

app.UseHttpsRedirection();

// Seed images ship with the Api (wwwroot/images); photos and logos uploaded later are in the Uploads folder.
// Both are served under /images.
app.UseStaticFiles();
var uploadsRoot = Path.GetFullPath(app.Services.GetRequiredService<ImageFileStore>().RootPath);
Directory.CreateDirectory(uploadsRoot);
app.UseStaticFiles(new StaticFileOptions { FileProvider = new PhysicalFileProvider(uploadsRoot) });

app.UseCors("PublicSite");
app.UseAuthentication();
app.UseAuthorization();

app.MapOpenApi();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "MEMS API v1");
    options.RoutePrefix = "swagger";
    options.EnablePersistAuthorization();
});
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
app.MapGet("/api", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.MapControllers();

app.Run();

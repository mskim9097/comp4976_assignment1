using System.Text;
using MEMS.Api.Auth;
using MEMS.Api.Infrastructure;
using MEMS.Data;
using MEMS.Data.Entities;
using MEMS.Data.Seed;
using MEMS.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Aspire: service discovery, resilient HttpClients, OpenTelemetry (logs/traces/metrics) and health checks.
builder.AddServiceDefaults();

// The connection string "memsdb" is supplied by Aspire (AppHost: sql.AddDatabase("memsdb")). The integration also adds
// a database health check and EF Core tracing.
builder.AddSqlServerDbContext<MemsDbContext>("memsdb");

builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<MemsDbContext>();

builder.Services.AddMemsServices();

// ---- JWT bearer authentication for administrator endpoints ----
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();
if (jwt.Key.Length < 32)
{
    throw new InvalidOperationException(
        "Jwt:Key must be at least 32 characters. Run the solution through MEMS.AppHost (it supplies the 'jwt-key' parameter) or set Jwt__Key.");
}
builder.Services.AddSingleton(jwt);
builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, ValidIssuer = jwt.Issuer,
            ValidateAudience = true, ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = JwtTokenService.NameClaim,
            RoleClaimType = JwtTokenService.RoleClaim
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(MemsPolicies.SuperAdmin, p => p.RequireRole(MemsRoles.SuperAdmin))
    .AddPolicy(MemsPolicies.AnyAdmin, p => p.RequireRole(MemsRoles.SuperAdmin, MemsRoles.MunicipalityAdmin));

// ---- CORS: the Blazor WebAssembly public site runs in the browser on another origin ----
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddPolicy("PublicSite", policy =>
    policy.WithOrigins(allowedOrigins).AllowAnyHeader().WithMethods("GET", "OPTIONS")));

builder.Services.AddScoped<TenantResolutionFilter>();
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddApiDocumentation();

var app = builder.Build();

// Migrate + seed (municipalities, positions, candidates, roles, users) before accepting traffic.
await MemsDbInitializer.InitializeAsync(app.Services);

app.UseExceptionHandler();
app.UseStatusCodePages();
// Redirecting to https only outside Development: locally the sites also work over plain http (see docs/readme.txt).
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseCors("PublicSite");
app.UseAuthentication();
app.UseAuthorization();

// API documentation: Swagger UI at /swagger, OpenAPI document at /openapi/v1.json
app.MapOpenApi();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "MEMS API v1");
    options.RoutePrefix = "swagger";
    options.EnablePersistAuthorization();
});
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.MapControllers();
app.MapDefaultEndpoints();

app.Run();

using System.Security.Claims;
using MEMS.Admin.Components;
using MEMS.Admin.Services;
using MEMS.Data;
using MEMS.Data.Entities;
using MEMS.Data.Services;
using MEMS.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Aspire: service discovery, resilience, OpenTelemetry, health checks.
builder.AddServiceDefaults();

// Connection string "memsdb" comes from Aspire (AppHost). The Admin app talks to SQL Server directly through EF Core.
builder.AddSqlServerDbContext<MemsDbContext>("memsdb");

builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddCascadingAuthenticationState();

// ---- ASP.NET Core Identity (cookie sign-in) ----
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<MemsDbContext>()
    .AddClaimsPrincipalFactory<MemsClaimsPrincipalFactory>() // puts the administrator's tenant into the cookie
    .AddSignInManager();

builder.Services.ConfigureApplicationCookie(options =>
{
    // Paths are relative to the /admin path base.
    options.LoginPath = "/login";
    options.AccessDeniedPath = "/access-denied";
    options.Cookie.Name = "MEMS.Admin";
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(MemsPolicies.SuperAdmin, p => p.RequireRole(MemsRoles.SuperAdmin))
    .AddPolicy(MemsPolicies.AnyAdmin, p => p.RequireRole(MemsRoles.SuperAdmin, MemsRoles.MunicipalityAdmin));

// Shared service layer (tenant rules live here, not in the pages).
builder.Services.AddMemsServices();
builder.Services.AddScoped<AdminContext>();

// Typed client for the API using Aspire service discovery: "https+http://api" is resolved by the AppHost reference.
builder.Services.AddHttpClient<ApiStatusClient>(client => client.BaseAddress = new Uri("https+http://api"));

var app = builder.Build();

// The administration site is served under /admin (https://localhost:7103/admin).
app.UsePathBase("/admin");
// Sign in / sign out run as middleware before endpoint routing (see AuthEndpoints).
app.UseWhen(
    ctx => ctx.Request.Path.Equals(AuthEndpoints.LoginPath, StringComparison.OrdinalIgnoreCase)
        || ctx.Request.Path.Equals(AuthEndpoints.LogoutPath, StringComparison.OrdinalIgnoreCase),
    branch => branch.Run(AuthEndpoints.HandleAsync));

// Endpoint matching must happen after the path base is removed, so routing is added explicitly right here.
app.UseRouting();

// Diagnostics: a 405 means the path exists but not for that HTTP method. Log exactly what was requested.
app.Use(async (context, next) =>
{
    await next();
    if (context.Response.StatusCode == StatusCodes.Status405MethodNotAllowed)
    {
        app.Logger.LogWarning("405 Method Not Allowed: {Method} {PathBase}{Path}",
            context.Request.Method, context.Request.PathBase, context.Request.Path);
    }
});

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    // Global exception handling: users get a friendly page, details only go to the log.
    app.UseExceptionHandler("/error", createScopeForErrors: true);
    app.UseHsts();
}

// Redirecting to https only outside Development: locally the sites also work over plain http (see docs/readme.txt).
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

// Images are stored in SQL Server; these endpoints stream them to the admin pages, enforcing the tenant boundary.
var media = app.MapGroup("/media").RequireAuthorization(MemsPolicies.AnyAdmin);
media.MapGet("/candidates/{municipalityId:int}/{id:int}", async (
    int municipalityId, int id, ClaimsPrincipal user, CandidateService candidates, HttpContext http, CancellationToken ct) =>
{
    if (!TenantScope.FromPrincipal(user).CanManage(municipalityId)) return Results.NotFound();
    var photo = await candidates.GetPhotoAsync(municipalityId, id, ct);
    if (photo is null) return Results.NotFound();
    http.Response.Headers.ContentSecurityPolicy = "default-src 'none'; style-src 'unsafe-inline'; sandbox";
    return Results.File(photo.Value.Data, photo.Value.ContentType);
});
media.MapGet("/municipalities/{id:int}", async (
    int id, ClaimsPrincipal user, MunicipalityService municipalities, HttpContext http, CancellationToken ct) =>
{
    if (!TenantScope.FromPrincipal(user).CanManage(id)) return Results.NotFound();
    var logo = await municipalities.GetLogoAsync(id, ct);
    if (logo is null) return Results.NotFound();
    http.Response.Headers.ContentSecurityPolicy = "default-src 'none'; style-src 'unsafe-inline'; sandbox";
    return Results.File(logo.Value.Data, logo.Value.ContentType);
});

app.MapDefaultEndpoints();

app.Run();

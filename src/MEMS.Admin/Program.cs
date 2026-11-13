using MEMS.Admin.Components;
using MEMS.Admin.Services;
using MEMS.Data;
using MEMS.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddCascadingAuthenticationState();

// Database: the connection string "memsdb" comes from the Aspire AppHost. The Api creates and seeds it.
builder.Services.AddMemsServices(builder.Configuration);
builder.Services.AddDbContext<MemsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("memsdb")));
builder.EnrichSqlServerDbContext<MemsDbContext>();

// ASP.NET Core Identity with a login cookie. The cookie carries the roles and the tenant of the administrator.
builder.Services.AddMemsIdentity()
    .AddSignInManager()
    .AddClaimsPrincipalFactory<TenantClaimsPrincipalFactory>();
builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme).AddIdentityCookies();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = ".MEMS.Admin";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.LoginPath = "/login";
    options.AccessDeniedPath = "/access-denied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});
builder.Services.AddAuthorization();

builder.Services.AddScoped<AdminSession>();
builder.Services.AddSingleton<ImageUrls>();

var app = builder.Build();

app.MapDefaultEndpoints();

// The Admin app lives under /admin (https://localhost:port/admin).
app.Use((context, next) =>
{
    if (context.Request.Path == "/")
    {
        context.Response.Redirect("/admin/");
        return Task.CompletedTask;
    }
    return next(context);
});
app.UsePathBase("/admin");
// Routing must come after UsePathBase (WebApplication would otherwise add it at the very start of the pipeline).
app.UseRouting();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

// The form in the top bar posts here (with an antiforgery token because it binds a form field).
app.MapPost("/logout", async (SignInManager<ApplicationUser> signInManager, [FromForm] string? returnUrl) =>
{
    await signInManager.SignOutAsync();
    return Results.LocalRedirect("~/login");
});

app.Run();

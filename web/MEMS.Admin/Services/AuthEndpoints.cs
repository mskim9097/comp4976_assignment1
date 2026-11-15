using MEMS.Data.Entities;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;

namespace MEMS.Admin.Services;

// Sign in / sign out as plain middleware (mapped in Program.cs BEFORE endpoint routing), so no routing or Blazor
// form-handling rule can interfere with writing the sign-in cookie.
public static class AuthEndpoints
{
    public const string LoginPath = "/account/login";
    public const string LogoutPath = "/account/logout";

    public static async Task HandleAsync(HttpContext context)
    {
        var isLogin = context.Request.Path.Equals(LoginPath, StringComparison.OrdinalIgnoreCase);

        // A refresh or bookmark of these addresses just goes back to the login form.
        if (!HttpMethods.IsPost(context.Request.Method))
        {
            Redirect(context, "login");
            return;
        }

        try
        {
            await context.RequestServices.GetRequiredService<IAntiforgery>().ValidateRequestAsync(context);
        }
        catch (AntiforgeryValidationException)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("The form expired. Go back, reload the page and try again.");
            return;
        }

        var signInManager = context.RequestServices.GetRequiredService<SignInManager<ApplicationUser>>();
        var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(AuthEndpoints));

        if (!isLogin)
        {
            await signInManager.SignOutAsync();
            Redirect(context, "login");
            return;
        }

        var form = await context.Request.ReadFormAsync();
        var userName = form["userName"].ToString().Trim();
        var password = form["password"].ToString();
        var rememberMe = form["rememberMe"].ToString() == "true";

        // The return target must be a local, base-relative address (prevents open redirects).
        var returnUrl = form["returnUrl"].ToString();
        var safeReturn = returnUrl.Length > 0 && !returnUrl.StartsWith('/') && !returnUrl.StartsWith('\\') && !returnUrl.Contains("://")
            ? returnUrl : string.Empty;
        string Back(string error) => $"login?error={error}&returnUrl={Uri.EscapeDataString(safeReturn)}";

        if (userName.Length == 0 || password.Length == 0)
        {
            Redirect(context, Back("missing"));
            return;
        }

        var result = await signInManager.PasswordSignInAsync(userName, password, rememberMe, lockoutOnFailure: true);
        if (result.Succeeded)
        {
            logger.LogInformation("Administrator {UserName} signed in", userName);
            Redirect(context, safeReturn);
            return;
        }

        logger.LogWarning("Failed sign-in for {UserName} (locked out: {LockedOut})", userName, result.IsLockedOut);
        Redirect(context, Back(result.IsLockedOut ? "locked" : "invalid"));
    }

    // target is relative to the /admin path base ("" = dashboard).
    private static void Redirect(HttpContext context, string target) =>
        context.Response.Redirect($"{context.Request.PathBase}/{target}");
}

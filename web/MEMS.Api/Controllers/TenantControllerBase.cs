using MEMS.Api.Infrastructure;
using MEMS.Data;
using Microsoft.AspNetCore.Mvc;

namespace MEMS.Api.Controllers;

// Base class for every /api/{tenant}/... controller. The filter resolves and authorizes the tenant before any action runs.
[ApiController]
[ServiceFilter(typeof(TenantResolutionFilter))]
public abstract class TenantControllerBase : ControllerBase
{
    protected TenantInfo Tenant => (TenantInfo)HttpContext.Items[TenantInfo.ItemKey]!;
    protected TenantScope Scope => User.ToScope();

    protected static void SetImageHeaders(HttpResponse response)
    {
        response.Headers.CacheControl = "public,max-age=86400";
        response.Headers["X-Content-Type-Options"] = "nosniff";
        // Seed images are SVG: forbid scripts if one is ever opened directly.
        response.Headers.ContentSecurityPolicy = "default-src 'none'; style-src 'unsafe-inline'; sandbox";
    }
}

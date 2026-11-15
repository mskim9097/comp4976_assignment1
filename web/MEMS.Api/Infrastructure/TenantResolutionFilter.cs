using MEMS.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace MEMS.Api.Infrastructure;

// Runs before every /api/{tenant}/... action:
//   1. resolves the tenant from the URL segment (unknown tenant -> 404),
//   2. refuses a signed-in Municipality Admin who asks for another tenant (-> 403).
// The services repeat the same checks, so isolation never depends on the filter alone.
public sealed class TenantResolutionFilter(MemsDbContext db, ILogger<TenantResolutionFilter> logger) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.RouteData.Values["tenant"] is not string slug)
        {
            context.Result = new NotFoundResult();
            return;
        }

        var normalized = slug.ToLowerInvariant();
        var tenant = await db.Municipalities.AsNoTracking()
            .Where(m => m.Slug == normalized)
            .Select(m => new TenantInfo(m.MunicipalityId, m.Slug, m.Name))
            .FirstOrDefaultAsync(context.HttpContext.RequestAborted);

        if (tenant is null)
        {
            context.Result = new ObjectResult(new ProblemDetails { Status = 404, Title = "Unknown municipality", Detail = $"There is no municipality '{normalized}'." })
            { StatusCode = StatusCodes.Status404NotFound };
            return;
        }

        var scope = context.HttpContext.User.ToScope();
        if (!scope.CanSee(tenant.MunicipalityId))
        {
            logger.LogWarning("Tenant violation blocked: user {User} (tenant {UserTenant}) requested tenant {RequestedTenant}",
                context.HttpContext.User.Identity?.Name, scope.MunicipalityId, tenant.MunicipalityId);
            context.Result = new ObjectResult(new ProblemDetails { Status = 403, Title = "Forbidden", Detail = "You do not have access to this municipality." })
            { StatusCode = StatusCodes.Status403Forbidden };
            return;
        }

        context.HttpContext.Items[TenantInfo.ItemKey] = tenant;
        await next();
    }
}

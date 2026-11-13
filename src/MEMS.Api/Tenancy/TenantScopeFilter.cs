using System.Security.Claims;
using MEMS.Api.Auth;
using MEMS.Data;
using MEMS.Data.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MEMS.Api.Tenancy;

// Tenant isolation in the API layer. It runs before every action:
//  1. A Municipality Admin is always restricted to the tenant in their token.
//  2. An endpoint marked [TenantScoped] names a tenant in the URL or in ?city=. An unknown tenant is 404,
//     and an administrator of another municipality gets 403 (and a warning in the log).
//  3. The scope is then restricted to that tenant, so every EF query below is filtered as well.
public class TenantScopeFilter(
    TenantContext tenant, MunicipalityService municipalities, ILogger<TenantScopeFilter> logger) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var user = context.HttpContext.User;
        int? ownTenant = int.TryParse(user.FindFirstValue(ClaimNames.MunicipalityId), out var id) ? id : null;
        if (ownTenant is int own)
        {
            tenant.Restrict(own);
        }

        if (context.ActionDescriptor.EndpointMetadata.OfType<TenantScopedAttribute>().Any())
        {
            var named = context.RouteData.Values["slug"] as string;
            if (string.IsNullOrWhiteSpace(named) && context.HttpContext.Request.Query.TryGetValue("city", out var city))
            {
                named = city.ToString();
            }

            if (string.IsNullOrWhiteSpace(named))
            {
                context.Result = Problem(StatusCodes.Status400BadRequest, "Specify the city, for example ?city=Burnaby.");
                return;
            }

            var municipality = await municipalities.ResolveAsync(named, context.HttpContext.RequestAborted);
            if (municipality is null)
            {
                context.Result = Problem(StatusCodes.Status404NotFound, "Municipality not found.");
                return;
            }

            if (ownTenant is int mine && mine != municipality.MunicipalityId)
            {
                logger.LogWarning(
                    "Tenant violation: user {UserId} of municipality {OwnTenant} asked for {RequestedTenant} ({Path})",
                    user.FindFirstValue("sub"), mine, municipality.Slug, context.HttpContext.Request.Path);
                context.Result = Problem(StatusCodes.Status403Forbidden, "You can only access your own municipality.");
                return;
            }

            tenant.Restrict(municipality.MunicipalityId);
        }

        await next();
    }

    private static ObjectResult Problem(int status, string title) =>
        new(new ProblemDetails { Status = status, Title = title }) { StatusCode = status, ContentTypes = { "application/problem+json" } };
}

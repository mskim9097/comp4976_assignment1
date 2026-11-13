using System.Security.Claims;
using MEMS.Data;
using MEMS.Data.Services;
using MEMS.Shared;
using Microsoft.AspNetCore.Components.Authorization;

namespace MEMS.Admin.Services;

public record AdminServices(
    MunicipalityService Municipalities,
    PositionService Positions,
    CandidateService Candidates,
    DashboardService Dashboard,
    AdminUserService Users);

public record AdminUser(string Name, bool IsSuperAdmin, int? MunicipalityId);

// Tenant isolation in the Blazor app. Every data operation of a page runs in its own DI scope with its own
// DbContext (a Blazor circuit lives for a long time) and, for a Municipality Admin, a TenantContext that is
// restricted to the tenant in their login cookie. The page cannot choose another tenant.
public class AdminSession(AuthenticationStateProvider authentication, IServiceScopeFactory scopes, ILogger<AdminSession> logger)
{
    public async Task<AdminUser> GetUserAsync()
    {
        var principal = (await authentication.GetAuthenticationStateAsync()).User;
        var tenant = int.TryParse(principal.FindFirstValue(ClaimNames.MunicipalityId), out var id) ? id : (int?)null;
        return new AdminUser(
            principal.Identity?.Name ?? string.Empty,
            principal.IsInRole(Roles.SuperAdmin),
            tenant);
    }

    public async Task<T> RunAsync<T>(Func<AdminServices, Task<T>> action)
    {
        var user = await GetUserAsync();
        if (!user.IsSuperAdmin && user.MunicipalityId is null)
        {
            throw new ForbiddenException("Your account is not assigned to a municipality.");
        }

        await using var scope = scopes.CreateAsyncScope();
        var provider = scope.ServiceProvider;
        if (user.MunicipalityId is int tenantId && !user.IsSuperAdmin)
        {
            provider.GetRequiredService<TenantContext>().Restrict(tenantId);
        }

        return await action(new AdminServices(
            provider.GetRequiredService<MunicipalityService>(),
            provider.GetRequiredService<PositionService>(),
            provider.GetRequiredService<CandidateService>(),
            provider.GetRequiredService<DashboardService>(),
            provider.GetRequiredService<AdminUserService>()));
    }

    public Task RunAsync(Func<AdminServices, Task> action) =>
        RunAsync<object?>(async services =>
        {
            await action(services);
            return null;
        });

    // Runs an operation and returns the message to show on the page: the text of a known problem
    // (not found, conflict, rule) or a general message. Details of unexpected errors only go to the log.
    public async Task<string?> TryAsync(Func<Task> action)
    {
        try
        {
            await action();
            return null;
        }
        catch (Exception ex) when (ex is NotFoundException or ConflictException or ForbiddenException or RuleViolationException)
        {
            return ex.Message;
        }
        catch (IOException ex)
        {
            logger.LogInformation(ex, "File could not be read");
            return "The file could not be read. Images must be 5 MB or smaller.";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Admin action failed");
            return "Something went wrong. Please try again.";
        }
    }
}

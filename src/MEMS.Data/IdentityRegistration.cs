using MEMS.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace MEMS.Data;

public static class IdentityRegistration
{
    // The same users, roles and password rules for the Api (JWT) and the Admin app (cookie).
    public static IdentityBuilder AddMemsIdentity(this IServiceCollection services) =>
        services.AddIdentityCore<ApplicationUser>(options =>
            {
                // Matches the minimum password length checked on the iOS registration screen.
                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<MemsDbContext>();
}

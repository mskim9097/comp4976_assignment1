using MEMS.Data.Entities;
using MEMS.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MEMS.Data.Seed;

// Creates/upgrades the database and seeds it, so the application can be tested immediately after the first run.
// Only the API calls this (Aspire starts the other projects after the API is healthy).
public static class MemsDbInitializer
{
    private const string SeedPassword = "P@$$w0rd";
    private static readonly DateOnly ElectionDate = new(2026, 10, 17);

    public static async Task InitializeAsync(IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("MemsDbInitializer");
        var db = scope.ServiceProvider.GetRequiredService<MemsDbContext>();

        // The SQL Server container may still be starting: the execution strategy retries transient connection failures.
        var strategy = db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            if (db.Database.GetMigrations().Any())
            {
                await db.Database.MigrateAsync(ct);
            }
            else
            {
                // No migration has been generated yet. Run:  dotnet ef migrations add InitialCreate --project MEMS.Data --startup-project MEMS.Api
                logger.LogWarning("No EF Core migrations found: creating the schema with EnsureCreated. See docs/readme.txt to add the InitialCreate migration.");
                await db.Database.EnsureCreatedAsync(ct);
            }
        });

        await SeedDomainAsync(db, logger, ct);
        await SeedIdentityAsync(db, scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>(),
            scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>(), logger, ct);
    }

    private static async Task SeedIdentityAsync(
        MemsDbContext db, UserManager<ApplicationUser> users, RoleManager<IdentityRole> roles, ILogger logger, CancellationToken ct)
    {
        foreach (var role in new[] { MemsRoles.SuperAdmin, MemsRoles.MunicipalityAdmin })
        {
            if (!await roles.RoleExistsAsync(role)) await roles.CreateAsync(new IdentityRole(role));
        }

        await EnsureUserAsync(users, "super", "super@example.com", "Super Admin", MemsRoles.SuperAdmin, null, logger);

        // One administrator per seeded tenant. "pm" is the Pitt Meadows administrator from Assignment 1.
        var tenantIds = await db.Municipalities.AsNoTracking().ToDictionaryAsync(m => m.Slug, m => m.MunicipalityId, ct);
        await EnsureTenantAdminAsync(users, tenantIds, "pitt-meadows", "pm", "Pitt Meadows Admin", logger);
        await EnsureTenantAdminAsync(users, tenantIds, "burnaby", "burnabyadmin", "Burnaby Admin", logger);
        await EnsureTenantAdminAsync(users, tenantIds, "surrey", "surreyadmin", "Surrey Admin", logger);
        await EnsureTenantAdminAsync(users, tenantIds, "richmond", "richmondadmin", "Richmond Admin", logger);
    }

    private static Task EnsureTenantAdminAsync(
        UserManager<ApplicationUser> users, Dictionary<string, int> tenantIds, string slug, string userName, string displayName, ILogger logger) =>
        tenantIds.TryGetValue(slug, out var tenantId)
            ? EnsureUserAsync(users, userName, $"{userName}@example.com", displayName, MemsRoles.MunicipalityAdmin, tenantId, logger)
            : Task.CompletedTask;

    private static async Task EnsureUserAsync(
        UserManager<ApplicationUser> users, string userName, string email, string displayName, string role, int? municipalityId, ILogger logger)
    {
        var user = await users.FindByNameAsync(userName);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = userName, Email = email, EmailConfirmed = true,
                DisplayName = displayName, MunicipalityId = municipalityId
            };
            var created = await users.CreateAsync(user, SeedPassword);
            if (!created.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Could not create seed user '{userName}': {string.Join("; ", created.Errors.Select(e => e.Description))}");
            }
            logger.LogInformation("Seeded user {UserName} with role {Role}", userName, role);
        }

        if (!await users.IsInRoleAsync(user, role)) await users.AddToRoleAsync(user, role);
    }

    private static async Task SeedDomainAsync(MemsDbContext db, ILogger logger, CancellationToken ct)
    {
        if (await db.Municipalities.AnyAsync(ct)) return;

        var pittMeadows = Tenant("Pitt Meadows", "pitt-meadows", "#198754",
            "A small city on the Fraser River known for farmland, trails and the Pitt River.");
        var burnaby = Tenant("Burnaby", "burnaby", "#d9a400",
            "A large city in Metro Vancouver with several town centres and Burnaby Mountain.");
        var surrey = Tenant("Surrey", "surrey", "#0aa2c0",
            "One of the fastest growing cities in British Columbia, made up of six distinct town centres.");
        var richmond = Tenant("Richmond", "richmond", "#3d8bfd",
            "An island city at the mouth of the Fraser River with a busy waterfront and a vibrant food scene.");

        // Positions and fictional candidates (name, profile) for every tenant.
        AddTenantData(pittMeadows, mayor: 1, councillors: 6, trustees: 2,
            [("Alana Whitfield", "Local business owner and former parks board member focused on keeping taxes steady while protecting farmland."),
             ("Marcus Delacroix", "Retired firefighter running on public safety, flood protection and responsible growth.")],
            [("Priya Nandakumar", "Software developer and community volunteer who wants better transit links and a modern city website."),
             ("Thomas Eriksen", "Small farm operator advocating for agricultural support and trail maintenance."),
             ("Janelle Okafor", "Nurse and mother of three focused on recreation programs and healthy neighbourhoods."),
             ("Gordon Lyall", "Long-time resident and engineer prioritizing infrastructure renewal and clear budgets.")],
            [("Sofia Marchetti", "Elementary teacher who supports smaller class sizes and stronger arts programs."),
             ("Daniel Cho", "Parent advisory council chair concerned with school safety and student mental health.")]);

        AddTenantData(burnaby, mayor: 1, councillors: 8, trustees: 7,
            [("Victor Salazar", "Union representative and longtime councillor focused on affordable housing and tenant protection."),
             ("Mei-Ling Tran", "Community health director running on safer streets, transit and local small business.")],
            [("Arjun Bhatia", "Urban designer who supports walkable town centres and protected tree canopy."),
             ("Hannah Greenwood", "Environmental lawyer advocating for climate action and clean waterways."),
             ("Priya Raman", "Transit planner focused on frequent bus service and safe cycling routes."),
             ("Lucas Ferreira", "Restaurant owner who wants faster permits for small business.")],
            [("Oliver Fraser", "High school counsellor focused on student well-being and trades training."),
             ("Naomi Cheung", "Parent and librarian supporting school libraries and reading programs.")]);

        AddTenantData(surrey, mayor: 1, councillors: 8, trustees: 7,
            [("Harpreet Gill", "Former city engineer promising predictable growth and faster infrastructure delivery."),
             ("Danielle Roy", "Small business advocate focused on town centre revitalization and local jobs.")],
            [("Imran Siddiqui", "Public safety volunteer who wants more community policing and street lighting."),
             ("Jasmine Kaur", "Youth sports coach advocating for new recreation facilities."),
             ("Trevor Mackenzie", "Farmer and agricultural land reserve defender."),
             ("Elena Petrova", "Transportation analyst focused on rapid transit and congestion relief.")],
            [("Samuel Adeyemi", "Teacher focused on portable classroom replacement and student supports."),
             ("Grace Lindqvist", "Parent advisory council member supporting inclusive education.")]);

        AddTenantData(richmond, mayor: 1, councillors: 8, trustees: 7,
            [("Wei Zhang", "Port logistics manager focused on flood protection and a stronger local economy."),
             ("Catherine Doyle", "Community organizer promoting housing choice and heritage preservation.")],
            [("Kenji Watanabe", "Architect who supports waterfront trails and sustainable building."),
             ("Fatima Haddad", "Settlement worker focused on newcomer services and multicultural events."),
             ("Marcus Reid", "Paramedic advocating for emergency response times and public safety."),
             ("Linda Chow", "Accountant committed to transparent budgeting and lower fees.")],
            [("Peter Novak", "Principal focused on literacy and trades training."),
             ("Aisha Rahman", "Student mental health advocate and parent volunteer.")]);

        db.Municipalities.AddRange(pittMeadows, burnaby, surrey, richmond);
        await db.SaveChangesAsync(ct);

        var candidates = await db.Candidates.CountAsync(ct);
        logger.LogInformation("Seeded 4 municipalities and {Candidates} candidates", candidates);
    }

    private static Municipality Tenant(string name, string slug, string color, string description) => new()
    {
        Name = name, Slug = slug, Province = "British Columbia", ElectionDate = ElectionDate, Description = description,
        ThemeColor = color, LogoData = SeedImages.Logo(name, color), LogoContentType = SeedImages.SvgContentType, LogoVersion = 1
    };

    private static void AddTenantData(
        Municipality tenant, int mayor, int councillors, int trustees,
        (string Name, string Profile)[] mayors, (string Name, string Profile)[] council, (string Name, string Profile)[] school)
    {
        var mayorPosition = new Position { Name = "Mayor", SeatsAvailable = mayor, Municipality = tenant };
        var councilPosition = new Position { Name = "Councillor", SeatsAvailable = councillors, Municipality = tenant };
        var schoolPosition = new Position { Name = "School Trustee", SeatsAvailable = trustees, Municipality = tenant };
        tenant.Positions.Add(mayorPosition);
        tenant.Positions.Add(councilPosition);
        tenant.Positions.Add(schoolPosition);

        void Add(Position position, (string Name, string Profile) c)
        {
            var slug = c.Name.ToLowerInvariant().Replace(' ', '.').Replace("-", string.Empty);
            var candidate = new Candidate
            {
                Name = c.Name, Profile = c.Profile, Municipality = tenant, Position = position,
                Website = $"https://example.com/{slug}", Email = $"{slug}@example.com",
                PhotoData = SeedImages.Avatar(c.Name), PhotoContentType = SeedImages.SvgContentType, PhotoVersion = 1,
                CreatedUtc = DateTime.UtcNow
            };
            tenant.Candidates.Add(candidate);
        }

        foreach (var c in mayors) Add(mayorPosition, c);
        foreach (var c in council) Add(councilPosition, c);
        foreach (var c in school) Add(schoolPosition, c);
    }
}

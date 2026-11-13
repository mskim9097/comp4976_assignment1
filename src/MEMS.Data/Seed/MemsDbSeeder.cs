using MEMS.Data.Entities;
using MEMS.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MEMS.Data.Seed;

// Creates/updates the database on start-up and loads sample data, roles and the administrator accounts.
// The image files named here are in MEMS.Api/wwwroot/images.
public static class MemsDbSeeder
{
    private const string SeedPassword = "P@$$w0rd";
    private static readonly DateOnly ElectionDate = new(2026, 10, 17);

    public static async Task InitializeAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<MemsDbContext>();
        if (context.Database.IsSqlServer())
        {
            await context.Database.MigrateAsync();
        }
        else
        {
            // SQLite (the Azure deployment): the migrations are written for SQL Server, so the schema is created from the model.
            await context.Database.EnsureCreatedAsync();
        }

        await SeedDomainAsync(context);
        await SeedIdentityAsync(
            context,
            services.GetRequiredService<UserManager<ApplicationUser>>(),
            services.GetRequiredService<RoleManager<IdentityRole>>());
    }

    private static async Task SeedIdentityAsync(
        MemsDbContext context, UserManager<ApplicationUser> users, RoleManager<IdentityRole> roles)
    {
        foreach (var roleName in new[] { Roles.SuperAdmin, Roles.MunicipalityAdmin })
        {
            if (!await roles.RoleExistsAsync(roleName))
            {
                await roles.CreateAsync(new IdentityRole(roleName));
            }
        }

        var tenants = await context.Municipalities.ToDictionaryAsync(m => m.Slug, m => m.MunicipalityId);

        await EnsureUserAsync(users, "super", "Super Admin", Roles.SuperAdmin, null);
        // "pm" is the Pitt Meadows administrator from Assignment 1.
        await EnsureUserAsync(users, "pm", "Pitt Meadows Admin", Roles.MunicipalityAdmin, tenants["pitt-meadows"]);
        await EnsureUserAsync(users, "burnabyadmin", "Burnaby Admin", Roles.MunicipalityAdmin, tenants["burnaby"]);
        await EnsureUserAsync(users, "surreyadmin", "Surrey Admin", Roles.MunicipalityAdmin, tenants["surrey"]);
        await EnsureUserAsync(users, "richmondadmin", "Richmond Admin", Roles.MunicipalityAdmin, tenants["richmond"]);
    }

    private static async Task EnsureUserAsync(
        UserManager<ApplicationUser> users, string userName, string name, string roleName, int? municipalityId)
    {
        var user = await users.FindByNameAsync(userName);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = userName,
                Email = $"{userName}@example.com",
                EmailConfirmed = true,
                Name = name,
                MunicipalityId = municipalityId
            };
            var result = await users.CreateAsync(user, SeedPassword);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Could not create user '{userName}': {string.Join("; ", result.Errors.Select(e => e.Description))}");
            }
        }

        if (!await users.IsInRoleAsync(user, roleName))
        {
            await users.AddToRoleAsync(user, roleName);
        }
    }

    private static async Task SeedDomainAsync(MemsDbContext context)
    {
        if (await context.Municipalities.AnyAsync())
        {
            return;
        }

        var burnaby = Town("Burnaby", "burnaby", "#1f4e8c", "burnaby.png",
            "A large city in Metro Vancouver with several town centres and Burnaby Mountain.",
            "Welcome to the Burnaby election guide. Meet the candidates and plan your vote.");
        var surrey = Town("Surrey", "surrey", "#6a3d9a", "surrey.png",
            "One of the fastest growing cities in British Columbia, with six town centres.",
            "Welcome to the Surrey election guide. See who is running in your community.");
        var richmond = Town("Richmond", "richmond", "#b3261e", "richmond.png",
            "An island city at the mouth of the Fraser River, home to Vancouver International Airport.",
            "Welcome to the Richmond election guide. Learn about the candidates before you vote.");
        var pittMeadows = Town("Pitt Meadows", "pitt-meadows", "#1f6f4a", "pitt-meadows.png",
            "A small city on the Fraser River known for farmland, trails and the Pitt River.",
            "Welcome to the Pitt Meadows election guide. Find out who is on the ballot.");

        var bbMayor = Pos("Mayor", 1, burnaby);
        var bbCouncillor = Pos("Councillor", 8, burnaby);
        var bbTrustee = Pos("School Trustee", 2, burnaby);
        var srMayor = Pos("Mayor", 1, surrey);
        var srCouncillor = Pos("Councillor", 8, surrey);
        var srTrustee = Pos("School Trustee", 7, surrey);
        var rmMayor = Pos("Mayor", 1, richmond);
        var rmCouncillor = Pos("Councillor", 8, richmond);
        var rmTrustee = Pos("School Trustee", 7, richmond);
        var pmMayor = Pos("Mayor", 1, pittMeadows);
        var pmCouncillor = Pos("Councillor", 5, pittMeadows);
        var pmTrustee = Pos("School Trustee", 2, pittMeadows);

        // Fictional candidates, so the sites and the iOS app can be tested immediately.
        var candidates = new List<Candidate>
        {
            Cand(1, "Alana Whitfield", pmMayor, "Local business owner and former parks board member focused on keeping taxes steady while protecting farmland."),
            Cand(2, "Marcus Delacroix", pmMayor, "Retired firefighter running on public safety, flood protection and responsible growth."),
            Cand(3, "Priya Nandakumar", pmCouncillor, "Software developer and community volunteer who wants better transit links and a modern city website."),
            Cand(4, "Thomas Eriksen", pmCouncillor, "Small farm operator advocating for agricultural support and trail maintenance."),
            Cand(5, "Janelle Okafor", pmCouncillor, "Nurse and mother of three focused on recreation programs and healthy neighbourhoods."),
            Cand(6, "Gordon Lyall", pmCouncillor, "Long-time resident and engineer prioritizing infrastructure renewal and clear budgets."),
            Cand(7, "Sofia Marchetti", pmTrustee, "Elementary teacher who supports smaller class sizes and stronger arts programs."),
            Cand(8, "Daniel Cho", pmTrustee, "Parent advisory council chair concerned with school safety and student mental health."),
            Cand(9, "Victor Salazar", bbMayor, "Union representative and longtime councillor focused on affordable housing and tenant protection."),
            Cand(10, "Mei-Ling Tran", bbMayor, "Community health director running on safer streets, transit and local small business."),
            Cand(11, "Arjun Bhatia", bbCouncillor, "Urban designer who supports walkable town centres and protected tree canopy."),
            Cand(12, "Hannah Greenwood", bbCouncillor, "Environmental lawyer advocating for climate action and clean waterways."),
            Cand(13, "Oliver Fraser", bbTrustee, "High school counsellor focused on student well-being and trades training."),
            Cand(14, "Harpreet Gill", srMayor, "Former school principal promising safer neighbourhoods, faster permits and more rec centres."),
            Cand(15, "Daniela Rossi", srMayor, "Transit planner who wants better bus service between the town centres."),
            Cand(16, "Samuel Okoye", srCouncillor, "Small business owner focused on local jobs and a strong night-time economy."),
            Cand(17, "Linh Pham", srCouncillor, "Public health nurse advocating for clinics, parks and clean drinking water."),
            Cand(18, "Gurdeep Sandhu", srCouncillor, "Civil engineer who wants road repairs done on time and on budget."),
            Cand(19, "Emily Watanabe", srTrustee, "Teacher-librarian supporting reading programs and more school counsellors."),
            Cand(20, "Wei Zhang", rmMayor, "Business consultant running on housing affordability and a vibrant waterfront."),
            Cand(21, "Catherine Boucher", rmMayor, "Retired police officer focused on community safety and flood protection."),
            Cand(22, "Nikhil Rao", rmCouncillor, "Data analyst and cyclist who wants safer streets and more bike lanes."),
            Cand(23, "Amelia Hart", rmCouncillor, "Farmer and environmentalist protecting agricultural land and wetlands."),
            Cand(24, "Jun-ho Park", rmCouncillor, "Restaurant owner supporting small business and the arts."),
            Cand(25, "Fatima Haddad", rmTrustee, "Parent and interpreter who wants more language support in schools.")
        };

        context.Municipalities.AddRange(burnaby, surrey, richmond, pittMeadows);
        context.Candidates.AddRange(candidates);
        await context.SaveChangesAsync();
    }

    private static Municipality Town(
        string name, string slug, string themeColor, string logoFileName, string description, string welcome) => new()
    {
        Name = name,
        Slug = slug,
        Province = "British Columbia",
        ElectionDate = ElectionDate,
        Description = description,
        LogoFileName = logoFileName,
        ThemeColor = themeColor,
        WelcomeMessage = welcome
    };

    private static Position Pos(string name, int seats, Municipality municipality) =>
        new() { Name = name, SeatsAvailable = seats, Municipality = municipality };

    private static Candidate Cand(int number, string name, Position position, string profile)
    {
        var slug = name.ToLowerInvariant().Replace(' ', '.').Replace("-", string.Empty);
        return new Candidate
        {
            Name = name,
            Profile = profile,
            PhotoFileName = $"c{number:00}.png",
            Website = $"https://example.com/{slug}",
            Email = $"{slug}@example.com",
            CreatedDate = DateTime.UtcNow,
            Position = position,
            Municipality = position.Municipality
        };
    }
}

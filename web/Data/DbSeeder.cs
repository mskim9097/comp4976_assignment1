using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MunicipalElections.Web.Models;

namespace MunicipalElections.Web.Data;

// Creates/updates the database on startup and loads the sample data, roles ("groups") and admin users.
// Image files named here are in wwwroot/images (see ImageUrls).
public static class DbSeeder
{
    public const string SuperAdminRole = "Super Admin";
    private const string SeedPassword = "P@$$w0rd";
    private static readonly DateTime ElectionDate = new(2026, 10, 17);

    public static async Task InitializeAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();

        await SeedDomainAsync(context);
        await SeedIdentityAsync(
            context,
            services.GetRequiredService<UserManager<ApplicationUser>>(),
            services.GetRequiredService<RoleManager<IdentityRole>>());
    }

    // Admin group name for a municipality, e.g. "Pitt Meadows Admin".
    public static string AdminRoleNameFor(string municipalityName) => $"{municipalityName} Admin";

    private static async Task SeedIdentityAsync(
        ApplicationDbContext context, UserManager<ApplicationUser> users, RoleManager<IdentityRole> roles)
    {
        var roleNames = new List<string> { SuperAdminRole };
        roleNames.AddRange(await context.Municipalities.Select(m => m.AdminRoleName).ToListAsync());
        foreach (var roleName in roleNames.Where(n => !string.IsNullOrEmpty(n)))
        {
            if (!await roles.RoleExistsAsync(roleName))
            {
                await roles.CreateAsync(new IdentityRole(roleName));
            }
        }

        await EnsureUserAsync(users, "super", "super@example.com", "Super Admin", SuperAdminRole);
        await EnsureUserAsync(users, "pm", "pm@example.com", "Pitt Meadows Admin", AdminRoleNameFor("Pitt Meadows"));
    }

    private static async Task EnsureUserAsync(
        UserManager<ApplicationUser> users, string userName, string email, string name, string roleName)
    {
        var user = await users.FindByNameAsync(userName);
        if (user is null)
        {
            user = new ApplicationUser { UserName = userName, Email = email, EmailConfirmed = true, Name = name };
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

    private static async Task SeedDomainAsync(ApplicationDbContext context)
    {
        if (await context.Municipalities.AnyAsync())
        {
            return;
        }

        var pittMeadows = Municipality("Pitt Meadows", "A small city on the Fraser River known for farmland, trails and the Pitt River.", "pitt-meadows.png");
        var mapleRidge = Municipality("Maple Ridge", "A growing city between the Fraser River and the Golden Ears mountains.", "maple-ridge.png");
        var burnaby = Municipality("Burnaby", "A large city in Metro Vancouver with several town centres and Burnaby Mountain.", "burnaby.png");

        var pmMayor = Pos("Mayor", 1, pittMeadows);
        var pmCouncillor = Pos("Councillor", 6, pittMeadows);
        var pmTrustee = Pos("School Trustee", 2, pittMeadows);
        var mrMayor = Pos("Mayor", 1, mapleRidge);
        var mrCouncillor = Pos("Councillor", 6, mapleRidge);
        var mrTrustee = Pos("School Trustee", 2, mapleRidge);
        var bbMayor = Pos("Mayor", 1, burnaby);
        var bbCouncillor = Pos("Councillor", 8, burnaby);
        var bbTrustee = Pos("School Trustee", 2, burnaby);

        // Fictional candidates, so the site and the iOS app can be tested immediately.
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
            Cand(9, "Rebecca Strand", mrMayor, "Former city planner promising a downtown revitalization and housing for young families."),
            Cand(10, "Hamid Karimi", mrMayor, "Entrepreneur and community organizer focused on jobs, transit and green space."),
            Cand(11, "Louise Beaumont", mrCouncillor, "Librarian who wants more community programming and accessible public spaces."),
            Cand(12, "Kwame Mensah", mrCouncillor, "Youth coach and accountant advocating for sport facilities and transparent spending."),
            Cand(13, "Ingrid Halvorsen", mrTrustee, "Retired principal committed to literacy support and inclusive classrooms."),
            Cand(14, "Victor Salazar", bbMayor, "Union representative and longtime councillor focused on affordable housing and tenant protection."),
            Cand(15, "Mei-Ling Tran", bbMayor, "Community health director running on safer streets, transit and local small business."),
            Cand(16, "Arjun Bhatia", bbCouncillor, "Urban designer who supports walkable town centres and protected tree canopy."),
            Cand(17, "Hannah Greenwood", bbCouncillor, "Environmental lawyer advocating for climate action and clean waterways."),
            Cand(18, "Oliver Fraser", bbTrustee, "High school counsellor focused on student well-being and trades training.")
        };

        context.Municipalities.AddRange(pittMeadows, mapleRidge, burnaby);
        context.Candidates.AddRange(candidates);
        await context.SaveChangesAsync();
    }

    private static Municipality Municipality(string name, string description, string logoFileName) => new()
    {
        Name = name,
        Province = "British Columbia",
        ElectionDate = ElectionDate,
        Description = description,
        LogoFileName = logoFileName,
        AdminRoleName = AdminRoleNameFor(name)
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
            Position = position
        };
    }
}

using MunicipalElections.Api.Models;

namespace MunicipalElections.Api.Data;

// Sample data (fictional candidates) so the API and the iOS app can be tested immediately.
// Image URLs are stored relative to the API host; controllers turn them into absolute URLs.
public static class SeedData
{
    private static readonly DateTime ElectionDate = new(2026, 10, 17);

    public static async Task InitializeAsync(ApplicationDbContext context)
    {
        if (context.Municipalities.Any())
        {
            return;
        }

        var pittMeadows = new Municipality
        {
            Name = "Pitt Meadows",
            Province = "British Columbia",
            ElectionDate = ElectionDate,
            Description = "A small city on the Fraser River known for farmland, trails and the Pitt River.",
            LogoUrl = "/images/logos/pitt-meadows.png"
        };
        var mapleRidge = new Municipality
        {
            Name = "Maple Ridge",
            Province = "British Columbia",
            ElectionDate = ElectionDate,
            Description = "A growing city between the Fraser River and the Golden Ears mountains.",
            LogoUrl = "/images/logos/maple-ridge.png"
        };
        var burnaby = new Municipality
        {
            Name = "Burnaby",
            Province = "British Columbia",
            ElectionDate = ElectionDate,
            Description = "A large city in Metro Vancouver with several town centres and Burnaby Mountain.",
            LogoUrl = "/images/logos/burnaby.png"
        };

        var pmMayor = Pos("Mayor", 1, pittMeadows);
        var pmCouncillor = Pos("Councillor", 6, pittMeadows);
        var pmTrustee = Pos("School Trustee", 2, pittMeadows);
        var mrMayor = Pos("Mayor", 1, mapleRidge);
        var mrCouncillor = Pos("Councillor", 6, mapleRidge);
        var mrTrustee = Pos("School Trustee", 2, mapleRidge);
        var bbMayor = Pos("Mayor", 1, burnaby);
        var bbCouncillor = Pos("Councillor", 8, burnaby);
        var bbTrustee = Pos("School Trustee", 2, burnaby);

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

    private static Position Pos(string name, int seats, Municipality municipality) =>
        new() { Name = name, SeatsAvailable = seats, Municipality = municipality };

    private static Candidate Cand(int number, string name, Position position, string biography)
    {
        var slug = name.ToLowerInvariant().Replace(' ', '.').Replace("-", string.Empty);
        return new Candidate
        {
            Name = name,
            Biography = biography,
            PhotoUrl = $"/images/candidates/c{number:00}.png",
            Website = $"https://example.com/{slug}",
            Email = $"{slug}@example.com",
            Position = position,
            Municipality = position.Municipality
        };
    }
}

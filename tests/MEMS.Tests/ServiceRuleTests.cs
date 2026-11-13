using MEMS.Data.Services;
using MEMS.Shared.Dtos;

namespace MEMS.Tests;

// Validation rules of the services (the same rules protect the Api and the Admin app).
public class ServiceRuleTests
{
    private static MunicipalityRequest Town(string name, string slug) => new()
    {
        Name = name, Slug = slug, Province = "British Columbia", ElectionDate = new DateOnly(2026, 10, 17), ThemeColor = "#336699"
    };

    [Theory]
    [InlineData("burnaby", "burnaby-2")]
    [InlineData("BURNABY", "burnaby-2")]
    [InlineData("  Burnaby ", "burnaby-2")]
    public async Task Municipality_names_are_unique_ignoring_case_and_spaces(string name, string slug)
    {
        using var host = new TestHost();
        var (service, _, _, db) = host.Services(null);
        await using var _ = db;
        await Assert.ThrowsAsync<ConflictException>(() => service.CreateAsync(Town(name, slug)));
    }

    [Fact]
    public async Task Municipality_slug_must_be_unique_valid_and_not_reserved()
    {
        using var host = new TestHost();
        var (service, _, _, db) = host.Services(null);
        await using var _ = db;
        await Assert.ThrowsAsync<ConflictException>(() => service.CreateAsync(Town("Other", "burnaby")));
        await Assert.ThrowsAsync<RuleViolationException>(() => service.CreateAsync(Town("Other", "Bad Slug")));
        await Assert.ThrowsAsync<RuleViolationException>(() => service.CreateAsync(Town("Other", "auth")));
        await Assert.ThrowsAsync<RuleViolationException>(() => service.CreateAsync(Town("Other", "api")));
    }

    [Fact]
    public async Task Municipality_province_must_come_from_the_list()
    {
        using var host = new TestHost();
        var (service, _, _, db) = host.Services(null);
        await using var _ = db;
        var request = Town("Other", "other");
        request.Province = "BC";
        await Assert.ThrowsAsync<RuleViolationException>(() => service.CreateAsync(request));
    }

    [Fact]
    public async Task Position_names_are_unique_inside_a_municipality_only()
    {
        using var host = new TestHost();
        var (_, positions, _, db) = host.Services(null);
        await using var _ = db;

        await Assert.ThrowsAsync<ConflictException>(() => positions.CreateAsync(host.BurnabyId, new PositionRequest { Name = "MAYOR", SeatsAvailable = 1 }));
        var councillor = await positions.CreateAsync(host.BurnabyId, new PositionRequest { Name = "Councillor", SeatsAvailable = 8 });
        Assert.Equal(host.BurnabyId, councillor.MunicipalityId);
        // The same name in another municipality is fine.
        await positions.CreateAsync(host.SurreyId, new PositionRequest { Name = "Councillor", SeatsAvailable = 8 });
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public async Task Position_seats_must_be_between_1_and_100(int seats)
    {
        using var host = new TestHost();
        var (_, positions, _, db) = host.Services(null);
        await using var _ = db;
        await Assert.ThrowsAsync<RuleViolationException>(() => positions.CreateAsync(host.BurnabyId, new PositionRequest { Name = "Trustee", SeatsAvailable = seats }));
    }

    [Fact]
    public async Task Candidate_rules()
    {
        using var host = new TestHost();
        var (_, _, candidates, db) = host.Services(host.BurnabyId);
        await using var _ = db;

        await Assert.ThrowsAsync<ConflictException>(() => candidates.CreateAsync(
            new CandidateRequest { Name = "bea burnaby", PositionId = host.BurnabyMayorId }));
        await Assert.ThrowsAsync<RuleViolationException>(() => candidates.CreateAsync(
            new CandidateRequest { Name = "  ", PositionId = host.BurnabyMayorId }));
        await Assert.ThrowsAsync<RuleViolationException>(() => candidates.CreateAsync(
            new CandidateRequest { Name = "Ftp Person", PositionId = host.BurnabyMayorId, Website = "ftp://example.com" }));

        var ok = await candidates.CreateAsync(new CandidateRequest
        {
            Name = "  Good   Person ", PositionId = host.BurnabyMayorId, Website = "https://example.com", Email = "a@b.ca", Biography = " Hello "
        });
        Assert.Equal("Good Person", ok.Name);
        Assert.Equal("Hello", ok.Profile);
    }

    [Fact]
    public async Task Candidate_search_ignores_case_and_treats_wildcards_as_text()
    {
        using var host = new TestHost();
        var (_, _, candidates, db) = host.Services(host.BurnabyId);
        await using var _ = db;

        Assert.Single(await candidates.ListAsync(host.BurnabyId, "BEA"));
        Assert.Empty(await candidates.ListAsync(host.BurnabyId, "%"));
        Assert.Empty(await candidates.ListAsync(host.BurnabyId, "zzz"));
    }
}

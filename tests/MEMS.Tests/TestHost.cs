using MEMS.Data;
using MEMS.Data.Entities;
using MEMS.Data.Services;
using Microsoft.EntityFrameworkCore;

namespace MEMS.Tests;

// A small in-memory world for the tests: two municipalities (tenants) with a position and a candidate each.
// One test = one database; every "request" gets its own TenantContext and DbContext, like the Api and the Admin app.
public sealed class TestHost : IDisposable
{
    private readonly string databaseName = Guid.NewGuid().ToString();
    private readonly string uploadFolder = Path.Combine(Path.GetTempPath(), "mems-tests-" + Guid.NewGuid().ToString("N"));

    public int BurnabyId { get; private set; }
    public int SurreyId { get; private set; }
    public int BurnabyMayorId { get; private set; }
    public int SurreyMayorId { get; private set; }
    public int BurnabyCandidateId { get; private set; }
    public int SurreyCandidateId { get; private set; }

    public ImageFileStore Images => new(uploadFolder);
    public string UploadFolder => uploadFolder;

    public TestHost()
    {
        using var db = CreateContext(new TenantContext());
        var burnaby = Town("Burnaby", "burnaby");
        var surrey = Town("Surrey", "surrey");
        var bbMayor = new Position { Name = "Mayor", SeatsAvailable = 1, Municipality = burnaby };
        var srMayor = new Position { Name = "Mayor", SeatsAvailable = 1, Municipality = surrey };
        var bbCandidate = new Candidate { Name = "Bea Burnaby", Position = bbMayor, Municipality = burnaby };
        var srCandidate = new Candidate { Name = "Sam Surrey", Position = srMayor, Municipality = surrey };
        db.AddRange(burnaby, surrey, bbMayor, srMayor, bbCandidate, srCandidate);
        db.SaveChanges();
        (BurnabyId, SurreyId) = (burnaby.MunicipalityId, surrey.MunicipalityId);
        (BurnabyMayorId, SurreyMayorId) = (bbMayor.PositionId, srMayor.PositionId);
        (BurnabyCandidateId, SurreyCandidateId) = (bbCandidate.CandidateId, srCandidate.CandidateId);
    }

    // tenantId null = not restricted (Super Admin); otherwise restricted to that municipality.
    public MemsDbContext CreateContext(TenantContext tenant)
    {
        var options = new DbContextOptionsBuilder<MemsDbContext>().UseInMemoryDatabase(databaseName).Options;
        return new MemsDbContext(options, tenant);
    }

    public TenantContext Tenant(int? municipalityId)
    {
        var tenant = new TenantContext();
        if (municipalityId is int id)
        {
            tenant.Restrict(id);
        }
        return tenant;
    }

    public (MunicipalityService Municipalities, PositionService Positions, CandidateService Candidates, MemsDbContext Db) Services(int? municipalityId)
    {
        var tenant = Tenant(municipalityId);
        var db = CreateContext(tenant);
        return (new MunicipalityService(db, tenant, Images), new PositionService(db, Images), new CandidateService(db, Images), db);
    }

    private static Municipality Town(string name, string slug) => new()
    {
        Name = name, Slug = slug, Province = "British Columbia", ElectionDate = new DateOnly(2026, 10, 17), ThemeColor = "#336699"
    };

    public void Dispose()
    {
        if (Directory.Exists(uploadFolder))
        {
            Directory.Delete(uploadFolder, true);
        }
    }
}

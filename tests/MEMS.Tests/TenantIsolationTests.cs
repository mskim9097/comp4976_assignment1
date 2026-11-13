using MEMS.Data;
using MEMS.Data.Services;
using MEMS.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace MEMS.Tests;

// Tenant isolation at the Entity Framework level and in the services.
public class TenantIsolationTests
{
    [Fact]
    public void A_scope_cannot_jump_to_another_tenant()
    {
        var tenant = new TenantContext();
        tenant.Restrict(1);
        tenant.Restrict(1);
        Assert.Throws<InvalidOperationException>(() => tenant.Restrict(2));
    }

    [Fact]
    public async Task Unrestricted_scope_sees_every_tenant()
    {
        using var host = new TestHost();
        await using var db = host.CreateContext(host.Tenant(null));
        Assert.Equal(2, await db.Municipalities.CountAsync());
        Assert.Equal(2, await db.Positions.CountAsync());
        Assert.Equal(2, await db.Candidates.CountAsync());
    }

    [Fact]
    public async Task Restricted_scope_sees_only_its_own_rows()
    {
        using var host = new TestHost();
        await using var db = host.CreateContext(host.Tenant(host.BurnabyId));
        Assert.Equal("Burnaby", Assert.Single(await db.Municipalities.ToListAsync()).Name);
        Assert.Equal(host.BurnabyMayorId, Assert.Single(await db.Positions.ToListAsync()).PositionId);
        Assert.Equal("Bea Burnaby", Assert.Single(await db.Candidates.ToListAsync()).Name);
    }

    [Fact]
    public async Task Ids_of_another_tenant_are_not_found()
    {
        using var host = new TestHost();
        await using var db = host.CreateContext(host.Tenant(host.BurnabyId));
        Assert.Null(await db.Candidates.FirstOrDefaultAsync(c => c.CandidateId == host.SurreyCandidateId));
        Assert.Null(await db.Positions.FirstOrDefaultAsync(p => p.PositionId == host.SurreyMayorId));
        Assert.Null(await db.Municipalities.FirstOrDefaultAsync(m => m.MunicipalityId == host.SurreyId));
    }

    [Fact]
    public async Task Candidate_service_reads_and_writes_only_inside_the_tenant()
    {
        using var host = new TestHost();
        var (_, _, candidates, db) = host.Services(host.BurnabyId);
        await using var _ = db;

        await Assert.ThrowsAsync<NotFoundException>(() => candidates.GetAsync(host.SurreyCandidateId));
        await Assert.ThrowsAsync<NotFoundException>(() => candidates.DeleteAsync(host.SurreyCandidateId));
        await Assert.ThrowsAsync<NotFoundException>(() => candidates.UpdateAsync(
            host.SurreyCandidateId, new CandidateRequest { Name = "Hacked", PositionId = host.BurnabyMayorId }));
        // A position of another tenant cannot be used for a new candidate either.
        await Assert.ThrowsAsync<NotFoundException>(() => candidates.CreateAsync(
            new CandidateRequest { Name = "Intruder", PositionId = host.SurreyMayorId }));
        Assert.Empty(await candidates.ListAsync(host.SurreyId));
    }

    [Fact]
    public async Task Candidate_gets_the_tenant_of_its_position_not_of_the_request()
    {
        using var host = new TestHost();
        var (_, _, candidates, db) = host.Services(host.BurnabyId);
        await using var _ = db;

        var created = await candidates.CreateAsync(new CandidateRequest { Name = "New Person", PositionId = host.BurnabyMayorId });

        Assert.Equal(host.BurnabyId, created.MunicipalityId);
    }

    [Fact]
    public async Task Position_service_is_limited_to_the_tenant()
    {
        using var host = new TestHost();
        var (_, positions, _, db) = host.Services(host.BurnabyId);
        await using var _ = db;

        await Assert.ThrowsAsync<NotFoundException>(() => positions.GetAsync(host.SurreyMayorId));
        await Assert.ThrowsAsync<NotFoundException>(() => positions.DeleteAsync(host.SurreyMayorId));
        await Assert.ThrowsAsync<NotFoundException>(() => positions.CreateAsync(
            host.SurreyId, new PositionRequest { Name = "Councillor", SeatsAvailable = 6 }));
    }

    [Fact]
    public async Task Only_an_unrestricted_scope_manages_municipalities()
    {
        using var host = new TestHost();
        var request = new MunicipalityRequest
        {
            Name = "Delta", Slug = "delta", Province = "British Columbia", ElectionDate = new DateOnly(2026, 10, 17), ThemeColor = "#000000"
        };

        var (restricted, _, _, restrictedDb) = host.Services(host.BurnabyId);
        await using (restrictedDb)
        {
            await Assert.ThrowsAsync<ForbiddenException>(() => restricted.CreateAsync(request));
            await Assert.ThrowsAsync<ForbiddenException>(() => restricted.UpdateAsync(host.BurnabyId, request));
            await Assert.ThrowsAsync<ForbiddenException>(() => restricted.DeleteAsync(host.BurnabyId));
        }

        var (super, _, _, superDb) = host.Services(null);
        await using (superDb)
        {
            var created = await super.CreateAsync(request);
            Assert.Equal("delta", created.Slug);
        }
    }

    [Fact]
    public async Task Municipality_service_resolves_a_tenant_by_slug_or_name_even_for_a_restricted_scope()
    {
        using var host = new TestHost();
        var (municipalities, _, _, db) = host.Services(host.BurnabyId);
        await using var _ = db;

        // The filtered lookup hides another tenant ...
        await Assert.ThrowsAsync<NotFoundException>(() => municipalities.GetBySlugAsync("surrey"));
        // ... the resolver used by the Api filter can still find it, so the filter can answer 403 for it.
        Assert.Equal(host.SurreyId, (await municipalities.ResolveAsync("surrey"))!.MunicipalityId);
        Assert.Equal(host.SurreyId, (await municipalities.ResolveAsync("  SURREY ")!)!.MunicipalityId);
        Assert.Null(await municipalities.ResolveAsync("atlantis"));
    }
}

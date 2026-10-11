using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MunicipalElections.Web.Models;

namespace MunicipalElections.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Municipality> Municipalities => Set<Municipality>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<Candidate> Candidates => Set<Candidate>();
    public DbSet<SavedCandidate> SavedCandidates => Set<SavedCandidate>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Municipality>().HasIndex(m => m.Name).IsUnique();

        // A municipality cannot have two positions with the same name.
        builder.Entity<Position>().HasIndex(p => new { p.MunicipalityId, p.Name }).IsUnique();

        // A user can save a candidate only once.
        builder.Entity<SavedCandidate>().HasIndex(s => new { s.UserId, s.CandidateId }).IsUnique();
    }
}

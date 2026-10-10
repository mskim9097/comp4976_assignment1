using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MunicipalElections.Api.Models;

namespace MunicipalElections.Api.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Municipality> Municipalities => Set<Municipality>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<Candidate> Candidates => Set<Candidate>();
    public DbSet<SavedCandidate> SavedCandidates => Set<SavedCandidate>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Candidate has two parents; only Position cascades so there is a single delete path.
        builder.Entity<Candidate>()
            .HasOne(c => c.Municipality)
            .WithMany(m => m.Candidates)
            .HasForeignKey(c => c.MunicipalityId)
            .OnDelete(DeleteBehavior.Restrict);

        // A user can save a candidate only once.
        builder.Entity<SavedCandidate>()
            .HasIndex(s => new { s.UserId, s.CandidateId })
            .IsUnique();
    }
}

using MEMS.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MEMS.Data;

public class MemsDbContext(DbContextOptions<MemsDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Municipality> Municipalities => Set<Municipality>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<Candidate> Candidates => Set<Candidate>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Municipality>(e =>
        {
            e.HasIndex(m => m.Name).IsUnique();
            e.HasIndex(m => m.Slug).IsUnique();
        });

        builder.Entity<Position>(e =>
        {
            e.HasOne(p => p.Municipality).WithMany(m => m.Positions)
                .HasForeignKey(p => p.MunicipalityId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(p => new { p.MunicipalityId, p.Name }).IsUnique();
            // Lets Candidate reference (PositionId, MunicipalityId) so a candidate can never sit in another tenant's position.
            e.HasAlternateKey(p => new { p.PositionId, p.MunicipalityId });
            e.ToTable(t => t.HasCheckConstraint("CK_Position_Seats", "[SeatsAvailable] BETWEEN 1 AND 100"));
        });

        builder.Entity<Candidate>(e =>
        {
            e.HasOne(c => c.Municipality).WithMany(m => m.Candidates)
                .HasForeignKey(c => c.MunicipalityId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(c => c.Position).WithMany(p => p.Candidates)
                .HasForeignKey(c => new { c.PositionId, c.MunicipalityId })
                .HasPrincipalKey(p => new { p.PositionId, p.MunicipalityId })
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(c => new { c.MunicipalityId, c.Name });
        });

        builder.Entity<ApplicationUser>(e =>
        {
            e.HasOne(u => u.Municipality).WithMany()
                .HasForeignKey(u => u.MunicipalityId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}

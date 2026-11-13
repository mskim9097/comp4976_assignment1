using MEMS.Data.Entities;
using MEMS.Shared;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MEMS.Data;

public class MemsDbContext(DbContextOptions<MemsDbContext> options, TenantContext tenant)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Municipality> Municipalities => Set<Municipality>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<Candidate> Candidates => Set<Candidate>();

    // Used by the query filters below (EF re-reads it for every query of this context instance).
    private int? CurrentTenantId => tenant.MunicipalityId;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(e =>
        {
            e.Property(u => u.Name).IsRequired().HasMaxLength(Limits.NameMax);
            e.HasOne(u => u.Municipality).WithMany(m => m.Admins)
                .HasForeignKey(u => u.MunicipalityId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Municipality>(e =>
        {
            e.Property(m => m.Name).IsRequired().HasMaxLength(Limits.NameMax);
            e.Property(m => m.Slug).IsRequired().HasMaxLength(Limits.SlugMax);
            e.Property(m => m.Province).IsRequired().HasMaxLength(Limits.ProvinceMax);
            e.Property(m => m.Description).HasMaxLength(Limits.DescriptionMax);
            e.Property(m => m.LogoFileName).HasMaxLength(Limits.FileNameMax);
            e.Property(m => m.ThemeColor).IsRequired().HasMaxLength(7);
            e.Property(m => m.WelcomeMessage).HasMaxLength(Limits.WelcomeMessageMax);
            e.HasIndex(m => m.Name).IsUnique();
            e.HasIndex(m => m.Slug).IsUnique();
            e.HasQueryFilter(m => CurrentTenantId == null || m.MunicipalityId == CurrentTenantId);
        });

        builder.Entity<Position>(e =>
        {
            e.Property(p => p.Name).IsRequired().HasMaxLength(Limits.NameMax);
            e.ToTable(t => t.HasCheckConstraint(
                "CK_Positions_SeatsAvailable",
                $"[SeatsAvailable] >= {Limits.SeatsMin} AND [SeatsAvailable] <= {Limits.SeatsMax}"));
            e.HasOne(p => p.Municipality).WithMany(m => m.Positions)
                .HasForeignKey(p => p.MunicipalityId)
                .OnDelete(DeleteBehavior.Cascade);
            // A municipality cannot have two positions with the same name.
            e.HasIndex(p => new { p.MunicipalityId, p.Name }).IsUnique();
            // Lets a candidate point at (position, municipality) together, see below.
            e.HasAlternateKey(p => new { p.PositionId, p.MunicipalityId });
            e.HasQueryFilter(p => CurrentTenantId == null || p.MunicipalityId == CurrentTenantId);
        });

        builder.Entity<Candidate>(e =>
        {
            e.Property(c => c.Name).IsRequired().HasMaxLength(Limits.NameMax);
            e.Property(c => c.Profile).HasMaxLength(Limits.ProfileMax);
            e.Property(c => c.PhotoFileName).HasMaxLength(Limits.FileNameMax);
            e.Property(c => c.Website).HasMaxLength(Limits.WebsiteMax);
            e.Property(c => c.Email).HasMaxLength(Limits.EmailMax);

            // Data integrity: the candidate's tenant can never differ from the tenant of its position.
            e.HasOne(c => c.Position).WithMany(p => p.Candidates)
                .HasForeignKey(c => new { c.PositionId, c.MunicipalityId })
                .HasPrincipalKey(p => new { p.PositionId, p.MunicipalityId })
                .OnDelete(DeleteBehavior.Cascade);
            // Deleting a municipality removes its candidates through the positions (one cascade path only).
            e.HasOne(c => c.Municipality).WithMany(m => m.Candidates)
                .HasForeignKey(c => c.MunicipalityId)
                .OnDelete(DeleteBehavior.NoAction);
            e.HasQueryFilter(c => CurrentTenantId == null || c.MunicipalityId == CurrentTenantId);
        });
    }
}

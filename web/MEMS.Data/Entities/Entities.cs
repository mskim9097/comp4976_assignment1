using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace MEMS.Data.Entities;

// Anything that belongs to exactly one tenant (municipality). Every query on such an entity goes through ForTenant().
public interface ITenantOwned
{
    int MunicipalityId { get; }
}

// A municipality IS the tenant.
public class Municipality
{
    public int MunicipalityId { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    // URL segment: /burnaby, /api/burnaby/candidates
    [Required, MaxLength(63)]
    public string Slug { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Province { get; set; } = string.Empty;

    public DateOnly ElectionDate { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    // Accent color of the tenant's public site (unique look per municipality).
    [Required, MaxLength(7)]
    public string ThemeColor { get; set; } = "#0d6efd";

    // Logo stored in the database so the API, the admin app and the public site share one source (no shared disk).
    public byte[]? LogoData { get; set; }
    [MaxLength(50)] public string? LogoContentType { get; set; }
    // 0 = no logo. Incremented on each upload; used to bust browser caches.
    public int LogoVersion { get; set; }

    public ICollection<Position> Positions { get; set; } = new List<Position>();
    public ICollection<Candidate> Candidates { get; set; } = new List<Candidate>();
}

public class Position : ITenantOwned
{
    public int PositionId { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(1, 100)]
    public int SeatsAvailable { get; set; }

    public int MunicipalityId { get; set; }
    public Municipality? Municipality { get; set; }

    public ICollection<Candidate> Candidates { get; set; } = new List<Candidate>();
}

public class Candidate : ITenantOwned
{
    public int CandidateId { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Profile { get; set; }

    public byte[]? PhotoData { get; set; }
    [MaxLength(50)] public string? PhotoContentType { get; set; }
    public int PhotoVersion { get; set; }

    [MaxLength(500)] public string? Website { get; set; }
    [MaxLength(200)] public string? Email { get; set; }

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public int PositionId { get; set; }
    public Position? Position { get; set; }

    // Stored on the candidate as well as on the position so tenant filtering needs no join. The database guarantees
    // both agree (composite foreign key (PositionId, MunicipalityId) -> Position).
    public int MunicipalityId { get; set; }
    public Municipality? Municipality { get; set; }
}

public class ApplicationUser : IdentityUser
{
    [MaxLength(100)]
    public string? DisplayName { get; set; }

    // null for the Super Admin; exactly one tenant for a Municipality Admin.
    public int? MunicipalityId { get; set; }
    public Municipality? Municipality { get; set; }
}

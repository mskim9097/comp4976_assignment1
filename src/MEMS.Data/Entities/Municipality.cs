namespace MEMS.Data.Entities;

// A municipality is a tenant. The slug is its part of the public URL (/burnaby).
public class Municipality
{
    public int MunicipalityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public DateOnly ElectionDate { get; set; }
    public string? Description { get; set; }
    public string? LogoFileName { get; set; }

    // Tenant-specific look of the public site.
    public string ThemeColor { get; set; } = "#0d6efd";
    public string? WelcomeMessage { get; set; }

    public ICollection<Position> Positions { get; set; } = [];
    public ICollection<Candidate> Candidates { get; set; } = [];
    public ICollection<ApplicationUser> Admins { get; set; } = [];
}

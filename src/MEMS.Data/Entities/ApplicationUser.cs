using Microsoft.AspNetCore.Identity;

namespace MEMS.Data.Entities;

// An administrator. A Municipality Admin belongs to exactly one tenant (MunicipalityId); a Super Admin has none.
public class ApplicationUser : IdentityUser
{
    public string Name { get; set; } = string.Empty;

    public int? MunicipalityId { get; set; }
    public Municipality? Municipality { get; set; }
}

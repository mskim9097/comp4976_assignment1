using Microsoft.AspNetCore.Identity;

namespace MunicipalElections.Api.Models;

public class ApplicationUser : IdentityUser
{
    public string? Name { get; set; }
}

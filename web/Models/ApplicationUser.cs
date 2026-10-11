using Microsoft.AspNetCore.Identity;

namespace MunicipalElections.Web.Models;

// Identity user, extended with the display name collected when someone registers in the iOS app.
public class ApplicationUser : IdentityUser
{
    public string? Name { get; set; }
}

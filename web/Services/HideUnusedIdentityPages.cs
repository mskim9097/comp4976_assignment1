using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace MunicipalElections.Web.Services;

// The default Identity UI ships many pages (register, password reset by email, two-factor, personal data, ...).
// This site only needs login, logout and access denied: administrators are created by the Super Admin and
// the mobile app registers through the Web API. Every other Identity page is removed from routing (404).
public class HideUnusedIdentityPages : IPageRouteModelConvention
{
    private static readonly HashSet<string> Allowed = new(StringComparer.OrdinalIgnoreCase)
    {
        "/Account/Login",
        "/Account/Logout",
        "/Account/AccessDenied"
    };

    public void Apply(PageRouteModel model)
    {
        if (string.Equals(model.AreaName, "Identity", StringComparison.OrdinalIgnoreCase) &&
            !Allowed.Contains(model.ViewEnginePath))
        {
            model.Selectors.Clear();
        }
    }
}

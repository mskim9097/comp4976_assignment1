using MEMS.Shared;

namespace MEMS.Admin.Services;

public static class AdminRoles
{
    public const string Any = $"{Roles.SuperAdmin},{Roles.MunicipalityAdmin}";
    public const string Super = Roles.SuperAdmin;
}

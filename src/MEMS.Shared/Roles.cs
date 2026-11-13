namespace MEMS.Shared;

// The two Identity roles. A Municipality Admin is also tied to exactly one tenant (see ApplicationUser.MunicipalityId).
public static class Roles
{
    public const string SuperAdmin = "Super Admin";
    public const string MunicipalityAdmin = "Municipality Admin";
}

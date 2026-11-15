namespace MEMS.Shared;

public static class MemsRoles
{
    public const string SuperAdmin = "Super Admin";
    public const string MunicipalityAdmin = "Municipality Admin";
}

public static class MemsClaims
{
    // The one tenant (municipality) a Municipality Admin belongs to. Absent for the Super Admin.
    public const string MunicipalityId = "mems:municipality_id";
    public const string MunicipalitySlug = "mems:municipality_slug";
}

public static class MemsPolicies
{
    public const string SuperAdmin = "SuperAdmin";
    public const string AnyAdmin = "AnyAdmin";
}

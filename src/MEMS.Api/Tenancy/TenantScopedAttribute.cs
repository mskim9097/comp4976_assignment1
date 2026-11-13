namespace MEMS.Api.Tenancy;

// Marks an endpoint that works on one municipality. The tenant comes from the URL (/api/burnaby/candidates)
// or from ?city= (the mobile app), see TenantScopeFilter.
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class TenantScopedAttribute : Attribute;

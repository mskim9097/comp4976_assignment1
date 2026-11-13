---
name: tenant-isolation
description: How tenant isolation works in MEMS (EF filters, TenantContext, Api filter, Admin session). Use when you add or change anything that reads or writes municipality data.
---
# Tenant isolation

A municipality is a tenant. Isolation is enforced in four places, so a mistake in one of them is caught by the next.

1. **Database**: `Position` and `Candidate` have `MunicipalityId`. `Candidate (PositionId, MunicipalityId)` is a composite foreign key to `Position (PositionId, MunicipalityId)`, so a candidate can never belong to another tenant than its position. Unique: municipality name and slug, position name per municipality.
2. **EF Core**: `MemsDbContext` query filters on `Municipality`, `Position`, `Candidate` read `TenantContext.MunicipalityId`. Not restricted (null) means all tenants (Super Admin, seeding, public list of municipalities).
3. **Api**: `TenantScopeFilter` runs before every action. A Municipality Admin is always restricted to the `municipalityId` claim of the token. A `[TenantScoped]` endpoint names its tenant in `{slug}` or `?city=`; unknown tenant: 404; administrator of another tenant: 403 plus a warning in the log; then the scope is restricted to that tenant.
4. **Blazor Admin**: `AdminSession.RunAsync` creates a new DI scope (and DbContext) for each operation and restricts its `TenantContext` to the tenant in the login cookie. Pages never talk to the database directly.

Rules for new code:
- Load parents through the filtered services; copy the tenant from the parent.
- A restricted scope can create or change only inside its tenant. Managing municipalities and administrator accounts is Super Admin only (the services check `TenantContext.IsRestricted`).
- `TenantContext.Restrict` can be repeated with the same id but throws for another id.

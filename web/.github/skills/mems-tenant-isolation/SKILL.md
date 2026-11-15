---
name: mems-tenant-isolation
description: Use when adding or changing any code that reads or writes municipality (tenant) data in MEMS.
---
1. Put `MunicipalityId` on the entity and implement `ITenantOwned`. If it references another tenant-owned row, use a composite foreign key (id + MunicipalityId) so the database enforces the same tenant.
2. Service methods: `(TenantScope scope, int municipalityId, ...)`. Reads: `db.X.AsNoTracking().ForTenant(municipalityId)`. Writes: `if (!scope.CanManage(municipalityId)) return Forbidden`.
3. API: controller derives from `TenantControllerBase`, route `api/{tenant}/...`, use `Tenant.MunicipalityId` and `Scope`, `[Authorize(Policy = MemsPolicies.AnyAdmin)]` for writes.
4. Admin pages: inherit `AdminPage`, use `Admin.ActiveTenantId`, never an id typed into the page without the service re-checking it.
5. Public site: only GET calls to `/api/{slug}/...`. Never persist or transmit visitor choices.
6. Test: request another tenant's id and expect 404 (read) or 403 (write).

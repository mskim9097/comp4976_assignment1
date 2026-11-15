# MEMS - Copilot instructions

Municipal Elections Management System. .NET 10, .NET Aspire, SQL Server, EF Core Code First.

## Projects
MEMS.AppHost (orchestration) | MEMS.ServiceDefaults | MEMS.Admin (Blazor Server, shared, /admin) | MEMS.Public (Blazor WASM, /{slug})
MEMS.Api (ASP.NET WebAPI) | MEMS.Data (EF Core, Identity, service layer, seed) | MEMS.Shared (DTOs, validation, constants)

## Non-negotiable rules
1. Every municipality is a tenant. Anything owned by a tenant implements `ITenantOwned`, and every query on it uses `.ForTenant(id)`.
2. Services take the tenant id and a `TenantScope`; writes call `scope.CanManage(id)` first. Never trust ids coming from the UI.
3. Municipality Admins belong to exactly one tenant (claim `mems:municipality_id`); only the Super Admin manages tenants.
4. Election privacy: ballot selections exist only in browser Session Storage. Never add a table, endpoint or HTTP call that stores or sends them.
5. MEMS.Public must never reference MEMS.Data or use anything but GET requests to the API.
6. Validate in three places: DataAnnotations on the DTO (Blazor + [ApiController]), service checks, EF constraints.
7. Return RFC 7807 problem details; never expose exception text. Log with structured templates (`logger.LogInformation("... {Id}", id)`).
8. Voters may choose any number of candidates per position and from several municipalities; administrators have no voting-guide tools.
9. Secrets and connection strings come from Aspire; never commit them.

## Style
File-scoped namespaces, nullable enabled, `async` all the way with CancellationToken in services, Bootstrap 5 for UI.

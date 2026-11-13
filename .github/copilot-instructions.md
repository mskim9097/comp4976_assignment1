# Copilot instructions - Municipal Elections Management System (MEMS)

BCIT COMP4976 (Blazor, multi-tenant) and COMP4977 (iOS app that uses the same Web API), Assignment 2. One repository, one solution (`MEMS.sln`), one SQL Server database.

## Solution layout (`src/`)
- `MEMS.AppHost`: .NET Aspire. Starts the SQL Server container (`memsdb`) and the Api, Admin and Public projects, and passes connection strings, the JWT key, the uploads folder and the CORS origins.
- `MEMS.ServiceDefaults`: logging, tracing, health checks and service discovery for the server projects.
- `MEMS.Shared`: DTOs and request classes with validation attributes, `Roles`, `Limits`, `TenantSlug`, `Provinces`, `SelectionRules`. No dependencies.
- `MEMS.Data`: EF Core Code First (`MemsDbContext`, entities, migrations, seed), `TenantContext`, the services (`MunicipalityService`, `PositionService`, `CandidateService`, `DashboardService`, `AdminUserService`, `ImageFileStore`).
- `MEMS.Api`: ASP.NET Web API (controllers, JWT, `TenantScopeFilter`, `ApiExceptionHandler`, Swagger). It is the only project that migrates and seeds the database.
- `MEMS.Admin`: Blazor Server under `/admin`, Identity cookie login. Every data operation runs through `AdminSession.RunAsync`.
- `MEMS.Public`: Blazor WebAssembly. It has no reference to `MEMS.Data` and never touches the database; it only calls `/api/{slug}/...`.

## Multi-tenancy (the most important rule)
- A municipality is a tenant. Its `Slug` is in the public URL (`/burnaby`). `Position`, `Candidate` and `ApplicationUser` carry the tenant in `MunicipalityId`.
- A candidate's tenant always comes from its position, never from the request. The database enforces it with a composite foreign key.
- `MemsDbContext` has query filters on `Municipality`, `Position` and `Candidate`. They use `TenantContext`; once restricted, a scope can never move to another tenant.
- Api: `TenantScopeFilter` restricts the scope (route `{slug}` or `?city=`), answers 404 for an unknown tenant and 403 for an administrator of another municipality.
- Admin: never inject the DbContext or services into a page. Use `AdminSession.RunAsync` (new scope per operation, restricted to the tenant in the login cookie) and `AdminSession.TryAsync` for messages.
- Never trust ids, slugs or tenant ids from a form or URL: load rows through the services (they are filtered).

## Conventions
- C#: PascalCase, nullable reference types on, file-scoped namespaces, primary constructors for DI, `async`/`await` with `CancellationToken` where a request can be cancelled.
- Controllers stay thin and call the services. Rules go into the services or `MEMS.Shared`, not into controllers or pages.
- Services throw `NotFoundException`, `ConflictException`, `ForbiddenException`, `RuleViolationException`. `ApiExceptionHandler` turns them into problem responses; the Admin app shows the message. Unexpected errors are logged and answered with a general message (no stack traces).
- Validation in three layers: DataAnnotations on the request classes in `MEMS.Shared` (Blazor forms and Api model binding), checks in the services, constraints in the database (required, lengths, unique, check, foreign keys).
- Razor: string parameters need `@` (`Error="@error"`). Blazor pages use Bootstrap 5 classes only.
- Use structured logging (`logger.LogWarning("Tenant violation: user {UserId} ...", id)`), never string concatenation.
- The voter's choices are never stored on the server: `MEMS.Public` keeps them in `sessionStorage` (key `mems.guide`).

## Configuration and secrets
- Connection string `memsdb` comes from Aspire. `Jwt__Key` comes from the AppHost parameter in development and from an app setting elsewhere. Do not commit secrets.
- Seed data (4 municipalities, 12 positions, 25 candidates, roles and users `super`, `pm`, `burnabyadmin`, `surreyadmin`, `richmondadmin`, password `P@$$w0rd`) is created by `MemsDbSeeder` at Api start-up.

## Do not
- Do not use MVC views or the Identity UI package. SQL Server is the database of the .NET solution (Aspire); SQLite is only used by the deployed Api on Azure (`Database__Provider=Sqlite`, schema from `EnsureCreated`, because the migrations are written for SQL Server). Keep the model free of provider-specific features.
- Do not commit `*.db`, `uploads/`, `bin/`, `obj/` or build output.
- Do not edit generated migrations; add a new one: `dotnet tool run dotnet-ef migrations add <Name> --project src/MEMS.Data`.
- Do not merge into `main` without checking the Azure deployment (a push to `main` redeploys the Api).

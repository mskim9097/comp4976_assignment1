# Copilot instructions - Municipal Elections Management System

## Project
- `comp4976/web/`: ASP.NET Core MVC (.NET 10) for COMP4976. Public site + Admin area.
- `comp4977/api/`: ASP.NET Core Web API (.NET 10, JWT) for COMP4977.
- Data access: Entity Framework Core Code First with SQLite. Authentication: ASP.NET Core Identity.

## Conventions
- C# naming: PascalCase for types and members, `_camelCase` for private fields, nullable reference types enabled.
- Controllers stay thin; shared logic goes in services or the DbContext.
- Use view models and `[Required]`, `[StringLength]`, `[EmailAddress]`, `[Url]` data annotations. Always check `ModelState.IsValid` on the server.
- Authorization is enforced on the server in every action (not only by hiding UI): Super Admin manages everything; a municipality admin manages only their own municipality's positions and candidates.
- Uploaded images: validate extension, content type and size; store a generated file name, never the original.

## Configuration and secrets
- The connection string lives in `appsettings.Development.json`, which is git-ignored. Update the matching `appsettings.Development.example.json` when keys change.
- Seed data (roles, users `super` and `pm`, municipalities, positions, candidates) runs at startup after `Database.Migrate()`.

## Do not
- Do not commit `*.db`, `appsettings.Development.json`, or uploaded files.
- Do not edit generated migration files by hand; add a new migration instead.

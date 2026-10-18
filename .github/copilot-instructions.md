# Copilot instructions - Municipal Elections Management System

BCIT COMP4976 (ASP.NET Core MVC) and COMP4977 (iOS + Web API), Assignment 1. One repository, one ASP.NET project, one SQLite database.

## Project layout
- `web/`: one ASP.NET Core (.NET 10) project. It serves the public MVC site, the Admin area and the JSON Web API.
  - `Controllers/`: public MVC controllers (`Home`, `CandidateDirectory`, `Positions`). `Controllers/Api/`: JSON API (JWT), used by the iOS app.
  - `Areas/Admin/`: the Admin area (`Dashboard`, `SuperAdmin`, `MunicipalityAdmin` controllers, views, view models). Cookie login.
  - `Areas/Identity/`: the customized login page (user name or email).
  - `Models/`: entities and view models. `Dtos/`: JSON shapes of the Web API. `Data/`: `ApplicationDbContext`, `DbSeeder`, `Migrations/`.
  - `Services/`: `AdminAccess` (which municipalities an admin manages), `ImageUploadStorage` (safe image uploads).
  - `Extensions/`: image URL helper and OpenAPI/Swagger setup. `wwwroot/`: CSS, JS, Bootstrap, images.
- `ios/`: SwiftUI app (MVVM) for COMP4977 that calls the Web API. `docs/`: ERD, architecture, `readme.txt`. `.github/`: this file, prompts, skills, deployment workflow.
- Entities: `Municipality`, `Position`, `Candidate`, `SavedCandidate` plus Identity (`ApplicationUser`, roles). `Candidate` has no `MunicipalityId`; the municipality comes from its `Position`.
- Groups are Identity roles: `Super Admin` and one admin role per municipality, named in `Municipality.AdminRoleName` (for example `Pitt Meadows Admin`).

## Reference style
- Follow the class examples where they exist: `comp4976/week03/RazorSportsSQLiteMVC` (MVC controllers, `Index/Details/Create/Edit/Delete`, tag-helper views, `asp-validation-*`), `comp4976/week05/SeedIdentity` (startup `Migrate()` + role/user seeding), `comp4976/week04/HealthAPI` (Web API), `comp4977/week05/MovieFactsMVVM` (SwiftUI MVVM).
- Where the course has no example (file upload, areas, print CSS, JWT), use the official ASP.NET Core approach and keep it small.

## Conventions
- C#: PascalCase for types and members, nullable reference types on, file-scoped namespaces, constructor injection (primary constructors are fine), `async`/`await` with `...Async` for helpers.
- Controllers stay thin. Rules that more than one controller needs go in `Services/`.
- Use view models for forms. Validate with data annotations (`[Required]`, `[StringLength]`, `[EmailAddress]`, `[Url]`, `[Range]`) AND check `ModelState.IsValid` on the server. Views include `_ValidationScriptsPartial` for client-side validation.
- Every POST action has `[ValidateAntiForgeryToken]`.
- Authorization is enforced on the server in every admin action. Never rely on hiding links. See the `admin-authorization` skill.
- Uploaded images go through `ImageUploadStorage` only. See the `image-upload` skill.
- The JSON API is a contract with the iOS app. See the `api-contract-ios` skill before changing `Dtos/` or `Controllers/Api/`.
- Bootstrap 5.3 for the UI. Use theme-aware classes (`bg-body-tertiary`, `text-body-secondary`), not fixed light colors, so dark mode keeps working (`data-bs-theme`).

## Configuration and secrets
- The connection string and `Jwt:Key` live in `web/appsettings.Development.json`, which is git-ignored. Update `appsettings.Development.example.json` when keys change. On Azure they come from the `ConnectionStrings__DefaultConnection` and `Jwt__Key` app settings.
- Seed data (roles, users `super` and `pm` with password `P@$$w0rd`, municipalities, positions, candidates) runs at startup after `Database.Migrate()` in `Data/DbSeeder.cs`.

## Do not
- Do not commit `*.db`, `appsettings.Development.json`, uploaded images (GUID file names) or build output.
- Do not edit generated migration files by hand; add a new migration. Tell the team before changing an entity (shared database and migrations).
- Do not push to `main` or `dev` directly: work on your own branch and open a pull request.

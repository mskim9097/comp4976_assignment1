---
description: Change an entity safely (shared database): migration, seed data, API DTOs, ERD and docs
agent: agent
---
I need to change the entity `${input:entity}`: ${input:change}.

1. Stop and confirm with the team first: MVC, Web API and iOS share one SQLite database and one migration history.
2. Change the model in `web/Models/` and `Data/ApplicationDbContext.cs` (indexes, delete behavior). Keep the assignment's minimum columns for `Municipality`, `Position`, `Candidate`.
3. Create a new migration (`dotnet ef migrations add <Name> -o Data/Migrations` from `web/`); do not edit old migrations. Check the generated `Up`/`Down`.
4. Update `Data/DbSeeder.cs` so a fresh database is seeded correctly. Seeding must stay idempotent.
5. Update the API DTOs in `web/Dtos/` so the JSON the iOS app reads does not change (or follow the `api-contract-ios` skill if it must).
6. Update `docs/erd.mmd`, `docs/erd.md` and regenerate `docs/erd.png`.
7. Build, run the app against a fresh database, and check the admin screens and `/api/...` still work.

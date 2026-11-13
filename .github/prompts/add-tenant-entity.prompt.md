---
description: Add an entity that belongs to a municipality (tenant): model, filter, migration, service
agent: agent
---
Add the entity `${input:entity}` (fields: ${input:fields}) to the MEMS solution.

1. `src/MEMS.Data/Entities/`: the class with a `MunicipalityId` and a `Municipality` navigation. Add the collection to `Municipality`.
2. `MemsDbContext`: required fields, `MaxLength` from `MEMS.Shared.Limits`, unique indexes where a name must be unique inside a municipality, a global query filter like the other entities (`CurrentTenantId == null || e.MunicipalityId == CurrentTenantId`), delete behavior (cascade from the municipality, but only one cascade path).
3. Migration: `dotnet tool run dotnet-ef migrations add <Name> --project src/MEMS.Data` and read the generated `Up`.
4. Service in `src/MEMS.Data/Services/` with list, get, create, update, delete. The tenant of the new row must come from a parent loaded through the filtered sets, never from the request. Throw the service exceptions (not found, conflict, rule violation).
5. Request and DTO classes with validation attributes in `src/MEMS.Shared/Dtos/`.
6. Register the service in `DependencyInjection.AddMemsServices`.
7. Update `docs/erd.png` (and the entity list in `docs/api.md`).

Finish with the commands to run and the manual checks (a Municipality Admin cannot read or change a row of another municipality).

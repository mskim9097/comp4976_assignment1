# MEMS database

The `MEMS.Data` project defines the EF Core schema used by the API. Aspire provides SQL Server during local development. The migration source is in `web/MEMS.Data/Data/Migrations/`, and the diagram is maintained in [erd.mmd](erd.mmd).

## Entities

| Entity | Purpose |
|---|---|
| `Municipality` | Election jurisdiction, identified by a unique slug |
| `Position` | Elected role within a municipality |
| `Candidate` | Candidate for one position |
| `ApplicationUser` and Identity tables | Accounts and administrative roles |

## Relationships and constraints

- One municipality has many positions; each position has many candidates.
- A position name is unique within its municipality. Municipality names and slugs are unique.
- `Municipality.AdminRoleName` names the corresponding Identity role; it is not a foreign key.
- Candidate municipality membership is derived from its position.
- Database migrations are generated from `MemsDbContext`; do not edit generated files by hand.

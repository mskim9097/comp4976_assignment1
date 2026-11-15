# Copilot instructions - Municipal Elections Management System

## Project layout
- `web/` contains the .NET 10 multi-project solution. `MEMS.AppHost` orchestrates SQL Server, `MEMS.Api`, `MEMS.Admin`, and `MEMS.Public`.
- `MEMS.Data` owns the EF Core model, SQL Server DbContext, migrations, and seed data. `MEMS.Shared` contains DTOs, and `MEMS.ServiceDefaults` contains shared Aspire defaults.
- `ios/` is the SwiftUI client. Do not modify it unless explicitly requested.

## Conventions
- C# naming: PascalCase for types and members, `_camelCase` for private fields, nullable reference types enabled.
- Keep controllers thin and put shared data logic in `MEMS.Data` or services.
- Validate API input and return clear error responses.
- Use EF Core Code First. Do not edit generated migration files by hand; add a migration instead.

## Configuration and secrets
- Aspire starts SQL Server in a container and provides the API connection string.
- Keep the SQL password in .NET User Secrets; do not commit secrets, databases, or local development settings.

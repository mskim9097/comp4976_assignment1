# Municipal Elections Management System

BCIT COMP4976 / COMP4977 - Assignment 1 (due Fri Oct 23, 2026, 11:59 pm)

| Folder | Course | Description |
|---|---|---|
| `web/` | COMP4976 | ASP.NET Core MVC app (public site + Admin area), EF Core Code First, SQLite, Identity |
| `api/` | COMP4977 | ASP.NET Core Web API (JWT), EF Core, SQLite |
| `ios/` | COMP4977 | SwiftUI app that consumes `api/` (added later) |
| `docs/` | COMP4976 | `readme.txt` and project documentation |

Requirements: .NET 10 SDK (`dotnet --version`), `dotnet tool install --global dotnet-ef`.

## Local setup

The connection string is **not** committed. Copy the example file in each project and keep the copy untracked:

```bash
cp web/appsettings.Development.example.json web/appsettings.Development.json
cp api/appsettings.Development.example.json api/appsettings.Development.json
```

On Windows (PowerShell): `Copy-Item web/appsettings.Development.example.json web/appsettings.Development.json`

Run:

```bash
dotnet build MunicipalElections.sln
dotnet run --project web
dotnet run --project api
```

## Branches

`main` is only updated through merges. Work on your own branch and merge when it builds.

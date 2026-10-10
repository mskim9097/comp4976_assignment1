# Municipal Elections Management System

BCIT COMP4976 / COMP4977 - Assignment 1 (due Fri Oct 23, 2026, 11:59 pm)

One GitHub repository, two separate D2L submissions. Each course folder is self-contained
and is zipped on its own for D2L.

| Folder | Course | Contents |
|---|---|---|
| `comp4976/` | COMP4976 | `MunicipalElections.Web.sln`, `web/` (ASP.NET Core MVC: public site + Admin area, EF Core Code First, SQLite, Identity), `docs/readme.txt` |
| `comp4977/` | COMP4977 | `MunicipalElections.Api.sln`, `api/` (ASP.NET Core Web API, JWT, EF Core, SQLite), `ios/` (SwiftUI app, added later), `docs/` |
| `.github/` | COMP4976 | Copilot instructions / prompts / skills (must stay at the repo root) |

Requirements: .NET 10 SDK (`dotnet --version`), `dotnet tool install --global dotnet-ef`.

## Local setup

The connection string is **not** committed. Copy the example file in each project and keep the copy untracked:

```bash
cp comp4976/web/appsettings.Development.example.json comp4976/web/appsettings.Development.json
cp comp4977/api/appsettings.Development.example.json comp4977/api/appsettings.Development.json
```

On Windows (PowerShell): `Copy-Item comp4976/web/appsettings.Development.example.json comp4976/web/appsettings.Development.json`

Build and run:

```bash
dotnet build comp4976/MunicipalElections.Web.sln
dotnet run --project comp4976/web
dotnet build comp4977/MunicipalElections.Api.sln
dotnet run --project comp4977/api
```

## Branches

Work on your own branch (`minsu`, `justice`) and open a Pull Request into `dev`.
`dev` is merged into `main` when stable. Do not push directly to `main` or `dev`.

## Submission (zip per course)

- COMP4976 zip: the `comp4976/` folder plus root `.github/`, `.gitignore`, `.gitattributes`, `global.json`.
- COMP4977 zip: the `comp4977/` folder plus root `.gitignore`, `.gitattributes`, `global.json`.
- Exclude `bin/`, `obj/`, `*.db`, `appsettings.Development.json`.

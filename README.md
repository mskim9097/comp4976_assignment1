# Municipal Elections Management System

BCIT COMP4976 / COMP4977 - Assignment 1 (due Fri Oct 23, 2026, 11:59 pm)

One repository for both courses. The MVC site and the Web API use one SQLite database
(see [docs/erd.md](docs/erd.md)); the iOS app talks to the Web API.

| Folder | What it is |
|---|---|
| `web/` | ASP.NET Core MVC site (COMP4976): public site, Admin area, EF Core Code First, SQLite, Identity |
| `api/` | ASP.NET Core Web API with JWT and Swagger (COMP4977). Being merged into `web/`, which will serve both |
| `ios/` | SwiftUI app "Municipal Elections Companion" (COMP4977) |
| `docs/` | `readme.txt` (COMP4976), ERD, architecture diagram, [API documentation](docs/api.md) |
| `.github/` | Copilot instructions / prompts / skills (COMP4976) and the Azure deployment workflow; must stay at the repo root |

Requirements: .NET 10 SDK (`dotnet --version`), `dotnet tool install --global dotnet-ef`; Xcode for the iOS app.

## Deployed apps

| What | URL |
|---|---|
| Web API (Swagger UI at `/swagger`) | https://municipalelections-api-mskim9097-edg2grfychegfwes.canadacentral-01.azurewebsites.net |
| MVC site | _to be added_ |

## Local setup

Secrets and connection strings are **not** committed. Copy the example file in each project
and keep the copy untracked:

```bash
cp web/appsettings.Development.example.json web/appsettings.Development.json
cp api/appsettings.Development.example.json api/appsettings.Development.json
```

On Windows (PowerShell): `Copy-Item web/appsettings.Development.example.json web/appsettings.Development.json`

The API also needs a JWT signing key in its `appsettings.Development.json`; see [docs/api.md](docs/api.md).

Build and run:

```bash
dotnet build MunicipalElections.sln
dotnet run --project web
dotnet run --project api --launch-profile http
```

iOS app (macOS with Xcode only): open `ios/MunicipalElectionsCompanion/MunicipalElectionsCompanion.xcodeproj`,
pick an iPhone simulator and press Run. See [ios/README.md](ios/README.md).

## Branches

Work on your own branch (`minsu`, `justice`) and open a Pull Request into `dev`.
`dev` is merged into `main` when stable. Do not push directly to `main` or `dev`.
Pushes to `main` that change `api/` redeploy the API to Azure automatically.

## Submission

Both courses are submitted from this repository (zip of the complete repository without `bin/`, `obj/`,
`*.db`, `appsettings.Development.json`, `xcuserdata`). Required files: `docs/readme.txt` (COMP4976),
the Copilot files in `.github/`, and the project document with ERD, screenshots and the Azure URL.

# Municipal Elections Management System

BCIT COMP4976 / COMP4977 - Assignment 1 (due Fri Oct 23, 2026, 11:59 pm)

One GitHub repository, two separate D2L submissions. Each course folder is self-contained
and is zipped on its own for D2L.

| Folder | Course | Contents |
|---|---|---|
| `comp4976/` | COMP4976 | `MunicipalElections.Web.sln`, `web/` (ASP.NET Core MVC: public site + Admin area, EF Core Code First, SQLite, Identity), `docs/readme.txt` |
| `comp4977/` | COMP4977 | `MunicipalElections.Api.sln`, `api/` (ASP.NET Core Web API, JWT, EF Core, SQLite), `ios/` (SwiftUI app "Municipal Elections Companion"), `docs/` (ERD, architecture), `README.md` (API details, endpoints, Azure URL) |
| `.github/` | both | Copilot instructions / prompts / skills (COMP4976) and the Azure deployment workflow for the COMP4977 API; must stay at the repo root |

Requirements: .NET 10 SDK (`dotnet --version`), `dotnet tool install --global dotnet-ef`.

## Deployed apps

| Course | URL |
|---|---|
| COMP4977 API | https://municipalelections-api-mskim9097-edg2grfychegfwes.canadacentral-01.azurewebsites.net |
| COMP4976 web | _to be added_ |

## Local setup

Secrets and connection strings are **not** committed. Copy the example file in each project
and keep the copy untracked:

```bash
cp comp4976/web/appsettings.Development.example.json comp4976/web/appsettings.Development.json
cp comp4977/api/appsettings.Development.example.json comp4977/api/appsettings.Development.json
```

On Windows (PowerShell): `Copy-Item comp4976/web/appsettings.Development.example.json comp4976/web/appsettings.Development.json`

The API also needs a JWT signing key in its `appsettings.Development.json`; see
[comp4977/README.md](comp4977/README.md).

Build and run:

```bash
dotnet build comp4976/MunicipalElections.Web.sln
dotnet run --project comp4976/web
dotnet build comp4977/MunicipalElections.Api.sln
dotnet run --project comp4977/api --launch-profile http
```

iOS app (macOS with Xcode only): open `comp4977/ios/MunicipalElectionsCompanion/MunicipalElectionsCompanion.xcodeproj`,
pick an iPhone simulator and press Run. It talks to the deployed Azure API by default; see
[comp4977/README.md](comp4977/README.md).

## Branches

Work on your own branch (`minsu`, `justice`) and open a Pull Request into `dev`.
`dev` is merged into `main` when stable. Do not push directly to `main` or `dev`.
Pushes to `main` that change `comp4977/api/` redeploy the API to Azure automatically.

## Submission (zip per course)

- COMP4976 zip: the `comp4976/` folder plus root `.github/`, `.gitignore`, `.gitattributes`, `global.json`.
- COMP4977 zip: the `comp4977/` folder plus root `.gitignore`, `.gitattributes`, `global.json`.
- Exclude `bin/`, `obj/`, `*.db`, `appsettings.Development.json`.

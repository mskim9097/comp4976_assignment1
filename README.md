# Municipal Elections Management System

BCIT COMP4976 / COMP4977 - Assignment 1 (due Fri Oct 23, 2026, 11:59 pm)

One repository for both courses. The MVC site and the Web API are one project with one SQLite database
(see [docs/erd.md](docs/erd.md)); the iOS app talks to the Web API.

| Folder | What it is |
|---|---|
| `web/` | One ASP.NET Core project: the MVC site (COMP4976: public site, Admin area, cookie login) and the Web API under `/api` (COMP4977: JWT, Swagger). EF Core Code First, one SQLite database, Identity |
| `ios/` | SwiftUI app "Municipal Elections Companion" (COMP4977) |
| `docs/` | `readme.txt` (COMP4976), ERD, architecture diagram, [API documentation](docs/api.md) |
| `.github/` | Copilot instructions / prompts / skills (COMP4976) and the Azure deployment workflow; must stay at the repo root |

Requirements: .NET 10 SDK (`dotnet --version`), `dotnet tool install --global dotnet-ef`; Xcode for the iOS app.

## Deployed apps

| What | URL |
|---|---|
| Web app: MVC site at `/`, Web API at `/api/...`, Swagger UI at `/swagger` | https://municipalelections-api-mskim9097-edg2grfychegfwes.canadacentral-01.azurewebsites.net |

## Local setup

Secrets and connection strings are **not** committed. Copy the example file and keep the copy untracked:

```bash
cp web/appsettings.Development.example.json web/appsettings.Development.json
```

On Windows (PowerShell): `Copy-Item web/appsettings.Development.example.json web/appsettings.Development.json`

Set `Jwt:Key` in `web/appsettings.Development.json` to a random string of at least 32 characters (`openssl rand -base64 48`); see [docs/api.md](docs/api.md).
Seeded admin users (password `P@$$w0rd`): `super` (Super Admin) and `pm` (Pitt Meadows Admin).

Build and run:

```bash
dotnet build MunicipalElections.sln
dotnet run --project web --launch-profile http     # http://localhost:5276
```

iOS app (macOS with Xcode only): open `ios/MunicipalElectionsCompanion/MunicipalElectionsCompanion.xcodeproj`,
pick an iPhone simulator and press Run. See [ios/README.md](ios/README.md).

## Branches

Work on your own branch (`minsu`, `justice`) and open a Pull Request into `dev`.
`dev` is merged into `main` when stable. Do not push directly to `main` or `dev`.
Pushes to `main` that change `web/` redeploy the app to Azure automatically.

## Submission

The instructor asked us not to split the ASP.NET project: the same project is submitted twice, once in the
COMP4976 (.NET) D2L dropbox and once in the COMP4977 (iOS) dropbox. We submit one zip of the whole repository
(`web/`, `ios/`, `docs/`, `.github/`, solution and settings files) to both, plus a project document for each course.

Create the zip from the committed files of `main` (no `bin/`, `obj/`, `*.db`, `appsettings.Development.json`,
`xcuserdata`, class material):

```bash
git archive --format=zip --prefix=MunicipalElections/ -o MunicipalElections.zip main
```

Required files: `docs/readme.txt` (COMP4976), the Copilot files in `.github/`, and the project document with the ERD,
screenshots, the Azure URL and the GitHub URL.

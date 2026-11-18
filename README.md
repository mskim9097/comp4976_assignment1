# Municipal Elections Management System (MEMS) - Assignment 2

BCIT COMP4976 / COMP4977, Assignment 2 (due Fri Nov 20, 2026, 11:59 pm). Team: Minsu Kim (A01424659), Justice Plunkett (A01405284).

MEMS is a multi-tenant application. Every municipality is a tenant with its own public site (`/burnaby`, `/surrey`, `/richmond`, `/pitt-meadows`), its own candidates and its own administrators. One shared administration site (`/admin`) manages all of them, and one Web API serves the public site and the iOS app. Everything is orchestrated with .NET Aspire.

![Architecture](docs/architecture.png)

## Solution (`MEMS.sln`)

| Project | What it is |
|---|---|
| `src/MEMS.AppHost` | .NET Aspire AppHost: SQL Server container, Api, Admin, Public, service discovery, connection strings, dashboard |
| `src/MEMS.ServiceDefaults` | Logging, tracing, health checks, service discovery |
| `src/MEMS.Shared` | DTOs and request classes with validation, roles, limits, shared rules |
| `src/MEMS.Data` | EF Core Code First, migrations, seed data, tenant-aware services |
| `src/MEMS.Api` | ASP.NET Web API (JWT, tenant isolation, Swagger) |
| `src/MEMS.Admin` | Blazor Server administration site (`/admin`, ASP.NET Core Identity) |
| `src/MEMS.Public` | Blazor WebAssembly public site (one app, tenant from the URL) |
| `ios/` | SwiftUI app "Municipal Elections Companion" (COMP4977) |
| `docs/` | API documentation, ERD, architecture diagram |

## Run it

Requirements: .NET 10 SDK (`global.json`), Docker Desktop (running), and optionally the Aspire CLI.

```bash
dotnet run --project src/MEMS.AppHost      # or: aspire run
```

The first start pulls the SQL Server 2022 image. The Api creates the database, applies the migration and loads the seed data. The AppHost passes the connection string (`memsdb`) and a development-only JWT key; nothing has to be configured by hand. Uploaded photos and logos go to the git-ignored `uploads/` folder.

| What | Address |
|---|---|
| Aspire dashboard | https://localhost:17228 (the exact link, with login token, is printed in the terminal) |
| Public site | https://localhost:7025 (tenants: `/burnaby`, `/surrey`, `/richmond`, `/pitt-meadows`) |
| Admin site | https://localhost:7036/admin |
| Web API and Swagger UI | https://localhost:7261/swagger |

The public site reads the Api address from `src/MEMS.Public/wwwroot/appsettings.json`, because a browser app cannot use the server-side service discovery. It matches the fixed Api port in `launchSettings.json`.

iOS app (macOS with Xcode only): open `ios/MunicipalElectionsCompanion/MunicipalElectionsCompanion.xcodeproj`, pick an iPhone or iPad simulator and press Run. See [ios/README.md](ios/README.md).

## Deployed API (for the iOS app)

| What | URL |
|---|---|
| Web API, Swagger UI at `/swagger` | https://municipalelections-api-mskim9097-edg2grfychegfwes.canadacentral-01.azurewebsites.net |

## Test accounts (password for all: `P@$$w0rd`)

| User | Role | Tenant |
|---|---|---|
| `super` | Super Admin | all municipalities |
| `burnabyadmin` | Municipality Admin | Burnaby |
| `surreyadmin` | Municipality Admin | Surrey |
| `richmondadmin` | Municipality Admin | Richmond |
| `pm` | Municipality Admin | Pitt Meadows |
| `aa@aa.aa` | none | iOS app login (exists on the Azure server only; locally create an account in the app) |

Seed data: 4 municipalities, 12 positions (Mayor, Councillor, School Trustee), 25 fictional candidates. People who register in the iOS app are normal users without a role and cannot log in to the admin site.

## Documentation

- [docs/api.md](docs/api.md): endpoints, request and response examples, error responses
- [docs/erd.png](docs/erd.png), [docs/architecture.png](docs/architecture.png), [docs/ios-architecture.png](docs/ios-architecture.png): diagrams
- [docs/deficiency-report.md](docs/deficiency-report.md): how the Assignment 1 findings were fixed
- `.github/`: `copilot-instructions.md`, prompts and skills for the AI assistants (GitHub Copilot, Claude Code), and the Azure deployment workflow

## Branches and deployment

Work is done on a personal branch, merged into `dev`, and `dev` into `main`. A push to `main` that changes `src/` redeploys only the Web API (`MEMS.Api`) to Azure App Service (`.github/workflows`). The Blazor Admin and Public sites run locally with Aspire.

The deployed Api is used by the iOS app. To save Azure credits, as advised by the instructor, it stores its data in a **SQLite file** instead of SQL Server. The Api supports both. The Aspire setup for the .NET assignment uses SQL Server. The App Service needs these settings:

| Setting | Value |
|---|---|
| `Database__Provider` | `Sqlite` |
| `ConnectionStrings__memsdb` | `Data Source=/home/data/mems.db` |
| `Jwt__Key` | a random string of at least 32 characters |
| `Uploads__Path` | `/home/data/uploads` |
| Startup command | `dotnet MEMS.Api.dll` |

The database file is created and seeded when the Api starts.

## Submission

The same repository is submitted for COMP4976 and COMP4977. The **iOS project (and `firebase/`) must not be in the COMP4976 zip**; the COMP4977 zip has everything.

Create the zips from the committed files of `main` (no `bin/`, `obj/`, `*.db`, `appsettings.Development.json`). Run this in the repository root; the zips are written next to the repository folder:

```bash
# COMP4976 (.NET): everything except the iOS app and its Firebase rules
git archive --format=zip --prefix=MEMS/ -o ../MEMS-COMP4976.zip main -- . ':(exclude)ios' ':(exclude)firebase'

# COMP4977 (iOS): the complete repository, including ios/
git archive --format=zip --prefix=MEMS/ -o ../MEMS-COMP4977.zip main
```

As in Assignment 1, each zip also contains its own project document (PDF) in the `MEMS/` folder:

```bash
cd .. && mkdir -p doc76/MEMS doc77/MEMS
cp /path/to/COMP4976-Assignment2-Documentation.pdf doc76/MEMS/ && (cd doc76 && zip ../MEMS-COMP4976.zip MEMS/*.pdf)
cp /path/to/COMP4977-Assignment2-Documentation.pdf doc77/MEMS/ && (cd doc77 && zip ../MEMS-COMP4977.zip MEMS/*.pdf)
```

Links to `ios/` do not resolve inside the COMP4976 zip.

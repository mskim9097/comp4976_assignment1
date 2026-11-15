# Municipal Elections Management System

BCIT COMP4976 / COMP4977 project. The web application is a multi-project .NET solution orchestrated by .NET Aspire; the SwiftUI app remains in `ios/`.

| Path | Purpose |
|---|---|
| `web/MEMS.AppHost/` | Aspire AppHost for the SQL Server, API, Admin, and Public projects |
| `web/MEMS.Api/` | JSON Web API and Swagger |
| `web/MEMS.Admin/` | Admin web application |
| `web/MEMS.Public/` | Public web application |
| `web/MEMS.Data/` | EF Core models, DbContext, migrations, and seed data |
| `web/MEMS.Shared/` | API DTOs shared by the web projects |
| `web/MEMS.ServiceDefaults/` | Shared Aspire service defaults |
| `ios/` | SwiftUI app "Municipal Elections Companion" |
| `docs/` | Assignment notes, architecture, data model, and API documentation |

## Requirements

- .NET 10 SDK
- .NET Aspire CLI
- Docker Desktop (Aspire starts SQL Server in a container)
- Xcode for the iOS app

## Build and run

Build the solution from the repository root:

```powershell
dotnet build web\MEMS.slnx
```

Run the Aspire application from `web/`:

```powershell
Set-Location web
aspire run
```

Aspire starts SQL Server, the API, Admin, and Public apps. Open the Aspire dashboard to find each app's URL and the API's Swagger UI. SQL Server data persists in the Docker volume `mems-sql-data-v2`. The SQL password is an Aspire secret parameter; set it with .NET User Secrets if prompted:

```powershell
dotnet user-secrets set "Parameters:sql-password" "<strong-password>" --project MEMS.AppHost\MEMS.AppHost.csproj
```

The iOS app is built separately on macOS with Xcode. Open `ios/MunicipalElectionsCompanion/MunicipalElectionsCompanion.xcodeproj`, select an iPhone simulator, and run it.

## Deployment

The legacy Azure App Service deployment workflow has been retired. No automated deployment is configured for the Aspire application.

## Submission

Both courses are submitted from this repository. Exclude build outputs, `*.db`, `appsettings.Development.json`, and `xcuserdata` from the submission.

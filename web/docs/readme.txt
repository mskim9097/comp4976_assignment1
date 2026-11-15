MEMS - Municipal Elections Management System (COMP4976 Assignment 2)
====================================================================

TEAM
  Student 1: <name, student id>
  Student 2: <name, student id>

CONTRIBUTIONS
  Student 1: <e.g. MEMS.Data, MEMS.Api, tenant isolation, Aspire AppHost>
  Student 2: <e.g. MEMS.Admin (Blazor Server), MEMS.Public (Blazor WASM), documentation>

DEPLOYED APPLICATION
  No Azure deployment is required for Assignment 2. Run locally through Aspire (see below).
  GitHub repository: <url>

WHAT TO INSTALL
  - .NET 10 SDK, Visual Studio 2026 (or 2022 17.14+) with the ASP.NET workload
  - Docker Desktop (or Podman) running: Aspire starts SQL Server in a container
  - Aspire CLI/templates are NOT required; the AppHost uses the Aspire.AppHost.Sdk NuGet package

FIRST RUN
  1. Open MEMS.sln and set MEMS.AppHost as the startup project (it is the only one you run).
  2. Press F5. The Aspire dashboard opens (https://localhost:17175). Wait until sql, api, admin and public are Running.
  3. The API creates the database and seeds it on first start.

  (Recommended, one time) create the EF Core migration, as required by the assignment:
      dotnet tool install --global dotnet-ef        (if not installed)
      dotnet ef migrations add InitialCreate --project MEMS.Data --startup-project MEMS.Api --output-dir Migrations
  Until a migration exists the API falls back to EnsureCreated (it logs a warning). SQL Server in the AppHost has no
  persistent volume, so the database is rebuilt on every run and adding the migration later needs no cleanup.

CERTIFICATE WARNINGS / "election service is not reachable"
  Every https address uses the ASP.NET Core development certificate. If it is not trusted, browsers warn on each page and the
  public site's calls to the API (a different port) are blocked, which shows as "The election service is not reachable".
  Fix once, then restart the browser and the AppHost:
      dotnet dev-certs https --clean
      dotnet dev-certs https --trust
  (Windows/macOS show a trust prompt, accept it. On Linux trust the certificate in your browser/OS store.)
  No-certificate alternative: start MEMS.AppHost with the "http" profile and use
      Public http://localhost:5102   Admin http://localhost:5103/admin   API http://localhost:5101/swagger
  The public site automatically calls the API over http when it is opened over http.

URLS (fixed ports)
  Aspire dashboard : https://localhost:17175
  API + Swagger    : https://localhost:7101/swagger
  Admin site       : https://localhost:7103/admin
  Public sites     : https://localhost:7102/            (list of municipalities)
                     https://localhost:7102/burnaby  /surrey  /richmond  /pitt-meadows

TEST ACCOUNTS (password for all: P@$$w0rd)
  super           Super Admin                    all municipalities
  burnabyadmin    Municipality Admin             Burnaby only
  surreyadmin     Municipality Admin             Surrey only
  richmondadmin   Municipality Admin             Richmond only
  pm              Municipality Admin             Pitt Meadows only (from Assignment 1)

SPECIAL INSTRUCTIONS FOR TESTING
  - Tenant isolation (admin): sign in as burnabyadmin. Only Burnaby appears; the "Municipalities" menu is absent;
    opening /admin/tenants shows "not authorized"; opening /admin/candidates/<id of a Surrey candidate> says not found.
  - Tenant isolation (API): in Swagger, POST /api/auth/login as burnabyadmin, press Authorize, then call
    GET /api/surrey/candidates -> 403. Without logging in, GET /api/surrey/candidates -> 200 (public data of that tenant).
    GET /api/burnaby/candidates/<id of a Surrey candidate> -> 404.
  - Voting guide: on https://localhost:7102/burnaby/candidates add TWO mayors and candidates from Surrey too, then open
    "My voting guide" and press Print. Close the tab: the list is gone (Session Storage).
  - Election privacy: in the browser dev tools Network tab, adding a candidate to the guide makes NO request.
  - Add a municipality in /admin/tenants/new (as super) with slug "delta": https://localhost:7102/delta works at once,
    no new application is needed.

WHAT HAS NOT BEEN COMPLETED
  <list honestly, e.g. "No automated tests", "Aspire browser telemetry for the WASM site">

MAJOR CHALLENGES
  <fill in: e.g. Blazor WASM cannot use Aspire service discovery directly (runs in the browser), composite FK for tenant integrity>

KNOWN LIMITATION
  The public Blazor WASM site reads the API address from wwwroot/appsettings.json (ApiBaseUrl). The API port is fixed at 7101
  in launchSettings.json. If you change that port, change ApiBaseUrl and Cors:AllowedOrigins in MEMS.Api/appsettings.json.

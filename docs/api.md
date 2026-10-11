# Municipal Elections Web API

ASP.NET Core Web API (.NET 10, EF Core, SQLite, Identity, JWT) inside the web app in `web/`, consumed by the
SwiftUI app "Municipal Elections Companion" (see [../ios/README.md](../ios/README.md)).

- **Deployed API (Azure App Service):** https://municipalelections-api-mskim9097-edg2grfychegfwes.canadacentral-01.azurewebsites.net
- **GitHub repository:** https://github.com/mskim9097/comp4976_assignment1
- **Swagger UI (try the API in a browser):** https://municipalelections-api-mskim9097-edg2grfychegfwes.canadacentral-01.azurewebsites.net/swagger
  (`/api` redirects there; the site root `/` is the MVC home page; locally it is http://localhost:5276/swagger)
- The API and the MVC site are one project (`web/`, solution `MunicipalElections.sln`) with one SQLite database, see [erd.md](erd.md)

The Azure app runs on the Free F1 plan, so the first request after a quiet period can take
10-30 seconds while the app wakes up.

## Try the deployed API

```bash
HOST=https://municipalelections-api-mskim9097-edg2grfychegfwes.canadacentral-01.azurewebsites.net
curl $HOST/api/municipalities
curl "$HOST/api/candidates/search?name=an"
```

Create an account with `POST /api/auth/register` (the iOS app does this on its Create Account screen), then call
`POST /api/auth/login` and send the returned token as `Authorization: Bearer <token>`.
The seeded admins can also log in here by email (`super@example.com`, `pm@example.com`, password `P@$$w0rd`).
Sample requests for every endpoint are in `web/MunicipalElections.Web.http`.
In Swagger UI, call `POST /api/auth/login`, copy the token, press **Authorize** and paste it to try the
endpoints marked with a lock.

## Endpoints

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| POST | `/api/auth/register` | no | Create an account (`name`, `email`, `password` min 6) |
| POST | `/api/auth/login` | no | Returns `{ "token": "..." }` |
| GET | `/api/auth/me` | JWT | Logged-in user (extra, used by the Profile screen) |
| GET | `/api/municipalities`, `/api/municipalities/{id}` | no | Municipalities |
| GET | `/api/positions`, `/api/positions/{id}` | no | Positions (`?municipalityId=`) |
| GET | `/api/candidates`, `/api/candidates/{id}` | no | Candidates (`?municipalityId=`, `?positionId=`) |
| GET | `/api/candidates/search?name=` | no | Case-insensitive name search (also takes the filters above) |
| GET | `/api/savedcandidates` | JWT | The user's voting guide |
| POST | `/api/savedcandidates` | JWT | Add a candidate (`{ "candidateId": 1 }`) |
| DELETE | `/api/savedcandidates/{id}` | JWT | Remove an entry (`id` = `savedCandidateId`) |

Notes for API clients:
- `electionDate` is `yyyy-MM-dd`; `dateSaved` is ISO-8601 UTC (`2026-10-10T22:37:03Z`).
- `logoUrl` and `photoUrl` are absolute `https://` URLs.
- Errors use the standard problem-details JSON (`title`, and `errors` for validation failures).
  Status codes: 400 validation, 401 missing/invalid token or wrong login, 404 not found, 409 already saved.

## Database and seed data

One SQLite database (`app.db`) shared with the MVC site; EF Core Code First (`web/Data/Migrations`).
On startup the app applies the migrations and, if the database is empty, loads sample data: 3
municipalities (Pitt Meadows, Maple Ridge, Burnaby), 9 positions (Mayor, Councillor, School Trustee for
each) and 18 fictional candidates, plus the roles and the admin users `super` (Super Admin) and `pm`
(Pitt Meadows Admin), password `P@$$w0rd`. Placeholder images are in `web/wwwroot/images`.

Tables: `AspNetUsers` (Identity, plus `Name`), `AspNetRoles`, `AspNetUserRoles`, `Municipalities`,
`Positions`, `Candidates`, `SavedCandidates` (unique per user and candidate). The full diagram is in
[erd.md](erd.md).

## Run locally

```bash
cp web/appsettings.Development.example.json web/appsettings.Development.json
# set Jwt:Key in that file to a random string of at least 32 characters:  openssl rand -base64 48
dotnet run --project web --launch-profile http     # http://localhost:5276 (site) and /swagger (API)
```

`appsettings.Development.json` is git-ignored (connection string and JWT key).

## Deployment (Azure App Service, Linux, .NET 10)

- App settings on Azure: `Jwt__Key` (random secret) and
  `ConnectionStrings__DefaultConnection` = `DataSource=/home/data/app.db;Cache=Shared`
  (`/home` is persistent storage; the app creates the folder on startup).
- Startup command (Configuration > Stack settings): `dotnet MunicipalElections.Web.dll`. Deployments only add
  and overwrite files, so files of an older project name can stay in `/home/site/wwwroot`; with two
  `*.runtimeconfig.json` files Azure cannot tell which app to run and shows its default welcome page instead.
- GitHub Actions workflow `.github/workflows/main_municipalelections-api-mskim9097.yml`
  publishes `web/` and deploys it when its code changes are pushed to `main`.
  Changes under `docs/` or `ios/` do not redeploy it.

# COMP4977 - Municipal Elections API and iOS App

ASP.NET Core Web API (.NET 10, EF Core, SQLite, Identity, JWT) in `api/`, consumed by the
SwiftUI app "Municipal Elections Companion" in `ios/`.

- **Deployed API (Azure App Service):** https://municipalelections-api-mskim9097-edg2grfychegfwes.canadacentral-01.azurewebsites.net
- **GitHub repository:** https://github.com/mskim9097/comp4976_assignment1 (this course lives in `comp4977/`)
- Solution: `MunicipalElections.Api.sln`

The Azure app runs on the Free F1 plan, so the first request after a quiet period can take
10-30 seconds while the app wakes up.

## Try the deployed API

```bash
HOST=https://municipalelections-api-mskim9097-edg2grfychegfwes.canadacentral-01.azurewebsites.net
curl $HOST/api/municipalities
curl "$HOST/api/candidates/search?name=an"
```

There are no seeded users: create one with `POST /api/auth/register`, then call
`POST /api/auth/login` and send the returned token as `Authorization: Bearer <token>`.
Sample requests for every endpoint are in `api/MunicipalElections.Api.http`.

## iOS app

SwiftUI, MVVM, async/await, `URLSession`, `Codable`, `NavigationStack`. Project:
`ios/MunicipalElectionsCompanion/MunicipalElectionsCompanion.xcodeproj` (Xcode, iOS simulator).

Run: open the project in Xcode, choose an iPhone simulator, press Run. The app calls the deployed
Azure API. To use a local API instead, add the environment variable `API_BASE_URL`
(for example `http://localhost:5214`) under Product > Scheme > Edit Scheme > Run > Arguments.

Screens: Log In, Create Account, Municipalities (logo, name, election date), Candidates (photo, name,
position, municipality; search by name and filter by municipality/position), Candidate Details (biography,
website, email, Add to / Remove from Voting Guide), My Voting Guide (view, swipe to remove) and Profile
(logged-in user, log out).

To try it, create an account on the Create Account screen. The login token is kept in the Keychain.

```
ios/MunicipalElectionsCompanion/MunicipalElectionsCompanion/
  Models/        Codable models (Municipality, Position, Candidate, SavedCandidate, auth)
  Services/      APIClient (URLSession + async/await), Session, TokenStore (Keychain), Validation
  ViewModels/    one @Observable view model per screen
  Views/         SwiftUI screens and small components
```

Form checks: required fields, valid email format, password of at least 6 characters, matching password
confirmation. Server and network errors are shown as readable messages; an expired token logs the user out.

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

SQLite (`api.db`), EF Core Code First (`api/Data/Migrations`). On startup the app applies the
migrations and, if the database is empty, loads sample data: 3 municipalities (Pitt Meadows,
Maple Ridge, Burnaby), 9 positions (Mayor, Councillor, School Trustee for each) and 18
fictional candidates. Placeholder images are in `api/wwwroot/images`.

Tables: `AspNetUsers` (Identity, plus `Name`), `Municipalities`, `Positions`, `Candidates`
(has both `PositionId` and `MunicipalityId`), `SavedCandidates` (unique per user and candidate).

## Run locally

```bash
cp api/appsettings.Development.example.json api/appsettings.Development.json
# set Jwt:Key in that file to a random string of at least 32 characters:  openssl rand -base64 48
dotnet run --project api --launch-profile http     # http://localhost:5214
```

`appsettings.Development.json` is git-ignored (connection string and JWT key).

## Deployment (Azure App Service, Linux, .NET 10)

- App settings on Azure: `Jwt__Key` (random secret) and
  `ConnectionStrings__DefaultConnection` = `DataSource=/home/data/api.db;Cache=Shared`
  (`/home` is persistent storage; the app creates the folder on startup).
- GitHub Actions workflow `.github/workflows/main_municipalelections-api-mskim9097.yml`
  publishes `comp4977/api` and deploys it when API code changes are pushed to `main`.
  Changes under `comp4976/` or `comp4977/ios/` do not redeploy the API.

# MEMS Web API

ASP.NET Core Web API (.NET 10, EF Core, JWT) in `src/MEMS.Api`. It uses SQL Server with Aspire and a SQLite file on Azure App Service (`Database__Provider=Sqlite`). It serves the Blazor public site and the iOS app. Swagger UI: `/swagger` (OpenAPI document: `/openapi/v1.json`). Health checks (`/health`, `/alive`) are mapped only in the Development environment, as in the Aspire template.

Local address: `https://localhost:7261` (started by the Aspire AppHost).

## Tenants

A municipality is a tenant and is named by its **slug** (`burnaby`, `surrey`, `richmond`, `pitt-meadows`).

- `GET /api/burnaby/candidates` returns only the candidates of Burnaby.
- A Municipality Admin who asks for another municipality gets **403** (and a warning in the log). An unknown slug is **404**. A candidate or position id of another municipality is **404**, even when the id exists.
- Reading is public. Creating, changing, deleting and uploading need a login as Super Admin or as Municipality Admin of that municipality. Managing municipalities needs a Super Admin.
- The mobile app uses `?city=Burnaby` (name or slug) instead of the slug in the route; the same isolation applies.

## Authentication

`POST /api/auth/login` returns a JWT; send it as `Authorization: Bearer <token>`. The login accepts the user name (`burnabyadmin`) or the email address. After 5 wrong passwords an account is locked for 5 minutes. People who register in the app (`POST /api/auth/register`) have no role: they can read public data but cannot change anything.

## Endpoints

| Method | Route | Access | Description |
|---|---|---|---|
| POST | `/api/auth/register` | public | Create an app account (`name`, `email`, `password` min 6) |
| POST | `/api/auth/login` | public | `{ "email": "<user name or email>", "password": "..." }` returns `{ "token": "..." }` |
| GET | `/api/auth/me` | login | Name, email, roles, tenant of the user |
| GET | `/api/municipalities`, `/{id}`, `/{slug}` | public | Municipalities (a Municipality Admin sees only their own) |
| POST | `/api/municipalities` | Super Admin | Create a municipality |
| PUT, DELETE | `/api/municipalities/{id}` | Super Admin | Update, delete (also deletes its positions and candidates; refused while it has administrators) |
| POST | `/api/municipalities/{id}/logo` | Super Admin | Upload a logo (`multipart/form-data`, field `file`) |
| GET | `/api/{slug}/positions`, `/{id}` | public | Positions of the municipality |
| POST, PUT, DELETE | `/api/{slug}/positions[/{id}]` | admin of the tenant | Create, update, delete a position |
| GET | `/api/{slug}/candidates?name=&positionId=` | public | Candidates (search by name ignoring case, filter by position) |
| GET | `/api/{slug}/candidates/{id}` | public | One candidate |
| POST, PUT, DELETE | `/api/{slug}/candidates[/{id}]` | admin of the tenant | Create, update, delete a candidate |
| POST | `/api/{slug}/candidates/{id}/photo` | admin of the tenant | Upload a photo |
| GET | `/api/candidates?city=`, `/api/candidates/search?city=&name=`, `/api/candidates/{id}?city=` | public | Candidates of a city (mobile app) |
| GET | `/api/offices?city=`, `/api/offices/{id}?city=` | public | Elected positions of a city (also `/api/positions`, the Assignment 1 name) |
| GET | `/api/elections?city=` | public | Election date and offices of a city |

## Examples

### Log in
```http
POST /api/auth/login
Content-Type: application/json

{ "email": "burnabyadmin", "password": "P@$$w0rd" }
```
```json
{ "token": "eyJhbGciOiJIUzI1NiIsInR5..." }
```

### Who am I
```http
GET /api/auth/me
Authorization: Bearer <token>
```
```json
{ "name": "Burnaby Admin", "email": "burnabyadmin@example.com", "roles": ["Municipality Admin"], "municipalityId": 1 }
```

### A municipality
```http
GET /api/municipalities/burnaby
```
```json
{
  "municipalityId": 1,
  "name": "Burnaby",
  "slug": "burnaby",
  "province": "British Columbia",
  "electionDate": "2026-10-17",
  "description": "A large city in Metro Vancouver with several town centres and Burnaby Mountain.",
  "logoUrl": "https://localhost:7261/images/logos/burnaby.png",
  "themeColor": "#1f4e8c",
  "welcomeMessage": "Welcome to the Burnaby election guide. Meet the candidates and plan your vote."
}
```

### One candidate of the municipality
```http
GET /api/burnaby/candidates/10
```
```json
{
  "candidateId": 10,
  "name": "Mei-Ling Tran",
  "biography": "Community health director running on safer streets, transit and local small business.",
  "photoUrl": "https://localhost:7261/images/candidates/c10.png",
  "website": "https://example.com/meiling.tran",
  "email": "meiling.tran@example.com",
  "positionId": 4,
  "positionName": "Mayor",
  "municipalityId": 1,
  "municipalityName": "Burnaby",
  "createdDate": "2026-11-13T05:27:03.50826"
}
```

### Create a candidate
```http
POST /api/burnaby/candidates
Authorization: Bearer <token of burnabyadmin or super>
Content-Type: application/json

{ "name": "Jane Example", "positionId": 5, "biography": "Teacher and volunteer.", "email": "jane@example.com", "website": "https://example.com/jane" }
```
Response `201 Created` with the candidate (same shape as above; `municipalityId` and `municipalityName` come from the position).

### Election of a city (mobile app)
```http
GET /api/elections?city=Burnaby
```
```json
{
  "municipalityId": 1,
  "city": "Burnaby",
  "slug": "burnaby",
  "electionDate": "2026-10-17",
  "province": "British Columbia",
  "offices": [
    { "positionId": 4, "name": "Mayor", "seatsAvailable": 1, "municipalityId": 1, "municipalityName": "Burnaby" },
    { "positionId": 5, "name": "Councillor", "seatsAvailable": 8, "municipalityId": 1, "municipalityName": "Burnaby" },
    { "positionId": 6, "name": "School Trustee", "seatsAvailable": 2, "municipalityId": 1, "municipalityName": "Burnaby" }
  ]
}
```

## Error responses

Errors use the standard problem details format (`title`, `status`, and `errors` for validation). The text never contains stack traces, SQL or file paths; unexpected errors are logged on the server and answered with a general 500 message.

| Status | When | Example body |
|---|---|---|
| 400 | Validation failed (required, email, length, seats 1-100, slug format, province list, image type or size) | `{ "title": "One or more validation errors occurred.", "status": 400, "errors": { "Name": ["Candidate name is required."], "Email": ["Enter a valid email address."] } }` |
| 401 | No or invalid token, wrong password | `{ "title": "Unauthorized", "status": 401 }` |
| 403 | Logged in but not allowed: another municipality's administrator, a user without a role, a Municipality Admin managing municipalities | `{ "title": "You can only access your own municipality.", "status": 403 }` |
| 404 | Unknown municipality, or a row of another municipality | `{ "title": "Candidate not found.", "status": 404 }` |
| 409 | Duplicate (municipality name or slug, position name, candidate name in a position), municipality that still has administrators | `{ "title": "A candidate named 'Jane Example' already runs for Councillor.", "status": 409 }` |
| 500 | Unexpected error | `{ "title": "Something went wrong on the server. Please try again later.", "status": 500 }` |

## Validation

| Layer | Where |
|---|---|
| Client (Blazor) | `DataAnnotations` on the request classes in `MEMS.Shared` (`EditForm` in the Admin app) |
| API | The same attributes on model binding (`[ApiController]`) plus the checks in the services (case-insensitive duplicates, http/https websites, province list, reserved slugs, image content) |
| Database | Required columns, lengths, unique indexes (municipality name and slug, position name per municipality), `CHECK` on seats, foreign keys including the composite key that keeps a candidate in the tenant of its position |

## Run

```bash
dotnet run --project src/MEMS.AppHost       # starts SQL Server, the Api, Admin and Public
```

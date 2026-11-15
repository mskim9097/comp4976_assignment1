# MEMS API documentation

Interactive documentation: **https://localhost:7101/swagger** (OpenAPI JSON at `/openapi/v1.json`).
Base URL: `https://localhost:7101`. Tenant routes contain the municipality slug: `/api/burnaby/candidates`.

## Authentication
`POST /api/auth/login`
```json
{ "userName": "burnabyadmin", "password": "P@$$w0rd" }
```
200 response
```json
{ "token": "eyJhbGciOi...", "expiresUtc": "2026-11-20T18:30:00Z", "userName": "burnabyadmin",
  "role": "Municipality Admin", "municipalityId": 2 }
```
Send `Authorization: Bearer <token>` on administrator endpoints. 401: `{"title":"Invalid user name or password.","status":401}`.

## Access rules
| Caller | Reads | Writes |
|---|---|---|
| Anonymous | any tenant's public data (what that tenant's public site shows) | never |
| Municipality Admin | own tenant only (other tenant -> 403) | own tenant only |
| Super Admin | everything | everything, plus municipalities |

## Endpoints
| Method | Route | Who | Notes |
|---|---|---|---|
| GET | `/api/municipalities` | public (own tenant only if a Municipality Admin is signed in) | list |
| GET | `/api/municipalities/{id}` | public | one |
| POST | `/api/municipalities` | Super Admin | create tenant (name, slug, province, electionDate, description, themeColor) |
| PUT | `/api/municipalities/{id}` | Super Admin | update |
| DELETE | `/api/municipalities/{id}` | Super Admin | removes the tenant with positions, candidates and its admins |
| GET / PUT | `/api/municipalities/{id}/logo` | public / Super Admin | image bytes / multipart `file` |
| GET | `/api/{tenant}/municipality` | public | the tenant's details |
| GET | `/api/{tenant}/positions` , `/{id}` | public | |
| POST / PUT / DELETE | `/api/{tenant}/positions[/{id}]` | tenant admin | |
| GET | `/api/{tenant}/candidates?search=&positionId=` , `/{id}` | public | |
| POST / PUT / DELETE | `/api/{tenant}/candidates[/{id}]` | tenant admin | |
| GET / PUT | `/api/{tenant}/candidates/{id}/photo` | public / tenant admin | image bytes / multipart `file` (PNG, JPEG, GIF, WebP, max 2 MB) |

## Examples
`GET /api/burnaby/candidates?search=victor`
```json
[ { "candidateId": 9, "name": "Victor Salazar", "profile": "Union representative...", "website": "https://example.com/victor.salazar",
    "email": "victor.salazar@example.com", "positionId": 4, "positionName": "Mayor", "municipalityId": 2,
    "photoUrl": "/api/burnaby/candidates/9/photo?v=1", "createdUtc": "2026-11-16T01:02:03Z" } ]
```
`POST /api/burnaby/positions` (as burnabyadmin)
```json
{ "name": "Parks Commissioner", "seatsAvailable": 2 }
```
201 -> `{ "positionId": 13, "name": "Parks Commissioner", "seatsAvailable": 2, "municipalityId": 2, "candidateCount": 0 }`

## Error responses (RFC 7807 problem details, never any stack trace)
| Status | When | Body |
|---|---|---|
| 400 | validation | `{"title":"One or more validation errors occurred.","status":400,"errors":{"Email":["Enter a valid email address."]}}` |
| 401 | missing / invalid token | empty or problem details |
| 403 | tenant violation or insufficient role | `{"title":"Forbidden","status":403,"detail":"You do not have access to this municipality."}` |
| 404 | unknown tenant, or an id that is not in that tenant | `{"title":"Unknown municipality","status":404,"detail":"There is no municipality 'xyz'."}` |
| 409 | duplicate slug / position name, position still has candidates | `{"title":"Conflict","status":409,"detail":"A position with this name already exists for the municipality."}` |
| 500 | unexpected | `{"title":"Something went wrong","status":500,"traceId":"00-..."}` (details only in the log) |

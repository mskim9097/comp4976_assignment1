---
name: api-contract-ios
description: The JSON Web API is a contract with the iOS app. Use before changing Dtos, API controllers, entity column names or authentication.
---
# Web API contract (iOS app)

The SwiftUI app in `ios/` decodes the JSON of `web/Controllers/Api/*` into `Codable` models. Changing a field name or type breaks the app.

- Endpoints: `POST /api/auth/register`, `POST /api/auth/login` (returns `{ "token": "..." }`), `GET /api/auth/me`; `GET /api/municipalities[/{id}]`, `GET /api/positions[/{id}]`, `GET /api/candidates[/{id}]`, `GET /api/candidates/search?name=`; `GET/POST/DELETE /api/savedcandidates` (JWT). Details: `docs/api.md`, Swagger at `/swagger`.
- Responses are built in `web/Dtos/` from the entities. Mapping: `Candidate.Profile` -> `biography`, `PhotoFileName`/`LogoFileName` -> absolute `photoUrl`/`logoUrl` (`Extensions/ImageUrls.cs`), municipality of a candidate = the municipality of its position. `electionDate` is `yyyy-MM-dd`; `dateSaved` is ISO-8601 UTC without fractions.
- The API uses JWT bearer authentication (`[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]`), the MVC site uses the Identity cookie. Do not mix them.
- Errors are problem-details JSON (`title`, `errors`); keep 400 / 401 / 404 / 409 meanings.
- After a change run the app and check `/swagger` and the iOS screens (login, municipalities, candidates with search and filters, details, voting guide, profile). If a field must change, change the DTO, the Swift model in `ios/.../Models/` and `docs/api.md` together.

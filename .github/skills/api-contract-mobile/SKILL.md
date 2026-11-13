---
name: api-contract-mobile
description: The Web API is also the backend of the iOS app. Use before changing DTOs, API routes, authentication or seed data.
---
# Web API contract (iOS app and Blazor)

The iOS app (`ios/`) and the Blazor Public site decode the JSON of `MEMS.Api`. Changing a field name or type breaks them.

- DTOs live in `src/MEMS.Shared/Dtos`. Dates are date-only (`electionDate: "2026-10-17"`); image URLs are absolute.
- Mobile endpoints (city given in `?city=`, name or slug): `GET /api/candidates`, `/api/candidates/search`, `/api/candidates/{id}`, `/api/offices`, `/api/offices/{id}`, `/api/elections`. Authentication: `POST /api/auth/register`, `POST /api/auth/login` (returns `{ "token": "..." }`), `GET /api/auth/me`.
- Tenant endpoints: `/api/{slug}/positions`, `/api/{slug}/candidates` (GET, GET by id, POST, PUT, DELETE, photo upload) and `/api/municipalities`.
- Errors are problem details (`title`, `errors` for validation); keep the meaning of 400, 401, 403, 404 and 409.
- Seats per position (`seatsAvailable`) drive the limits of a voter's choices in the apps; keep them in the DTOs.
- After a change: run the Api, check `/swagger`, update `docs/api.md`, the Swift models in `ios/.../Models/` and the Swift calls together.

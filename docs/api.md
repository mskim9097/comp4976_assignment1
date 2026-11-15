# MEMS Web API

The ASP.NET Core Web API is in `web/MEMS.Api/`. Aspire starts it with the SQL Server resource from `web/MEMS.AppHost/`. Use the Aspire dashboard to find the local API URL and open Swagger at `/swagger` in Development.

## Endpoints

| Method | Route | Description |
|---|---|---|
| GET | `/api/municipalities` | List municipalities |
| GET | `/api/{slug}/municipality` | Get a municipality by slug |
| GET | `/api/{id}` | Get a municipality by numeric ID |
| POST | `/api/municipalities` | Create a municipality |
| PUT | `/api/municipalities/{id}` | Update a municipality |
| DELETE | `/api/municipalities/{id}` | Delete a municipality |
| GET | `/api/{slug}/positions` | List positions for a municipality |
| GET | `/api/positions/{id}` | Get a position |
| POST | `/api/positions` | Create a position |
| PUT | `/api/positions/{id}` | Update a position |
| DELETE | `/api/positions/{id}` | Delete a position |
| GET | `/api/{slug}/candidates` | List candidates for a municipality |
| GET | `/api/candidates/{id}` | Get a candidate |
| POST | `/api/candidates` | Create a candidate |
| PUT | `/api/candidates/{id}` | Update a candidate |
| DELETE | `/api/candidates/{id}` | Delete a candidate |
| GET | `/health` | API health check |

Swagger is enabled when the API runs in Development. The API uses the shared DTOs in `web/MEMS.Shared/`; its database schema and seed data are maintained in `web/MEMS.Data/`.

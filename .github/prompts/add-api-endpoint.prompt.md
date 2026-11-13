---
description: Add a tenant-scoped Web API endpoint with validation, error handling and OpenAPI
agent: agent
---
Add the endpoint `${input:route}` (${input:purpose}) to `src/MEMS.Api`.

- Follow `CandidatesController`: `[ApiController]`, `[TenantScoped]`, route `api/{slug}/...`, thin actions that call a service and map the entity to a DTO with `DtoMapping`.
- Reading is public; create, update, delete and uploads need `[Authorize(Roles = "Super Admin,Municipality Admin")]`. The tenant check is done by `TenantScopeFilter`; do not repeat it in the action and do not take a tenant id from the body.
- Request classes live in `MEMS.Shared` with validation attributes. Do not return entities.
- Errors: let the services throw `NotFoundException`, `ConflictException`, `ForbiddenException`, `RuleViolationException`; `ApiExceptionHandler` answers with problem details.
- Swagger: check that the endpoint appears, shows the lock when it needs a login, and works with "Try it out".
- Add the endpoint with a request and a response example to `docs/api.md`.
- Check by hand (or with curl): anonymous, Municipality Admin of this tenant, Municipality Admin of another tenant (403), Super Admin, unknown slug (404), invalid body (400).

---
description: Review code for tenant isolation, authorization and validation gaps
agent: agent
---
Review the files I name (default: everything under `src/`) as a security reviewer for a multi-tenant application.

Check for:
- A query on `Municipality`, `Position`, `Candidate` that bypasses the filter (`IgnoreQueryFilters`, raw SQL). Only `MunicipalityService.ResolveAsync` and the uniqueness checks may do it, and they must not return data to the user.
- Ids, slugs or tenant ids taken from a URL, form or body and used without loading the row through a filtered service.
- Endpoints without `[Authorize]` that change data; endpoints that return entities instead of DTOs; missing `[TenantScoped]` on a route with `{slug}`.
- A Blazor page that injects the DbContext or a service directly instead of `AdminSession.RunAsync`.
- Validation only on the client; uploads that are not checked by `ImageFileStore`; error messages that expose exceptions, SQL or paths.
- `MEMS.Public` code that references `MEMS.Data`, sends the voter's choices anywhere, or stores them outside `sessionStorage`.

Report findings as a short list with file and line, severity and a suggested fix. Do not change code unless I ask.

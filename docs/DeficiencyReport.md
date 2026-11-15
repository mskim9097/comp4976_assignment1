# Assignment #1 deficiency resolution report

The first four rows are the problems found in the Assignment 1 code. **Add the rows from your instructor's marking
feedback below them** (the assignment requires every identified issue to be listed).

| Issue | Original problem | Resolution |
|---|---|---|
| Municipality admin visibility | A Pitt Meadows admin could still reach other municipalities' data through the JSON API, and an admin whose group matched no municipality was treated as a normal visitor and saw everything | A1 patch: `AdminAccess.GetPublicScopeAsync` treats any role-holder as an administrator limited to their groups (empty list = nothing); the API controllers use the same scope. A2: `TenantScope`, `TenantResolutionFilter`, `ForTenant()` and a composite foreign key enforce it in the API, service layer and database; the admin app is locked to the user's tenant claim |
| Visitors limited to one city | `preferred-rules.js` refused a candidate from a second municipality | Rule removed. A2 stores each item with its municipality and election date; the printed guide groups by municipality |
| Only one mayor | The same script capped choices per position at the number of seats | Rule removed (also in the saved-candidates API). Any number of candidates can be chosen per position |
| Admins could shortlist | Candidate-list tools were shown to administrators (and the API accepted their saves) | A1 patch: tools hidden for admins and `POST /api/savedcandidates` returns 403 for any role-holder. A2: the public site has no administrator features and ballots never leave the browser |
| Image upload | Files written to `wwwroot` on the web server | Signature, size and type checks shared by Admin and API; images stored in SQL Server so every project sees the same data |
| Validation | Server and MVC client validation only | Three layers: Blazor `DataAnnotationsValidator`, `[ApiController]` model validation plus service checks, EF constraints (lengths, unique indexes, check constraint, foreign keys) |
| Error handling | Developer exception page / generic error view | `IExceptionHandler` in the API (problem details + trace id), `/error` page and `ErrorBoundary` in the apps, structured `ILogger` messages in the Aspire dashboard |
| Secrets | JWT key in `appsettings.Development.json` | Aspire secret parameter `jwt-key`; SQL connection string owned by Aspire |
| Architecture | Controllers contained business rules | One service layer in `MEMS.Data` used by both the API and the Admin app |

<!-- Example rows to copy for instructor feedback:
| Missing validation | Candidate website accepted any text | Added [HttpUrl] on the DTO, checked again in CandidateService |
-->

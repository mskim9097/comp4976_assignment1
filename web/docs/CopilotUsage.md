# How GitHub Copilot was used

Repository files that guide Copilot:
- `.github/copilot-instructions.md`: always-on rules (architecture, tenant isolation, privacy, validation, style)
- `.github/prompts/*.prompt.md`: reusable prompts (`/add-tenant-entity`, `/review-tenant-isolation`)
- `.github/skills/mems-tenant-isolation/SKILL.md`: step-by-step skill for anything that touches tenant data

## Example prompts
- "Using the tenant rules in copilot-instructions.md, add a `Notice` entity owned by a municipality with list/create/delete in the service layer, API controller and Blazor admin page."
- "Review `CandidateService` and find any query that is not restricted with `ForTenant`."
- "Write a Blazor `EditForm` for `PositionRequest` with `DataAnnotationsValidator` and server error display."

## Productivity improvements
<fill in: e.g. boilerplate for DTOs, repeated CRUD pages, XML docs, test data>

## Problems encountered
<fill in: e.g. Copilot suggested loading a DbContext once per circuit (not thread safe) so we used OwningComponentBase; it suggested storing ballots in the API, which breaks the election privacy rule>

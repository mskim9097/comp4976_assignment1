---
mode: agent
description: Add a new tenant-owned entity end to end
---
Add the entity `${input:entity}` owned by a municipality.
Follow .github/copilot-instructions.md and the skill in .github/skills/mems-tenant-isolation/SKILL.md:
entity in MEMS.Data, DbContext mapping, DTOs + request validation in MEMS.Shared, service with ForTenant and TenantScope,
API controller under /api/{tenant}/..., Blazor Server admin pages, and a note for the EF migration command.

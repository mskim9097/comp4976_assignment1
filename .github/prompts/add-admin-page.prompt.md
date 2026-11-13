---
description: Add a Blazor Server admin page that works inside the tenant of the logged-in administrator
agent: agent
---
Add the admin page `${input:page}` (${input:purpose}) to `src/MEMS.Admin/Components/Pages/`.

- Authorize with `@attribute [Authorize(Roles = AdminRoles.Any)]` (or `AdminRoles.Super` for Super Admin only pages).
- Get data only through `Admin.RunAsync(s => s.<Service>...)` and show errors with `Admin.TryAsync` and `<Alerts Error="@error" />`. Never inject `MemsDbContext` or a service into the page.
- Forms: `EditForm` with `DataAnnotationsValidator`, the request classes from `MEMS.Shared`, `ValidationMessage` next to every field, Bootstrap classes.
- Deletes ask for a confirmation (`confirm`) and reload the list.
- Uploads use `InputFile` with the `accept` list and `Limits.ImageMaxBytes`; the service checks the file again.
- Remember: string parameters of components need `@` (`Error="@error"`), and an `AuthorizeView` inside an `EditForm` needs its own `Context` name.
- Test it in a browser as Super Admin and as a Municipality Admin: the second one must not see or open rows of another municipality.
